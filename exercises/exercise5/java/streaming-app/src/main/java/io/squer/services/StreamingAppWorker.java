package io.squer.services;

import java.util.Properties;

public class StreamingAppWorker {

    private final Properties properties;
    static final String INPUT_TOPIC = "src-topic";
    static final String OUTPUT_TOPIC = "dest-topic";

    public StreamingAppWorker(Properties props) {
        properties = props;
    }

    public void run(){
    }
}
