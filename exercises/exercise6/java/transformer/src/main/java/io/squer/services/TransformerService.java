package io.squer.services;

import io.confluent.kafka.schemaregistry.client.CachedSchemaRegistryClient;
import io.confluent.kafka.serializers.AbstractKafkaAvroSerDeConfig;
import io.confluent.kafka.serializers.AbstractKafkaSchemaSerDeConfig;
import org.apache.kafka.streams.KafkaStreams;
import org.apache.kafka.streams.StreamsBuilder;
import org.apache.kafka.streams.Topology;
import org.apache.kafka.streams.kstream.KStream;

import java.util.Properties;

public class TransformerService {

    private final Properties properties;
    static final String CDC_TOPIC = "cdc.public.customers";
    static final String TRANSFORMER_TOPIC = "customer-transformed-topic";

    public TransformerService(Properties props) {
        properties = props;
    }

    public void run() {

        final StreamsBuilder builder = new StreamsBuilder();

        CachedSchemaRegistryClient schemaRegistryClient = new CachedSchemaRegistryClient(
                properties.getProperty(AbstractKafkaSchemaSerDeConfig.SCHEMA_REGISTRY_URL_CONFIG).toString(),
                AbstractKafkaAvroSerDeConfig.MAX_SCHEMAS_PER_SUBJECT_DEFAULT);

        //KStream<Key, Envelope> cdcStream = builder.stream(CDC_TOPIC);

        // TODO: implement the stream topology

        Topology topology = builder.build();

        KafkaStreams streams = new KafkaStreams(topology, properties);

        // attach shutdown handler to catch control-c
        Runtime.getRuntime().addShutdownHook(new Thread(streams::close));

        streams.start();
    }
}
