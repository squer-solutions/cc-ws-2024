package io.squer.services;

import java.util.Properties;

public class StreamingAppWorker {
    private static final String STREAMS_APPLICATION_ID = "my-first-streams";
    private static final String BOOTSTRAP_SERVERS_CONFIG = "localhost:9092";
    private static final String CLIENT_ID_CONFIG = "streams-client-example";
    private static final String INPUT_TOPIC = "src-topic";
    private static final String OUTPUT_TOPIC = "dest-topic";
    private final Properties properties;

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
