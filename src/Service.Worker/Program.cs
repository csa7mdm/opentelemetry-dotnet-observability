using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Service.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

// Define the resource
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("Service.Worker", serviceVersion: "1.0.0")
    .AddTelemetrySdk();

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.SetResourceBuilder(resourceBuilder);
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
    logging.AddOtlpExporter();
});

// Configure Tracing & Metrics
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.SetResourceBuilder(resourceBuilder)
               .AddHttpClientInstrumentation()
               .AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.SetResourceBuilder(resourceBuilder)
               .AddHttpClientInstrumentation()
               .AddRuntimeInstrumentation()
               .AddMeter("Service.Worker")
               .AddOtlpExporter();
    });

var host = builder.Build();
host.Run();
