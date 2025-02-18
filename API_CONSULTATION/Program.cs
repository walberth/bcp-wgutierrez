using API_CONSULTATION.Application.Background;
using API_CONSULTATION.Application.Consultation;
using API_CONSULTATION.Application.Enums;
using API_CONSULTATION.Configuration;
using API_CONSULTATION.CrossCutting;
using API_CONSULTATION.Domain.Consultation;
using API_CONSULTATION.Endpoints;
using API_CONSULTATION.Infrastructure;
using Confluent.Kafka;
using Mapster;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Diagnostics;

var builder = WebApplication.CreateSlimBuilder(args);

await Task.Delay(20000);

var isInDevelopment = Convert.ToBoolean(builder.Configuration["IsInDevelopment"]);
Constant.BackgroundSecondsToWait = Convert.ToInt32(builder.Configuration.GetSection("Background:SecondsToWait").Value);

builder.WebHost.UseUrls("http://+:4090");
builder.Services.AddHttpContextAccessor();

#region TODO: AUTH

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(opt =>
//    {
//        opt.Authority = builder.Configuration.GetValue<string>("FederateApi:AuthorityUrl");
//        opt.Audience = builder.Configuration.GetValue<string>("FederateApi:AudienceUrl");
//        opt.RequireHttpsMetadata = false;

//        opt.TokenValidationParameters.ValidateAudience = true;
//    });

//builder.Services.AddAuthorization();

#endregion

#region LOGS

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);
});

#endregion

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, ConsultationSerializerContext.Default);
});

#region KAFKA

var kafkaSettings = new KafkaSettings();
builder.Configuration.GetSection("Kafka").Bind(kafkaSettings);
builder.Services.AddSingleton(kafkaSettings);

builder.Services.AddSingleton<IConsumer<Null, string>>(sp =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = kafkaSettings.BootstrapServers,
        GroupId = kafkaSettings.GroupId,
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false
    };

    return new ConsumerBuilder<Null, string>(config).Build();
});

#endregion

#region TRACING

var jaegerserver = builder.Configuration.GetValue<string>("JaegerServer")
    ?? throw new Exception("No se ha configurado la informaci�n del 'Jaeger Server' correctamente");

builder.Services.AddOpenTelemetry()
    .WithTracing(opt => opt
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("API_CONSULTATION"))
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
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("API_CONSULTATION"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter("Microsoft.AspNetCore")
        .AddPrometheusExporter());

#endregion

#region MAPPER

builder.Services.AddMapster();

TypeAdapterConfig<ConsultationDto, Consultation>
    .NewConfig()
    .Map(dest => dest.IdConsulta, src => src.IdConsulta)
    .Map(dest => dest.IdPago, src => src.IdPago)
    .Map(dest => dest.IdPedido, src => src.IdPedido)
    .Map(dest => dest.NombreCliente, src => src.NombreCliente)
    .Map(dest => dest.IdPago, src => src.IdPago)
    .Map(dest => dest.FormaPago, src => $"{(int)src.FormaPago}")
    .Map(dest => dest.MontoPago, src => src.MontoPago);

TypeAdapterConfig<Consultation, ConsultationDto>
    .NewConfig()
    .Map(dest => dest.IdConsulta, src => src.IdConsulta)
    .Map(dest => dest.IdPedido, src => src.IdPedido)
    .Map(dest => dest.NombreCliente, src => src.NombreCliente)
    .Map(dest => dest.IdPago, src => src.IdPago)
    .Map(dest => dest.MontoPago, src => src.MontoPago)
    .Map(dest => dest.FormaPago, src => $"{(FormaPagoEnum)src.FormaPago}")
    .Map(dest => dest.FormaPagoValue, src => $"{((FormaPagoEnum)src.FormaPago).GetEnumMemberValue()}");

#endregion

builder.Services.AddHostedService<ConsultationProcess>();
builder.Services.AddScoped<ConsultationHandler>();

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

builder.Services.AddScoped<IConsultationRepository, ConsultationRepository>();

#endregion

var app = builder.Build();

app.MapGet("/", () => "Hello World from Consultations API!");

app.MapConsultations();
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