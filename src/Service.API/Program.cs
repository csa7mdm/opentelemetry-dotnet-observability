using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------------------------------------
// 1. Resource Configuration
// define the "Service Resource" with semantic conventions. 
// "service.name" is critical for correlating signals in the backend.
// --------------------------------------------------------------------------------
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("Service.API", serviceVersion: "1.0.0")
    .AddTelemetrySdk(); // Adds otel.library.* metadata

// --------------------------------------------------------------------------------
// 2. Logging (Structured & Correlated)
// Replaces the default console logger with OTel to support seamless correlation.
// TraceID and SpanID are automatically injected into every log message.
// --------------------------------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.SetResourceBuilder(resourceBuilder);
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
    logging.AddOtlpExporter(); // Exports logs to Collector (then to Loki)
});

// --------------------------------------------------------------------------------
// 3. Tracing (Distributed System Visibility)
// We use Head-Based Sampling here to control costs. 
// 10% sampling provides sufficient statistical significance for general latency trends.
// --------------------------------------------------------------------------------
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.SetResourceBuilder(resourceBuilder)
               .AddAspNetCoreInstrumentation() // Auto-instrument incoming HTTP requests
               .AddHttpClientInstrumentation() // Auto-instrument outgoing HTTP client calls
                                               // SRE UPDATE: Removed Head Sampling (was 10%). 
                                               // We now send 100% of traces to the Agent. 
                                               // The 'otel-gateway' performs Tail-Based Sampling to decide retention.
               .AddOtlpExporter(); // Exports traces to Collector (then to Tempo)
    })
    .WithMetrics(metrics =>
    {
        metrics.SetResourceBuilder(resourceBuilder)
               .AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddRuntimeInstrumentation() // CPU, Memory, GC metrics
               .AddMeter("Service.API.Business") // Registers our custom business meter
               .AddOtlpExporter(); // Exports metrics to Collector (then to Prometheus)
    });

// Register local services for dependency injection
builder.Services.AddSingleton<BusinessMetrics>();

var app = builder.Build();

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (ILogger<Program> logger) =>
{
    // OTel will automatically correlate this log with the current request's TraceID
    logger.LogInformation("Getting weather forecast for user request");

    return Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
});

// --------------------------------------------------------------------------------
// 4. Business Metrics Simulation
// Demonstrates how to track High-Value Key Performance Indicators (KPIs).
// --------------------------------------------------------------------------------
app.MapPost("/checkout", (BusinessMetrics metrics, ILogger<Program> logger) =>
{
    // Simulate complex business logic
    var success = Random.Shared.Next(0, 10) > 2; // 80% success rate

    if (success)
    {
        // RECORD METRIC: Checkout success
        // We add high-cardinality attributes like 'currency' carefully.
        // We DO NOT add user-specific data (PII) to metrics.
        metrics.OrderCreated.Add(1,
            new KeyValuePair<string, object?>("currency", "USD"),
            new KeyValuePair<string, object?>("plan", "premium"));

        logger.LogInformation("Order processed successfully");
        return Results.Ok(new { Status = "Order Created" });
    }
    else
    {
        // Log Error: This will be picked up by the "High Error Rate" alert rule
        logger.LogError("Payment processing failed");
        return Results.Problem("Payment Failed", statusCode: 500);
    }
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// --------------------------------------------------------------------------------
// 5. Custom Meter Definition
// Encapsulates business metric definitions to enforce consistency.
// --------------------------------------------------------------------------------
public class BusinessMetrics
{
    public Counter<long> OrderCreated { get; }

    public BusinessMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Service.API.Business");

        // Counter Name: Follows Prometheus snake_case convention (orders_created_total)
        OrderCreated = meter.CreateCounter<long>("orders_created_total", "orders", "Total number of orders created");
    }
}
