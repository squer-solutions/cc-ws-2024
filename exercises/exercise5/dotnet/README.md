# Getting Started with Streaming Applications

First bring up the Kafka Broker and create a topic there, named `src-topic`. Checkout previous exercises for how to bring up a Kafka Broker, 
bear in mind to expose  a public port for the hosting environment.

To get started add the following nuget package to your project:

```bash
dotnet add package Streamiz.Kafka.Net
```

In the project there is a file `StreamingAppWorker`, it is the background service that will host our code for our data stream.

The first step is to create the configuration and selecting the Serializer and Deserializer for the key and value of the stream should work with;
since for our example both the key and value are string we will use the following settings: 

```csharp
// 1. Define the configuration
// 2. Create Serde instances
var config = new StreamConfig<StringSerDes, StringSerDes>
{
    ApplicationId = _options.ApplicationId,
    BootstrapServers = string.Join(',', _options.BootstrapServers),
    AutoOffsetReset = AutoOffsetReset.Earliest
};
```

The above-code uses option patterns to inject settings in the worker, you should have this section in either
of your `appsettings.json` or `appsettings.Development.json`

```json
"Settings": {
    "ApplicationId": "streaming-service",
    "BootstrapServers": [
    "localhost:9092"
    ],
    "SourceTopic" : "src-topic",
    "DestinationTopic" : "dest-topic"
}
```
The next step would be to create the topology of the stream: 

```csharp
// 3. Build the Topology
var streamBuilder = new StreamBuilder();
var sourceStream = streamBuilder.Stream(_options.SourceTopic, new StringSerDes(), new StringSerDes(), named: "Streaming App - Input");
```

Now, it is time to build the functionalities for the stream, in this example, we just want to:
1. Convert the value to upper case
2. Print it on the Console
3. Push it to a destination topic

```csharp
sourceStream
    .MapValues(value => value.ToUpperInvariant())
    .Peek((_, v) => _logger.LogInformation(v))
    .To(_options.DestinationTopic, named: "Streaming App - Output")
    ;
```

Build the topology and start the stream: 

```csharp
var topology = streamBuilder.Build();

// 4. Create and start the kafka Stream
var stream = new KafkaStream(topology, config);
await stream.StartAsync(stoppingToken);
```

To create the stream, both the topology and the config are required, and keep an eye to pass the `CancellationToken` to 
the `StartAsync` method.

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

[Kafka Stream Documentation](https://kafka.apache.org/documentation/streams/)
[Streamiz - GitHub](https://github.com/LGouellec/kafka-streams-dotnet): .NET Stream Processing Library for Apache Kafka <sup>TM</sup>
[Streamiz - Documentation](https://lgouellec.github.io/kafka-streams-dotnet/overview.html#)
