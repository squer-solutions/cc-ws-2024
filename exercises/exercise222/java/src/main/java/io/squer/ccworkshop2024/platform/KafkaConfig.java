package io.squer.ccworkshop2024.platform;

import com.fasterxml.jackson.databind.ser.std.StringSerializer;
import io.confluent.kafka.serializers.KafkaAvroSerializer;
import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.streams.StreamsConfig;

import java.util.Properties;

public class KafkaConfig {

    static final String STREAMS_APPLICATION_ID="streams-squerify";
    static final String BOOTSTRAP_SERVERS_CONFIG="localhost:9092";
    static final String CLIENT_ID_CONFIG="streams-squerify-client-example";
    static final String INPUT_TOPIC = "squerify-input";
    static final String OUTPUT_TOPIC = "squerify-output";

    public static Properties getDefaultKafkaConfig() {
        Properties properties = new Properties();

        properties.put("bootstrap.servers", "localhost:9092");
        properties.put("acks", "all");
        properties.put("retries", "10");
        properties.put("schema.registry.url", "http://localhost:8081");
        return properties;

    }

    public static Properties getStreamsConfig() {
        Properties props = new Properties();
        props.put(StreamsConfig.APPLICATION_ID_CONFIG, STREAMS_APPLICATION_ID);
        props.put(StreamsConfig.BOOTSTRAP_SERVERS_CONFIG, BOOTSTRAP_SERVERS_CONFIG);
        props.put(StreamsConfig.CLIENT_ID_CONFIG, CLIENT_ID_CONFIG);
        props.put(StreamsConfig.DEFAULT_KEY_SERDE_CLASS_CONFIG, Serdes.String().getClass());
        props.put(StreamsConfig.DEFAULT_VALUE_SERDE_CLASS_CONFIG, Serdes.String().getClass());
        return props;
    }
}
