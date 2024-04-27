package io.squer.services;

import io.confluent.kafka.serializers.AbstractKafkaSchemaSerDeConfig;
import io.confluent.kafka.streams.serdes.avro.SpecificAvroSerde;
import org.apache.kafka.streams.KafkaStreams;
import org.apache.kafka.streams.StreamsBuilder;
import org.apache.kafka.streams.StreamsConfig;
import org.apache.kafka.streams.Topology;
import java.util.Properties;

public class TransformerService {
    static final String STREAMS_APPLICATION_ID = "transformer-development-app";
    static final String BOOTSTRAP_SERVERS_CONFIG = "http://localhost:9092";
    static final String SCHEMA_REGISTRY_URL_CONFIG = "http://localhost:8081";
    static final String CLIENT_ID_CONFIG = "transformer-development";
    private final Properties properties;
    static final String CDC_TOPIC = "cdc.public.customers";
    static final String TRANSFORMER_TOPIC = "customer-transformed-topic";

    public TransformerService() {
        properties = getStreamsConfig();
    }

    public void run() {

        final StreamsBuilder builder = new StreamsBuilder();

        /*
         * To consume the stream, you will need something along the lines of:
         *  KStream<cdc.public$.customers.Key, cdc.public$.customers.Envelope> sourceStream = builder.stream(
         *         CDC_TOPIC);
         *  sourceStream.peek((key, value) -> {
         *       System.out.println("Key: " + key);
         *       System.out.println("Value before: " + value.getBefore());
         *       System.out.println("Value after: " + value.getAfter());
         *     });
         * TODO: implement the stream topology
         */


        Topology topology = builder.build();

        KafkaStreams streams = new KafkaStreams(topology, properties);

        // attach shutdown handler to catch control-c
        Runtime.getRuntime().addShutdownHook(new Thread(streams::close));

        streams.start();
    }

    private static Properties getStreamsConfig() {
        Properties props = new Properties();
        props.put(StreamsConfig.APPLICATION_ID_CONFIG, STREAMS_APPLICATION_ID);
        props.put(StreamsConfig.BOOTSTRAP_SERVERS_CONFIG, BOOTSTRAP_SERVERS_CONFIG);
        props.put(StreamsConfig.CLIENT_ID_CONFIG, CLIENT_ID_CONFIG);
        props.put(StreamsConfig.DEFAULT_KEY_SERDE_CLASS_CONFIG, SpecificAvroSerde.class);
        props.put(StreamsConfig.DEFAULT_VALUE_SERDE_CLASS_CONFIG, SpecificAvroSerde.class);
        props.put(AbstractKafkaSchemaSerDeConfig.AUTO_REGISTER_SCHEMAS, true);
        props.put(AbstractKafkaSchemaSerDeConfig.SCHEMA_REGISTRY_URL_CONFIG, SCHEMA_REGISTRY_URL_CONFIG);
        return props;
    }
}
