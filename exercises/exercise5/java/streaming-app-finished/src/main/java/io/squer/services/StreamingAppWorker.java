package io.squer.services;

import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.streams.KafkaStreams;
import org.apache.kafka.streams.StreamsBuilder;
import org.apache.kafka.streams.Topology;
import org.apache.kafka.streams.kstream.KStream;
import org.apache.kafka.streams.kstream.Produced;

import java.util.Properties;

public class StreamingAppWorker {

    private final Properties properties;
    static final String INPUT_TOPIC = "src-topic";
    static final String OUTPUT_TOPIC = "dest-topic";

    public StreamingAppWorker(Properties props){
        properties = props;
    }

    public void Run(){
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

}
