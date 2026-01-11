namespace Service.Worker;

using System.Diagnostics.Metrics;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Counter<long> _iterationCounter;

    public Worker(ILogger<Worker> logger, IMeterFactory meterFactory)
    {
        _logger = logger;
        var meter = meterFactory.Create("Service.Worker");
        _iterationCounter = meter.CreateCounter<long>("work_iterations_total");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Record metric with attribute
            _iterationCounter.Add(1, new KeyValuePair<string, object?>("worker_type", "background"));

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(5000, stoppingToken); // Slow down log spam to 5s
        }
    }
}
