package io.squer.services;

import Transformer.Models.Address;
import Transformer.Models.Customer;
import cdc.public$.customers.Envelope;
import cdc.public$.customers.Key;
import cdc.public$.customers.Value;
import io.confluent.kafka.schemaregistry.client.CachedSchemaRegistryClient;
import io.confluent.kafka.serializers.AbstractKafkaAvroSerDeConfig;
import io.confluent.kafka.serializers.AbstractKafkaSchemaSerDeConfig;
import io.confluent.kafka.streams.serdes.avro.SpecificAvroSerde;
import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.streams.KafkaStreams;
import org.apache.kafka.streams.KeyValue;
import org.apache.kafka.streams.StreamsBuilder;
import org.apache.kafka.streams.Topology;
import org.apache.kafka.streams.kstream.KStream;
import org.apache.kafka.streams.kstream.Produced;


import java.util.Properties;
import java.util.UUID;

public class TransformerService {

    private final Properties properties;
    static final String CDC_TOPIC = "cdc.public.customers";
    static final String TRANSFORMER_TOPIC = "customer-transformed-topic";

    public TransformerService(Properties props) {
        properties = props;
    }

    public void run() {

        final StreamsBuilder builder = new StreamsBuilder();

        CachedSchemaRegistryClient schemaRegistryClient = new CachedSchemaRegistryClient(
                properties.getProperty(AbstractKafkaSchemaSerDeConfig.SCHEMA_REGISTRY_URL_CONFIG).toString(),
                AbstractKafkaAvroSerDeConfig.MAX_SCHEMAS_PER_SUBJECT_DEFAULT);

        KStream<Key, Envelope> sourceStream = builder.stream(CDC_TOPIC);

        sourceStream.peek((key, value) -> {
            System.out.println("Key: " + key);
            System.out.println("Value before: " + value.getBefore());
            System.out.println("Value after: " + value.getAfter());
        });

        sourceStream
                .mapValues(TransformerService::mapCustomer)
                .map((k, v) -> KeyValue.pair(v.getUsername().toString(), v))
                .to(TRANSFORMER_TOPIC, Produced.with(Serdes.String(), new SpecificAvroSerde<>(schemaRegistryClient)));

        Topology topology = builder.build();

        KafkaStreams streams = new KafkaStreams(topology, properties);

        // attach shutdown handler to catch control-c
        Runtime.getRuntime().addShutdownHook(new Thread(streams::close));

        streams.start();
    }

    private static Customer mapCustomer(Envelope envelope) {

        Value envelopeAfter = envelope.getAfter();

        Address deliveryAddress = Address.newBuilder()
                .setLien1(envelopeAfter.getDeliveryAddress())
                .setZipcode(envelopeAfter.getDeliveryZipcode())
                .setCity(envelopeAfter.getDeliveryCity())
                .build();

        Address billingAddress = envelopeAfter.getBillingAddress() == null
                ? Address.newBuilder()
                .setLien1(envelopeAfter.getDeliveryAddress())
                .setZipcode(envelopeAfter.getDeliveryZipcode())
                .setCity(envelopeAfter.getDeliveryCity())
                .build()
                : Address.newBuilder()
                .setLien1(envelopeAfter.getBillingAddress())
                .setZipcode(envelopeAfter.getBillingZipcode())
                .setCity(envelopeAfter.getBillingCity())
                .build();

        String[] names = envelopeAfter.getFullName().toString().split(" ");
        String firstName = names[0];
        String lastName = names.length > 1 ? names[1] : names[0];

        return Customer.newBuilder()
                .setId(UUID.fromString(envelopeAfter.getCustomerId().toString()))
                .setUsername(envelopeAfter.getUserName())
                .setFirstName(firstName)
                .setLastName(lastName)
                .setEmail(envelopeAfter.getEmail())
                .setDefaultDeliveryAddress(deliveryAddress)
                .setDefaultBillingAddress(billingAddress)
                .build();
    }
}
