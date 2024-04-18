# Getting Started with Streaming Applications

First bring up the Kafka Broker and create a topic there, named `src-topic`. Checkout previous exercises for how to bring up a Kafka Broker, 
bear in mind to expose a public port for the hosting environment.

To get started add the following dependencies to your project (`pom.xml` file):

```xml
<dependencies>
    <dependency>
        <groupId>org.apache.kafka</groupId>
        <artifactId>kafka-clients</artifactId>
        <version>3.7.0</version>
    </dependency>

    <dependency>
        <groupId>org.apache.kafka</groupId>
        <artifactId>kafka-streams</artifactId>
        <version>3.7.0</version>
    </dependency>

    <dependency>
        <groupId>org.slf4j</groupId>
        <artifactId>slf4j-simple</artifactId>
        <version>2.0.12</version>
    </dependency>

    <dependency>
        <groupId>org.slf4j</groupId>
        <artifactId>slf4j-api</artifactId>
        <version>2.0.12</version>
    </dependency>
</dependencies>
```

In the project there is a file `StreamingAppWorker`, under `io.squer.services` package, it is the background service that will host our code for our data stream.

The first step is to create the configuration and selecting the Serializer and Deserializer for the key and value of the stream should work with;
since for our example both the key and value are string we will use the following settings, add them inside the `getStreamsConfig` method, in the `Main` class: 

```java
// 1. Define the configuration

props.put(StreamsConfig.APPLICATION_ID_CONFIG, STREAMS_APPLICATION_ID);
props.put(StreamsConfig.BOOTSTRAP_SERVERS_CONFIG, BOOTSTRAP_SERVERS_CONFIG);
props.put(StreamsConfig.CLIENT_ID_CONFIG, CLIENT_ID_CONFIG);

// 2. Create Serde instances
props.put(StreamsConfig.DEFAULT_KEY_SERDE_CLASS_CONFIG, Serdes.StringSerde.class);
props.put(StreamsConfig.DEFAULT_VALUE_SERDE_CLASS_CONFIG, Serdes.StringSerde.class);
```

The above config is passed through the constructor to the `StreamingAppWorker`.
The next step would be to create the topology of the stream, the following codes will go inside the `Run`
method of the `StreamingAppWorker`

```java
// 3. Build the Topology
final StreamsBuilder builder = new StreamsBuilder();
KStream<String, String> sourceStream = builder.stream(INPUT_TOPIC);
```

Now, it is time to build the functionalities for the stream, in this example, we just want to:
1. Convert the value to upper case
2. Print it on the Console
3. Push it to a destination topic

```java
sourceStream
    .mapValues(value -> value.toUpperCase())
    .peek((key, value) -> System.out.println("Value: " + value))
    .to(OUTPUT_TOPIC, Produced.with(Serdes.String(), Serdes.String()));
```

Build the topology and start the stream: 

```java
Topology topology = builder.build();

// 4. Create and start the kafka Stream
KafkaStreams streams = new KafkaStreams(topology, properties);

// attach shutdown handler to catch control-c
Runtime.getRuntime().addShutdownHook(new Thread(streams::close));

streams.start();
```

To create the stream, both the topology and the config are required, and keep an eye on the shutdown hook to catch the Ctrl + C command.

Run your application and start to publish messages to the source topic, observe the console output and then 
the destination topic to see the results. 

## Exercise

1. Create a stream that reads numbers from a source topic (`numbers-topic`) and pushes odd numbers to `odd-number-topic` and 
even numbers to `even-numbers-topic`

2. Create a stream that reads numbers from a source topic (`numbers-topic`) and pushes a message to three different topic, as of **FizzBuzz** game:
* If the number is divisible by 3: "Fizz" -> `fizz-topic`
* If the number is divisible by 5: "Buzz" -> `buzz-topic`
* If the number is divisible by 3,5: "FizzBuzz" -> `fizz-buzz-topic`

## Congratulations

Perfect, you made your first steps to data streaming and made your first application.

## Related Documents

* [Kafka Stream Documentation](https://kafka.apache.org/documentation/streams/)
