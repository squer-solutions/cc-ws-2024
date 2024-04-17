using Microsoft.Extensions.Options;

namespace StreamingApp;

public class StreamingAppWorker : BackgroundService
{
    private readonly StreamingAppSettings _options;
    private readonly ILogger<StreamingAppWorker> _logger;

    public StreamingAppWorker(IOptions<StreamingAppSettings> options, ILogger<StreamingAppWorker> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
