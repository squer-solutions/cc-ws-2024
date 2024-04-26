package io.squer.services;

import java.util.Properties;
import java.util.UUID;
import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.streams.KafkaStreams;
import org.apache.kafka.streams.KeyValue;
import org.apache.kafka.streams.StreamsBuilder;
import org.apache.kafka.streams.Topology;
import org.apache.kafka.streams.kstream.KStream;
import org.apache.kafka.streams.kstream.Produced;
import transformer.models.Address;
import transformer.models.Customer;

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
     *
     *  sourceStream.peek((key, value) -> {
     *       System.out.println("Key: " + key);
     *       System.out.println("Value before: " + value.getBefore());
     *       System.out.println("Value after: " + value.getAfter());
     *     });
     */
    KStream<cdc.public$.customers.Key, cdc.public$.customers.Envelope> sourceStream = builder.stream(
        CDC_TOPIC);

    sourceStream.peek((key, value) -> {
      System.out.println("Key: " + key);
      System.out.println("Value before: " + value.getBefore());
      System.out.println("Value after: " + value.getAfter());
    });

    sourceStream
        .mapValues(TransformerService::mapCustomer)
        .map((k, v) -> KeyValue.pair(v.getId().toString(), v))
        .to(TRANSFORMER_TOPIC,
            Produced.with(Serdes.String(), null)); //TODO: include this tip in the readme or comment

    // TODO: implement the stream topology

    Topology topology = builder.build();

    KafkaStreams streams = new KafkaStreams(topology, properties);

    // attach shutdown handler to catch control-c
    Runtime.getRuntime().addShutdownHook(new Thread(streams::close));

    streams.start();
  }

  private static Customer mapCustomer(cdc.public$.customers.Envelope envelope) {
    cdc.public$.customers.Value updatedValue = envelope.getAfter();
    Address deliveryAddress = Address.newBuilder()
        .setLine1(updatedValue.getDeliveryAddress())
        .setZipcode(updatedValue.getDeliveryZipcode())
        .setCity(updatedValue.getDeliveryCity())
        .build();

    Address billingAddress = updatedValue.getBillingAddress() == null
        ? Address.newBuilder()
        .setLine1(updatedValue.getDeliveryAddress())
        .setZipcode(updatedValue.getDeliveryZipcode())
        .setCity(updatedValue.getDeliveryCity())
        .build()
        : Address.newBuilder()
            .setLine1(updatedValue.getBillingAddress())
            .setZipcode(updatedValue.getBillingZipcode())
            .setCity(updatedValue.getBillingCity())
            .build();

    String[] names = updatedValue.getFullName().split(" ");
    String firstName = names[0];
    String lastName = names.length > 1 ? names[1] : names[0];

    return Customer.newBuilder()
        .setId(UUID.fromString(updatedValue.getCustomerId()))
        .setUsername(updatedValue.getUserName())
        .setFirstName(firstName)
        .setLastName(lastName)
        .setEmail(updatedValue.getEmail())
        .setDefaultDeliveryAddress(deliveryAddress)
        .setDefaultBillingAddress(billingAddress)
        .build();
  }
}
