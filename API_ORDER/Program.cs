//using Circuit.ApiInvoice.Datasources;
//using Circuit.Commons;
using API_ORDER.Application.Client;
using API_ORDER.Application.Order;
using API_ORDER.Domain.Client;
using API_ORDER.Domain.Order;
using API_ORDER.Endpoints;
using API_ORDER.Infrastructure;
using API_ORDER.Infrastructure.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Debug;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Diagnostics;

var builder = WebApplication.CreateSlimBuilder(args);

var isInDevelopment = Convert.ToBoolean(builder.Configuration["IsInDevelopment"]);

builder.WebHost.UseUrls("http://+:4070");
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

#region Logs

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);
});

#endregion

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, ClientSerializerContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, OrderSerializerContext.Default);
});

#region TODO: TRACING

var jaegerserver = builder.Configuration.GetValue<string>("JaegerServer") ?? throw new Exception("No se ha configurado la informaci�n del 'Jaeger Server' correctamente");

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

TypeAdapterConfig<Client, ClientDto>
    .NewConfig()
    .Map(dest => dest.NombreCliente, src => src.NombreCliente);

#endregion

builder.Services.AddScoped<OrderHandler>();
builder.Services.AddScoped<ClientHandler>();

#region DATABASE

var loggerFactory = new LoggerFactory(new[] { new DebugLoggerProvider() });
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    if (isInDevelopment)
    {
        options.UseLoggerFactory(loggerFactory);
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors().LogTo(Console.WriteLine, LogLevel.Debug);
    }

    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("No se ha configurado la informaci�n de la base de datos correctamente"));
});
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();

#endregion

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();

//app.UsePathBase("/");
//app.UseSwagger();
//app.UseSwaggerUI();

app.MapGet("/", () => "Hello World!");

app.MapOrders();
app.MapClients();
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