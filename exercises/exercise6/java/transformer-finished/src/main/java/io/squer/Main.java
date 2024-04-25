package io.squer;

import io.confluent.kafka.serializers.AbstractKafkaAvroSerDeConfig;
import io.confluent.kafka.serializers.AbstractKafkaSchemaSerDeConfig;
import io.confluent.kafka.streams.serdes.avro.SpecificAvroSerde;
import io.squer.services.TransformerService;
import org.apache.kafka.streams.StreamsConfig;

import java.util.Properties;

public class Main {
    static final String STREAMS_APPLICATION_ID = "transformer-development-app";
    static final String BOOTSTRAP_SERVERS_CONFIG = "http://localhost:9092";
    static final String SCHEMA_REGISTRY_URL_CONFIG = "http://localhost:8081";
    static final String CLIENT_ID_CONFIG = "transformer-development";

    public static void main(String[] args) {

        Properties props = getStreamsConfig();

        TransformerService transformer = new TransformerService(props);

        transformer.Run();
    }


    private static Properties getStreamsConfig() {
        Properties props = new Properties();
        props.put(StreamsConfig.APPLICATION_ID_CONFIG, STREAMS_APPLICATION_ID);
        props.put(StreamsConfig.BOOTSTRAP_SERVERS_CONFIG, BOOTSTRAP_SERVERS_CONFIG);
        props.put(AbstractKafkaSchemaSerDeConfig.SCHEMA_REGISTRY_URL_CONFIG, SCHEMA_REGISTRY_URL_CONFIG);
        props.put(StreamsConfig.CLIENT_ID_CONFIG, CLIENT_ID_CONFIG);
        props.put(AbstractKafkaAvroSerDeConfig.AUTO_REGISTER_SCHEMAS, true);
        props.put(StreamsConfig.DEFAULT_KEY_SERDE_CLASS_CONFIG, SpecificAvroSerde.class);
        props.put(StreamsConfig.DEFAULT_VALUE_SERDE_CLASS_CONFIG, SpecificAvroSerde.class);
        return props;
    }
}
