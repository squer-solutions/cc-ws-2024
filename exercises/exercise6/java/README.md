# Putting it Altogether

After bringing up your environment the first thing you need is to download the schema files for the events published by
`Debezium` connector.

## Retrieving the schema files

### Via Confluent Control Center
* The quickest way to retrieve the current schema for a topic is by downloading it from the control center
* Open the [ControlCenter](http://localhost:9021/), open the Topics section and find the CDC topic
* Open the tab "Schema", and in the context menu (...), you can download both the key-schema and the value-schema
* Place the schemas in the `src/main/resources/avro` folder

## Avro-based code generation
* The project is set up to generate the required Java-classes from the downloaded avro schemas
  * It is highly recommended to inspect the `pom.xml` in detail to understand which steps are performed

## Topic and schema creation
* Any Kafka producer (that includes a Kafka Streams application) is capable of creating a topic if it does not exist and automatically registering the schema with registry
* In general, it is advisable to do so beforehand. For the purpose of this execute, you may rely on the automatic behaviour.
* However, if you prefer to do it manually, you can follow the steps below

Before starting the application we also need to register the new `Customer-Transformed-Topic` schema for the `customer-transformed-topic`, 
go to the [Control Center](http://localhost:9021/), click on the **cluster card**, choose `Topics` from the left menu
and click on the `+ Add Topic` button, give it the name `customer-transformed-topic` and click on the `Create with defaults` button,
after the topic is created, go to the `Schema` tab for the topic and for the `value` click on `Set schema` button, upload the 
contents of the [Customer-Transformed.avsc](./transformer/src/main/resources/avro/Customer-Transformer.avsc), and click on
the `Create button`.

## The exercise

Now, we are ready to consume the events from the `cdc.public.customers` topic using Kafka Streams.
It will take the cdc event and convert it to a new message of type `Customer`, and finally publish that to the `customer-transformed-topic`.


Follow the steps you learned in the [exercise 5](../../exercise5/dotnet/README.md) to create your Kafka Streams application.
If you want to create the destination topic for this application by hand using the CLI, checkout [exercise 2](../../exercise2/README.md)

We are now ready to implement the stream topology:

1. Map the values from the cdc representation to a `Customer` event, using the `mapValues`
2. Change the old key (`customer_id`) to a new key `username`, using `map` and return `KeyValue.pair`
3. Publish to the destination topic `customer-transformed-topic`
4. Hint: You will want to serialize the key using a regular String-Serde for the key and the default SpecificAvroSerde for the value. To use the default serializer, you can pass `null` as an argument for the serializer:
```java
.to("output-topic-name", Produced.with(Serdes.String(), null)); 
```


<details>

<summary>After completing the previous steps you could use the following for the stream topology</summary>

```java
public void run() {

  final StreamsBuilder builder = new StreamsBuilder();

  KStream<cdc.public$.customers.Key, cdc.public$.customers.Envelope> sourceStream = builder.stream(
          CDC_TOPIC);

  sourceStream.peek((key, value) -> {
    System.out.println("Key: " + key);
    System.out.println("Value before: " + value.getBefore());
    System.out.println("Value after: " + value.getAfter());
  });

  sourceStream
          .mapValues(TransformerService::mapCustomer)
          .map((k, v) -> KeyValue.pair(v.getUsername(), v))
          .to(TRANSFORMER_TOPIC, Produced.with(Serdes.String(), null));

  Topology topology = builder.build();

  KafkaStreams streams = new KafkaStreams(topology, properties);

  // attach shutdown handler to catch control-c
  Runtime.getRuntime().addShutdownHook(new Thread(streams::close));

  streams.start();
}

private static Customer mapCustomer(cdc.public$.customers.Envelope envelope) {
  cdc.public$.customers.Value updatedValue = envelope.getAfter();
  Address deliveryAddress = Address.newBuilder()
          .setLien1(updatedValue.getDeliveryAddress())
          .setZipcode(updatedValue.getDeliveryZipcode())
          .setCity(updatedValue.getDeliveryCity())
          .build();

  Address billingAddress = updatedValue.getBillingAddress() == null
          ? Address.newBuilder()
          .setLien1(updatedValue.getDeliveryAddress())
          .setZipcode(updatedValue.getDeliveryZipcode())
          .setCity(updatedValue.getDeliveryCity())
          .build()
          : Address.newBuilder()
                  .setLien1(updatedValue.getBillingAddress())
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
