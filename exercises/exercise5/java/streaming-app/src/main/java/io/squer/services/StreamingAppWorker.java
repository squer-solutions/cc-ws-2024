package io.squer.services;

import java.util.Properties;

public class StreamingAppWorker {
    static final String STREAMS_APPLICATION_ID = "my-first-streams";
    static final String BOOTSTRAP_SERVERS_CONFIG = "localhost:9092";
    static final String CLIENT_ID_CONFIG = "streams-squerify-client-example";
    private final Properties properties;
    static final String INPUT_TOPIC = "src-topic";
    static final String OUTPUT_TOPIC = "dest-topic";

    public StreamingAppWorker() {
        properties = getStreamsConfig();
    }

    public void run(){
    }

    private static Properties getStreamsConfig() {

        Properties props = new Properties();

        return props;
    }
}
