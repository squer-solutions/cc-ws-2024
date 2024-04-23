using Avro.Generic;
using cdc.@public.customers;
using Confluent.Kafka;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SchemaRegistry.SerDes.Avro;
using Streamiz.Kafka.Net.SerDes;
using Transformer.Models;

namespace Transformer;

public class TransformerService : BackgroundService
{
    private readonly ILogger<TransformerService> _logger;

    public TransformerService(ILogger<TransformerService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. Define the configuration
        // 2. Create Serde instances
        var config = new StreamConfig<SchemaAvroSerDes<Key>, SchemaAvroSerDes<Value>>
        {
            // TODO: use option pattern
            ApplicationId = "Transformer-test",
            BootstrapServers = "localhost:9092",
            AutoOffsetReset = AutoOffsetReset.Earliest,

            SchemaRegistryUrl = "localhost:8081",
            AutoRegisterSchemas = true
        };

        // 3. Build the Topology
        var streamBuilder = new StreamBuilder();
        var cdcStream = streamBuilder.Stream("cdc.public.customers",
            new SchemaAvroSerDes<Key>(), new SchemaAvroSerDes<Envelope>(), named: "Transformer - Import");

        cdcStream.MapValues(envelope => Customer.Create(
                Guid.Parse(envelope.after.customer_id), envelope.after.user_name,
                envelope.after.full_name,
                envelope.after.email,
                new Address(envelope.after.delivery_address, envelope.after.delivery_zipcode,
                    envelope.after.delivery_city),
                string.IsNullOrEmpty(envelope.after.billing_address)
                    ? null
                    : new Address(envelope.after.billing_address, envelope.after.billing_zipcode,
                        envelope.after.billing_city)
            ))
            .Map((_, v) => KeyValuePair.Create(v!.Username, v))
            .To<StringSerDes, SchemaAvroSerDes<Customer>>("customer-transformed-topic",
                named: "Transformer Export");

        var topology = streamBuilder.Build();

        // 4. Create and start the kafka Stream
        var stream = new KafkaStream(topology, config);
        await stream.StartAsync(stoppingToken);
    }
}
