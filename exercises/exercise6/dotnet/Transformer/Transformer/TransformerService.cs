using Avro.Generic;
using cdc.@public.customers;
using Confluent.Kafka;
using DefaultNamespace;
using Microsoft.Extensions.Options;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SchemaRegistry.SerDes.Avro;
using Streamiz.Kafka.Net.SerDes;
using Transformer.Models;

namespace Transformer;

public class TransformerService : BackgroundService
{
    private readonly ILogger<TransformerService> _logger;
    private readonly IOptionsMonitor<TransformerConfiguration> _options;

    public TransformerService(ILogger<TransformerService> logger, IOptionsMonitor<TransformerConfiguration> options)
    {
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var kafkaConfig = _options.CurrentValue;
        
        // 1. Define the configuration
        // 2. Create Serde instances
        var config = new StreamConfig<SchemaAvroSerDes<Key>, SchemaAvroSerDes<Value>>
        {
            ApplicationId = kafkaConfig.ApplicationId,
            BootstrapServers = string.Join(",", kafkaConfig.Brokers),
            AutoOffsetReset = AutoOffsetReset.Earliest,

            SchemaRegistryUrl = kafkaConfig.SchemaRegistryUrl,
            AutoRegisterSchemas = true
        };

        // 3. Build the Topology
        var streamBuilder = new StreamBuilder();
        var cdcStream = streamBuilder.Stream(kafkaConfig.CdcTopic,
            new SchemaAvroSerDes<Key>(), new SchemaAvroSerDes<Envelope>(), named: "Transformer - Import");

        // TODO: Create the topology and mappings

        var topology = streamBuilder.Build();

        // 4. Create and start the kafka Stream
        var stream = new KafkaStream(topology, config);
        await stream.StartAsync(stoppingToken);
    }
}
