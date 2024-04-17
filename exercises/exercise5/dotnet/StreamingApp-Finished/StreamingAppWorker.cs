using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;

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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. Define the configuration
        // 2. Create Serde instances
        var config = new StreamConfig<StringSerDes, StringSerDes>
        {
            ApplicationId = _options.ApplicationId,
            BootstrapServers = string.Join(',', _options.BootstrapServers),
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        // 3. Build the Topology
        var streamBuilder = new StreamBuilder();
        var sourceStream = streamBuilder.Stream(_options.SourceTopic, new StringSerDes(), new StringSerDes(),
            named: "Streaming App - Input");

        sourceStream
            .MapValues(value => value.ToUpperInvariant())
            .Peek((_, v) => _logger.LogInformation(v))
            .To(_options.DestinationTopic, named: "Streaming App - Output")
            ;

        var topology = streamBuilder.Build();

        // 4. Create and start the kafka Stream
        var stream = new KafkaStream(topology, config);
        await stream.StartAsync(stoppingToken);
    }
}
