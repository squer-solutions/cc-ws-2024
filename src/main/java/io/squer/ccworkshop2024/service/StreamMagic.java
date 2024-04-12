package io.squer.ccworkshop2024.service;

import io.confluent.kafka.serializers.KafkaAvroSerializer;
import io.confluent.kafka.streams.serdes.avro.SpecificAvroSerde;
import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.streams.KafkaStreams;
import org.apache.kafka.streams.StreamsBuilder;
import org.apache.kafka.streams.StreamsConfig;
import org.apache.kafka.streams.Topology;
import org.apache.kafka.streams.kstream.KStream;
import squerdb.public$.orders.Envelope;

import java.util.Properties;
import java.util.concurrent.CountDownLatch;

public class StreamMagic {


    static final String STREAMS_APPLICATION_ID="my-streams3";
    static final String BOOTSTRAP_SERVERS_CONFIG="localhost:9092";
    static final String CLIENT_ID_CONFIG="streams-squerify-client-example";
    static final String INPUT_TOPIC = "squerdb.public.orders";
    static final String OUTPUT_TOPIC = "squerify-output";


    public void start() {

        Properties props = getStreamsConfig();

        final StreamsBuilder builder = new StreamsBuilder();

        KStream<squerdb.public$.orders.Key, squerdb.public$.orders.Envelope> input = builder.stream(INPUT_TOPIC);

        input.peek((key, value) -> {
            System.out.println("Key: " + key);
            System.out.println("Value: " + value);
            System.out.println("Value: " + value.getAfter().toString());
        });

        Topology topology = builder.build();

        KafkaStreams streams = new KafkaStreams(topology, props);

        // attach shutdown handler to catch control-c
        Runtime.getRuntime().addShutdownHook(new Thread(streams::close));

        streams.start();
    }


        private static Properties getStreamsConfig() {
        Properties props = new Properties();
        props.put(StreamsConfig.APPLICATION_ID_CONFIG, STREAMS_APPLICATION_ID);
        props.put(StreamsConfig.BOOTSTRAP_SERVERS_CONFIG, BOOTSTRAP_SERVERS_CONFIG);
        props.put("schema.registry.url", "http://localhost:8081");
        props.put(StreamsConfig.CLIENT_ID_CONFIG, CLIENT_ID_CONFIG);
        props.put(StreamsConfig.DEFAULT_KEY_SERDE_CLASS_CONFIG, SpecificAvroSerde.class);
        props.put(StreamsConfig.DEFAULT_VALUE_SERDE_CLASS_CONFIG, SpecificAvroSerde.class);
        return props;
    }
}
