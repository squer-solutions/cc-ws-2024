# Putting it Altogether

After bringing up your environment the first thing you need is to download the schema files for the events published by
`Debezium` connector.

The goal for this exercise is to produce messages that conform to a predefined schema that you have already aligned on with your colleagues: [Customer-Transformed.avsc](./transformer/src/main/resources/avro/Customer-Transformer.avsc)

## Retrieving the schema files
### Option 1: Using the schema registry API

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

### Option 2: Via Confluent Control Center
* The quickest way to retrieve the current schema for a topic is by downloading it from the control center
* Open the [ControlCenter](http://localhost:9021/), open the Topics section and find the CDC topic
* Open the tab "Schema", and in the context menu (...), you can download both the key-schema and the value-schema
* Place the schemas in the `src/main/resources/avro` folder

## Maven Plugin Code Generation
* The project is set up to generate the required Java-classes from the downloaded avro schemas
    * It is highly recommended to inspect the `pom.xml` in detail to understand which steps are performed


## Topic and schema creation
* Any Kafka producer (that includes a Kafka Streams application) is capable of creating a topic if it does not exist and automatically registering the schema with registry
* In general, it is advisable to manually register the schemas beforehand. For the purpose of this execute, you may rely on the automatic behaviour.
* However, if you prefer to do it manually, you can follow the steps below
### Manual topic creation and schema registration
Before starting the application we also need to register the new `Customer-Transformed-Topic` schema for the `customer-transformed-topic`, 
go to the [Control Center](http://localhost:9021/), click on the **cluster card**, choose `Topics` from the left menu
and click on the `+ Add Topic` button, give it the name `customer-transformed-topic` and click on the `Create with defaults` button,
after the topic is created, go to the `Schema` tab for the topic and for the `value` click on `Set schema` button, upload the 
contents of the [Customer-Transformed.avsc](./transformer/src/main/resources/avro/Customer-Transformer.avsc), and click on
the `Create button`.

## Kafka streams application
Now, we are ready to consume the events from the `cdc.public.customers` topic using Kafka Streams,
it will take the cdc event and convert it to a new message of type `Customer`, and finally publishes that to the `customer-transformed-topic`.

Follow the steps you learned in the [exercise 5](../../exercise5/dotnet/README.md) to create you Kafka Stream.

we are now ready to implement the stream topology:

1. Map the values from the cdc representation to a `Customer` event, using the `mapValues`
2. Change the old key (`customer_id`) to a new key `username`, using `map` and return `KeyValue.pair`
3. Publish to the destination topic `customer-transformed-topic`
4. Hint: You will want to serialize the key using a regular String-Serde for the key and the default SpecificAvroSerde for the value. To use the default serializer, you can pass `null` as an argument for the serializer like so:
```java
.to("output-topic-name", Produced.with(Serdes.String(), null)); 
```

#### Notes
Try to map the entity on your own first to get a feel how you can work with the generated avro classes.
After you made your own steps and don't feel like typing everything out, you can use the mapping function provided below.
<details>
<summary>SPOILER: Mapping function</summary>

```java
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
```
</details>

Please try to solve this exercise yourself to get the most of it.
<details>
<summary>Should you get stuck, you can have a look at the solution here:</summary>

```java
public void run() {

  final StreamsBuilder builder = new StreamsBuilder();

  KStream<Key, Envelope> sourceStream = builder.stream(CDC_TOPIC);

  sourceStream.peek((key, value) -> {
    System.out.println("Key: " + key);
    System.out.println("Value before: " + value.getBefore());
    System.out.println("Value after: " + value.getAfter());
  });

  sourceStream
          .mapValues(TransformerService::mapCustomer)
          .map((k, v) -> KeyValue.pair(v.getUsername().toString(), v))
          .to(TRANSFORMER_TOPIC, Produced.with(Serdes.String(),null));

  Topology topology = builder.build();

  KafkaStreams streams = new KafkaStreams(topology, properties);

  // attach shutdown handler to catch control-c
  Runtime.getRuntime().addShutdownHook(new Thread(streams::close));

  streams.start();
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
