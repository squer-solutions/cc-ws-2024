package io.squer;

import io.squer.services.StreamingAppWorker;
import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.streams.StreamsConfig;

import java.util.Properties;

public class Main {
    static final String STREAMS_APPLICATION_ID = "my-streams3";
    static final String BOOTSTRAP_SERVERS_CONFIG = "localhost:9092";
    static final String CLIENT_ID_CONFIG = "streams-squerify-client-example";

    public static void main(String[] args) {

        Properties props = getStreamsConfig();

        StreamingAppWorker worker = new StreamingAppWorker(props);

        worker.Run();
    }


    private static Properties getStreamsConfig() {
        Properties props = new Properties();
        props.put(StreamsConfig.APPLICATION_ID_CONFIG, STREAMS_APPLICATION_ID);
        props.put(StreamsConfig.BOOTSTRAP_SERVERS_CONFIG, BOOTSTRAP_SERVERS_CONFIG);
//        props.put("schema.registry.url", "http://localhost:8081");
        props.put(StreamsConfig.CLIENT_ID_CONFIG, CLIENT_ID_CONFIG);
        props.put(StreamsConfig.DEFAULT_KEY_SERDE_CLASS_CONFIG, Serdes.StringSerde.class);
        props.put(StreamsConfig.DEFAULT_VALUE_SERDE_CLASS_CONFIG, Serdes.StringSerde.class);
        return props;
    }
}
