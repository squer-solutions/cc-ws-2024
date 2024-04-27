package io.squer.services;

import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.streams.KafkaStreams;
import org.apache.kafka.streams.StreamsBuilder;
import org.apache.kafka.streams.StreamsConfig;
import org.apache.kafka.streams.Topology;
import org.apache.kafka.streams.kstream.KStream;
import org.apache.kafka.streams.kstream.Produced;

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
        final StreamsBuilder builder = new StreamsBuilder();

        KStream<String, String> sourceStream = builder.stream(INPUT_TOPIC);

        sourceStream
                .mapValues(value -> value.toUpperCase())
                .peek((key, value) -> System.out.println("Value: " + value))
                .to(OUTPUT_TOPIC, Produced.with(Serdes.String(), Serdes.String()));

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
        props.put(StreamsConfig.DEFAULT_KEY_SERDE_CLASS_CONFIG, Serdes.StringSerde.class);
        props.put(StreamsConfig.DEFAULT_VALUE_SERDE_CLASS_CONFIG, Serdes.StringSerde.class);
        return props;
    }

}
