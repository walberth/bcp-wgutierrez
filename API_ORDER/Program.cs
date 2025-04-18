using API_ORDER.Application.Client;
using API_ORDER.Application.Order;
using API_ORDER.Configuration;
using API_ORDER.Domain;
using API_ORDER.Domain.Client;
using API_ORDER.Domain.Order;
using API_ORDER.Endpoints;
using API_ORDER.Infrastructure;
using API_ORDER.Infrastructure.Context;
using Confluent.Kafka;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Debug;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Steeltoe.Extensions.Configuration.ConfigServer;
using System.Diagnostics;
using System.Net.Http.Headers;
using Log = Serilog.Log;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Configuration.Sources.Clear();

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Configuration.AddConfigServer(builder.Environment);

builder.Services.AddHealthChecks();

// Set the environment
var environment = builder.Configuration["environment"] ?? "Production";
builder.Host.UseEnvironment(environment);

var isInDevelopment = Convert.ToBoolean(builder.Configuration["IsInDevelopment"]);

builder.WebHost.UseUrls("http://+:4070");
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

#region POLLY

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError() // Handles 5xx, 408, etc.
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
        onRetry: (outcome, timespan, retryAttempt, context) =>
        {
            Log.Information($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds due to {outcome.Exception?.Message}");
        });

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3, // Open after 3 consecutive failures
        durationOfBreak: TimeSpan.FromSeconds(30), // Circuit stays open for 30s
        onBreak: (outcome, breakDelay, context) =>
        {
            Log.Warning($"Circuit opened for {breakDelay.TotalSeconds} seconds due to {outcome.Exception?.Message}");
        },
        onReset: (context) =>
        {
            Log.Warning("Circuit closed, normal operation resumed.");
        },
        onHalfOpen: () =>
        {
            Log.Warning("Circuit is half-open. Next call will test the circuit.");
        });

var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
    TimeSpan.FromSeconds(10), // 10s timeout
    onTimeoutAsync: (context, timespan, task) =>
    {
        Log.Error($"Request timed out after {timespan.TotalSeconds} seconds.");
        return Task.CompletedTask;
    });

builder.Services.AddHttpClient("PaymentApiClient", (serviceProvider, client) =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext;

    if (httpContext != null && httpContext.Request.Headers.ContainsKey("Authorization"))
    {
        var bearerToken = httpContext.Request.Headers["Authorization"].ToString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken.Replace("Bearer ", ""));
    }

    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ApiPaymentUrl")
        ?? throw new Exception("No se ha configurado la información del 'API PAYMENT' correctamente"));
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy)
.AddPolicyHandler(timeoutPolicy);

#endregion

#region KAFKA

var kafkaSettings = new KafkaSettings();
builder.Configuration.GetSection("Kafka").Bind(kafkaSettings);
builder.Services.AddSingleton(kafkaSettings);

builder.Services.AddSingleton<IProducer<Null, string>>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = kafkaSettings.BootstrapServers,
        MessageTimeoutMs = 15000
    };
    return new ProducerBuilder<Null, string>(config).Build();
});

#endregion

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, ClientSerializerContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, OrderSerializerContext.Default);
});

#region TRACING

var jaegerserver = builder.Configuration.GetValue<string>("JaegerServer")
    ?? throw new Exception("No se ha configurado la información del 'Jaeger Server' correctamente");

builder.Services.AddOpenTelemetry()
    .WithTracing(opt => opt
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("API_ORDER"))
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
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("API_ORDER"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter("Microsoft.AspNetCore")
        .AddPrometheusExporter());

#endregion

#region MAPPER

builder.Services.AddMapster();

TypeAdapterConfig<RequestDto, Order>
    .NewConfig()
    .Ignore(dest => dest.IdPedido)
    .Ignore(dest => dest.FechaPedido)
    .Map(dest => dest.IdCliente, src => src.IdCliente)
    .Map(dest => dest.MontoPedido, src => src.MontoPago)
    .Ignore(dest => dest.Client);

TypeAdapterConfig<API_ORDER.Domain.Client.Client, ClientDto>
    .NewConfig()
    .Map(dest => dest.NombreCliente, src => src.NombreCliente);

#endregion

builder.Services.AddScoped<OrderHandler>();
builder.Services.AddScoped<ClientHandler>();

#region DATABASE

builder.Services.AddDbContext<DatabaseContext>(options =>
{
    if (isInDevelopment)
    {
        var loggerFactory = new LoggerFactory(new[] { new DebugLoggerProvider() });
        options.UseLoggerFactory(loggerFactory);
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors().LogTo(Console.WriteLine, LogLevel.Debug);
    }

    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new Exception("No se ha configurado la informaci�n de la base de datos correctamente"));
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();

#endregion

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World from Orders API!").AllowAnonymous();
app.MapHealthChecks("/health").AllowAnonymous();

app.MapOrders().RequireAuthorization();
app.MapClients().RequireAuthorization();
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
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}