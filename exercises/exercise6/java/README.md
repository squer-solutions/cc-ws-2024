# Putting it Altogether

After bringing up your environment the first thing you need is to download the schema files for the events published by
`Debezium` connector.

The following command lists all the registered schemas:

```http request
GET http://localhost:8081/subjects
```

Checkout the other commands listed in the [schema-registry.http](./scripts/schema-registry.http) file; now, you should
be able to fetch the schema for a specific subject and version:

```http request
### GET the schema by its id
GET http://localhost:8081/schemas/ids/2
```

Take the schema property value out and create `resources/avro/cdc-customers-v1-avsc` file, do the same thing for the other
schema: 

```http request
### GET the schema by its id
GET http://localhost:8081/schemas/ids/1
```

After that, you need to download the [avro-tools] jar file and copy it in the root folder of the `transformer` project,

then run the following commands from `terminal`, to generate `java` classes for each specific schema:

```bash
java -jar ./java/transformer/avro-tools-1.11.3.jar compile schema ./java/transformer/src/main/resources/avro/cdc-customers-v1.avsc ./java/transformer/src/main/java
```
```bash
java -jar ./java/transformer/avro-tools-1.11.3.jar compile schema ./java/transformer/src/main/resources/avro/cdc-customers-key-v1.avsc ./java/transformer/src/main/java
```
```bash
java -jar ./java/transformer/avro-tools-1.11.3.jar compile schema ./java/transformer/src/main/resources/avro/Customer-Transformer.avsc ./java/transformer/src/main/java
```

Before starting the application we also need to register the new `Customer-Transformed-Topic` schema for the `customer-transformed-topic`, 
go to the [Control Center](http://localhost:9021/), click on the **cluster card**, choose `Topics` from the left menu
and click on the `+ Add Topic` button, give it the name `customer-transformed-topic` and click on the `Create with defaults` button,
after the topic is created, go to the `Schema` tab for the topic and for the `value` click on `Set schema` button, upload the 
contents of the [Customer-Transformed.avsc](./transformer/src/main/resources/avro/Customer-Transformer.avsc), and click on
the `Create button`.


Now, we are ready to consume the events from the `cdc.public.customers` topic using Kafka Streams,
it will take the cdc event and convert it to a new message of type `Customer`, and finally publishes that to the `customer-transformed-topic`.


Follow the steps you learned in the [exercise 5](../../exercise5/dotnet/README.md) to create you Kafka Stream.
If you want to create the destination topic for this application, by hand, checkout [exercise 2](../../exercise2/README.md)

we are now ready to implement the stream topology:

1. Map the values from the cdc representation to a `Customer` event, using the `mapValues`
2. Change the old key (`customer_id`) to a new key `username`, using `map` and return `KeyValue.pair`
3. publish to the destination topic `customer-transformed-topic`


<details>

<summary>After completing the previous steps you could use the following for the stream topology</summary>

```java
cdcStream
    .mapValues(envelope -> {
                Address deliveryAddress = Address.newBuilder()
                        .setLien1(envelope.getAfter().getDeliveryAddress())
                        .setZipcode(envelope.getAfter().getDeliveryZipcode())
                        .setCity(envelope.getAfter().getDeliveryCity())
                        .build();

                Address billingAddress = envelope.getAfter().getBillingAddress() == null
                        ? Address.newBuilder()
                        .setLien1(envelope.getAfter().getDeliveryAddress())
                        .setZipcode(envelope.getAfter().getDeliveryZipcode())
                        .setCity(envelope.getAfter().getDeliveryCity())
                        .build()
                        : Address.newBuilder()
                        .setLien1(envelope.getAfter().getBillingAddress())
                        .setZipcode(envelope.getAfter().getBillingZipcode())
                        .setCity(envelope.getAfter().getBillingCity())
                        .build();

                String[] names = envelope.getAfter().getFullName().toString().split(" ");
                String firstName = names[0];
                String lastName = names.length > 1 ? names[1] : names[0];

                return Customer.newBuilder()
                        .setId(UUID.fromString(envelope.getAfter().getCustomerId().toString()))
                        .setUsername(envelope.getAfter().getUserName())
                        .setFirstName(firstName)
                        .setLastName(lastName)
                        .setEmail(envelope.getAfter().getEmail())
                        .setDefaultDeliveryAddress(deliveryAddress)
                        .setDefaultBillingAddress(billingAddress)
                        .build();
            }

    )
    .map((k, v) -> KeyValue.pair(v.getUsername().toString(), v))
    .to(TRANSFORMER_TOPIC, Produced.with(Serdes.String(), new SpecificAvroSerde<>(schemaRegistryClient)));

```
</details>

## Congratulations

Perfect, you completed the whole pipeline for transferring data from your legacy app to the new one,
now open the portals for the [Legacy App](http://localhost:9091) and the [New Workshop Management App](http://localhost:9090),
change or add some data in the legacy app, and you should see them near realtime in the new app.


## Related Documents

* [Schema Registry API usage Examples](https://docs.confluent.io/platform/current/schema-registry/develop/using.html)
* [Register a new version of a schema](https://docs.confluent.io/platform/current/schema-registry/develop/using.html#register-a-new-version-of-a-schema-under-the-subject-kafka-value)
* [Apache Avro Tools - Compiling Schemas](https://avro.apache.org/docs/1.11.1/getting-started-java/#compiling-the-schema)
* [Apache Avro Tools - Maven Repository](https://mvnrepository.com/artifact/org.apache.avro/avro-tools)
* [Apache Avro Maven Plugin](https://mvnrepository.com/artifact/org.apache.avro/avro-maven-plugin)
* [Avro Schema Serializer and Deserializer for Schema Registry](https://docs.confluent.io/platform/current/schema-registry/fundamentals/serdes-develop/serdes-avro.html)
* [Available Serializers for Kafka Stream](https://docs.confluent.io/platform/current/streams/developer-guide/datatypes.html#avro)
* [Kafka Stream Avro Serializer Example]( https://github.com/confluentinc/kafka-streams-examples/blob/7.6.1-post/src/test/java/io/confluent/examples/streams/SpecificAvroIntegrationTest.java )
* [Stream Avro Serialized Objecs in 6 Steps](https://medium.com/new-generation/apache-kafka-stream-avro-serialized-objects-in-6-steps-94c012f75588)
* [How to produce Avro messages to Kafka using Schema Registry](https://itnext.io/howto-produce-avro-messages-to-kafka-ec0b770e1f54)
