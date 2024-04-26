package io.squer;

import io.squer.services.StreamingAppWorker;
import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.streams.StreamsConfig;

import java.util.Properties;

public class Main {
    static final String STREAMS_APPLICATION_ID = "my-first-streams";
    static final String BOOTSTRAP_SERVERS_CONFIG = "localhost:9092";
    static final String CLIENT_ID_CONFIG = "streams-squerify-client-example";

    public static void main(String[] args) {

        Properties props = getStreamsConfig();

        StreamingAppWorker worker = new StreamingAppWorker(props);

        worker.run();
    }


    private static Properties getStreamsConfig() {

        Properties props = new Properties();

        return props;
    }
}
