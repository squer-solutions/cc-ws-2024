package io.squer.services;

import java.util.Properties;
import org.apache.kafka.streams.KafkaStreams;
import org.apache.kafka.streams.StreamsBuilder;
import org.apache.kafka.streams.Topology;

public class TransformerService {

  private final Properties properties;
  static final String CDC_TOPIC = "cdc.public.customers";
  static final String TRANSFORMER_TOPIC = "customer-transformed-topic";

  public TransformerService(Properties props) {
    properties = props;
  }

  public void run() {

    final StreamsBuilder builder = new StreamsBuilder();

    /*
     * To consume the stream, you will need something along the lines of:
     *  KStream<cdc.public$.customers.Key, cdc.public$.customers.Envelope> sourceStream = builder.stream(
     *         CDC_TOPIC);
     *  sourceStream.peek((key, value) -> {
     *       System.out.println("Key: " + key);
     *       System.out.println("Value before: " + value.getBefore());
     *       System.out.println("Value after: " + value.getAfter());
     *     });
     * TODO: implement the stream topology
     */


    Topology topology = builder.build();

    KafkaStreams streams = new KafkaStreams(topology, properties);

    // attach shutdown handler to catch control-c
    Runtime.getRuntime().addShutdownHook(new Thread(streams::close));

    streams.start();
  }
}
