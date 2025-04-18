﻿using API_PAYMENT.Application.Enums;
using API_PAYMENT.Application.Payment;
using API_PAYMENT.Configuration;
using API_PAYMENT.Domain.Payment;
using API_PAYMENT.Endpoints;
using API_PAYMENT.Infrastructure;
using Mapster;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Steeltoe.Extensions.Configuration.ConfigServer;
using System.Diagnostics;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Configuration.Sources.Clear();

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Configuration.AddConfigServer(builder.Environment);

builder.Services.AddHealthChecks();

var isInDevelopment = Convert.ToBoolean(builder.Configuration["IsInDevelopment"]);

builder.WebHost.UseUrls("http://+:4080");
builder.Services.AddHttpContextAccessor();

#region AUTHENTICATION

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters.ValidateIssuerSigningKey = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.ValidateIssuer = false;
    });

builder.Services.AddAuthorization();

#endregion

#region LOGS

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);
});

#endregion

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, PaymentSerializerContext.Default);
});

#region TRACING

var jaegerserver = builder.Configuration.GetValue<string>("JaegerServer") ?? throw new Exception("No se ha configurado la informaci�n del 'Jaeger Server' correctamente");

builder.Services.AddOpenTelemetry()
    .WithTracing(opt => opt
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("API_PAYMENT"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(opcion =>
        {
            opcion.Endpoint = new Uri(jaegerserver);
        })
    );

#endregion

#region CORS

builder.Services.AddCors(options =>
{
    var corsOriginAllowed = builder.Configuration.GetSection("AllowedOrigins").Get<List<string>>();

    options.AddPolicy("CorsPolicy",
        builder => builder
        .WithOrigins(corsOriginAllowed != null ? corsOriginAllowed.ToArray() : ["*"])
        .AllowAnyMethod()
        .AllowAnyHeader()
        );
});

#endregion

#region METRICS

builder.Services.AddOpenTelemetry()
    .WithMetrics(metricsBuilder => metricsBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("API_PAYMENT"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter("Microsoft.AspNetCore")
        .AddPrometheusExporter());

#endregion

#region MAPPER

builder.Services.AddMapster();

TypeAdapterConfig<PaymentDto, Payment>
    .NewConfig()
    .Map(dest => dest.IdPago, src => src.IdPago)
    .Map(dest => dest.FechaPago, src => src.FechaPago)
    .Map(dest => dest.IdCliente, src => src.IdCliente)
    .Map(dest => dest.IdPedido, src => src.IdPedido)
    .Map(dest => dest.FormaPago, src => $"{(int)src.FormaPago}")
    .Map(dest => dest.MontoPago, src => src.MontoPago);

TypeAdapterConfig<Payment, PaymentDto>
    .NewConfig()
    .Map(dest => dest.IdPago, src => src.IdPago)
    .Map(dest => dest.FechaPago, src => src.FechaPago)
    .Map(dest => dest.IdCliente, src => src.IdCliente)
    .Map(dest => dest.IdPedido, src => src.IdPedido)
    .Map(dest => dest.FormaPago, src => $"{(FormaPagoEnum)src.FormaPago}")
    .Map(dest => dest.MontoPago, src => src.MontoPago);

#endregion

builder.Services.AddScoped<PaymentHandler>();

#region DATABASE

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings")
);

builder.Services.AddSingleton<IMongoClient>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

    var mongoClientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
    mongoClientSettings.ClusterConfigurator = cb =>
    {
        cb.Subscribe<MongoDB.Driver.Core.Events.CommandStartedEvent>(e =>
        {
            var logger = loggerFactory.CreateLogger("MongoDB.Driver");
            logger.LogInformation($"MongoDB Command Started: {e.CommandName} - {e.Command.ToJson()}");
        });

        cb.Subscribe<MongoDB.Driver.Core.Events.CommandSucceededEvent>(e =>
        {
            var logger = loggerFactory.CreateLogger("MongoDB.Driver");
            logger.LogInformation($"MongoDB Command Succeeded: {e.CommandName} - Duration: {e.Duration}");
        });

        cb.Subscribe<MongoDB.Driver.Core.Events.CommandFailedEvent>(e =>
        {
            var logger = loggerFactory.CreateLogger("MongoDB.Driver");
            logger.LogError($"MongoDB Command Failed: {e.CommandName} - Error: {e.Failure}");
        });
    };

    return new MongoClient(mongoClientSettings);
});

builder.Services.AddSingleton(provider =>
{
    var client = provider.GetRequiredService<IMongoClient>();
    var settings = provider.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

#endregion

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World from Payments API!").AllowAnonymous();
app.MapHealthChecks("/health").AllowAnonymous();

app.MapPayments().RequireAuthorization();
//app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.UseCors("CorsPolicy");

try
{
    if (isInDevelopment)
    {
        Serilog.Debugging.SelfLog.Enable(msg =>
        {
            Debug.Print(msg);
        });
    }

    app.Run();
}
catch (Exception ex)
{
    Serilog.Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Serilog.Log.CloseAndFlush();
}