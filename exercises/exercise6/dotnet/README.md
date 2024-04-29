# Putting it Altogether

After bringing up your environment the first thing you need is to download the schema files for the events published by
`Debezium` connector.

The goal for this exercise is to produce messages that conform to a predefined schema that you have already aligned on
with your colleagues: [Customer-Transformed.avsc](./Transformer/Transformer/avdl/Customer-Transformer.avsc)

## Retrieving the schema files

### Option 1: Using the schema registry API

```http request
GET http://localhost:8081/subjects
```

The output should be similar to this:

```bash
[
  "cdc.public.customers-key",
  "cdc.public.customers-value"
]
```

Checkout the other commands listed in the [schema-registry.http](./Transformer/Transformer/scripts/schema-registry.http)
file; now, you should
be able to fetch the schema for a specific subject and version:

```http request
### GET the schema by its id
GET http://localhost:8081/schemas/ids/1

###
GET http://localhost:8081/schemas/ids/2
```

each of the above requests should provide an output which looks like the following:

```bash
{
  "schema": "{\"type\":\"record\",\"name\":\"Envelope\",\"namespace\":\"cdc.public.customers\",\"fields\":[{\"name\":\"before\",\"type\":[\"null\",{\"type\":\"record\",\"name\":\"Value\",\"fields\":[{\"name\":\"customer_id\",\"type\":{\"type\":\"string\",\"connect.version\":1,\"connect.name\":\"io.debezium.data.Uuid\"}},{\"name\":\"ssn\",\"type\":\"string\"},{\"name\":\"email\",\"type\":\"string\"},{\"name\":\"user_name\",\"type\":\"string\"},{\"name\":\"full_name\",\"type\":\"string\"},{\"name\":\"delivery_address\",\"type\":\"string\"},{\"name\":\"delivery_zipcode\",\"type\":\"string\"},{\"name\":\"delivery_city\",\"type\":\"string\"},{\"name\":\"billing_address\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"billing_zipcode\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"billing_city\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"ts\",\"type\":{\"type\":\"long\",\"connect.version\":1,\"connect.default\":0,\"connect.name\":\"io.debezium.time.MicroTimestamp\"},\"default\":0}],\"connect.name\":\"cdc.public.customers.Value\"}],\"default\":null},{\"name\":\"after\",\"type\":[\"null\",\"Value\"],\"default\":null},{\"name\":\"source\",\"type\":{\"type\":\"record\",\"name\":\"Source\",\"namespace\":\"io.debezium.connector.postgresql\",\"fields\":[{\"name\":\"version\",\"type\":\"string\"},{\"name\":\"connector\",\"type\":\"string\"},{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"ts_ms\",\"type\":\"long\"},{\"name\":\"snapshot\",\"type\":[{\"type\":\"string\",\"connect.version\":1,\"connect.parameters\":{\"allowed\":\"true,last,false,incremental\"},\"connect.default\":\"false\",\"connect.name\":\"io.debezium.data.Enum\"},\"null\"],\"default\":\"false\"},{\"name\":\"db\",\"type\":\"string\"},{\"name\":\"sequence\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"schema\",\"type\":\"string\"},{\"name\":\"table\",\"type\":\"string\"},{\"name\":\"txId\",\"type\":[\"null\",\"long\"],\"default\":null},{\"name\":\"lsn\",\"type\":[\"null\",\"long\"],\"default\":null},{\"name\":\"xmin\",\"type\":[\"null\",\"long\"],\"default\":null}],\"connect.name\":\"io.debezium.connector.postgresql.Source\"}},{\"name\":\"op\",\"type\":\"string\"},{\"name\":\"ts_ms\",\"type\":[\"null\",\"long\"],\"default\":null},{\"name\":\"transaction\",\"type\":[\"null\",{\"type\":\"record\",\"name\":\"block\",\"namespace\":\"event\",\"fields\":[{\"name\":\"id\",\"type\":\"string\"},{\"name\":\"total_order\",\"type\":\"long\"},{\"name\":\"data_collection_order\",\"type\":\"long\"}],\"connect.version\":1,\"connect.name\":\"event.block\"}],\"default\":null}],\"connect.version\":1,\"connect.name\":\"cdc.public.customers.Envelope\"}"
}
```

### Option 2: Via Confluent Control Center

* The quickest way to retrieve the current schema for a topic is by downloading it from the control center
* Open the [ControlCenter](http://localhost:9021/), open the Topics section and find the CDC topic
* Open the tab "Schema", and in the context menu (...), you can download both the key-schema and the value-schema
* Place the schemas in the `src/main/resources/avro` folder

No matter which option you used, you need to save the `json` value/file to
the [avdl directory](./Transformer/Transformer/avdl), do that for both the `value` and `key` schemas and save them with
the following names `cdc-customers-v1-avsc` and `cdc-customers-key-v1.avsc`

## Generate C# classes from Avro Schema Files

To generate `C#` classes representing this schema file we need some `dotnet` tools to be installed:

```bash
dotnet tool install --global Apache.Avro.Tools --version 1.11.3
```

**IMPORTANT:** If for some reason the installed tool is not recognized on your machine try to add it the the `PATH`,
check out [this GitHub issue](https://github.com/dotnet/sdk/issues/9415#issuecomment-406915716)

This tool extends the command line with a new command `avrogen`, run `avrogen --help` to see its parameters and flags;
we will use this tool to generate `C#` classes for the avro schemas.

Download the schema for the `Key` as well,and take the schema property value out and
create `avdl/cdc-customers-key-v1-avsc` file.

Now, we could generate the required `C#` files, put the downloaded schema files in the following
folder: `exercise6/dotnet/Transformer/Transformer/avdl/`
and run the following commands:

```bash
avrogen -s ./dotnet/Transformer/Transformer/avdl/cdc-customers-v1.avsc ./dotnet/Transformer/Transformer/Generated/ --skip-directories 
```

```bash
avrogen -s ./dotnet/Transformer/Transformer/avdl/cdc-customers-key-v1.avsc ./dotnet/Transformer/Transformer/Generated/ --skip-directories 
```

The schema for the new contracts is already located [here](./Transformer/Transformer/avdl); also a generated code is
located on the [Generated folder](./Transformer/Transformer/Models/Generated), but if you want to try
it for yourself run the following:

```bash
avrogen -s ./dotnet/Transformer/Transformer/avdl/Customer-Transformer.avsc ./dotnet/Transformer/Transformer/Models/Generated --skip-directories
```

**PS:** The schema and the classes are already added to the project for the sake of time!

## Topic and schema creation

* Any Kafka producer (that includes a Kafka Streams application) is capable of creating a topic if it does not exist and
  automatically registering the schema with registry
* In general, it is advisable to manually register the schemas beforehand. For the purpose of this execute, you may rely
  on the automatic behaviour.
* However, if you prefer to do it manually, you can follow the steps below

### Manual topic creation and schema registration

Before starting the application we also need to register the new `Customer-Transformed-Topic` schema for
the `customer-transformed-topic`, go to the [Control Center](http://localhost:9021/), click on the **cluster card**,
choose `Topics` from the left menu and click on the `+ Add Topic` button, give it the name `customer-transformed-topic`
and click on the `Create with defaults` button, after the topic is created, go to the `Schema` tab for the topic and for
the `value` click on `Set schema` button, upload the contents of
the [Customer-Transformed.avsc](./transformer/src/main/resources/avro/Customer-Transformer.avsc), and click on
the `Create button`.

## Kafka streams application

Now, we are ready to consume the events from the `cdc.public.customers` topic using Kafka Streams:

```bash
dotnet add package Streamiz.Kafka.Net.SchemaRegistry.SerDes.Avro --version 1.5.1
```

Follow the steps you learned in the [exercise 5](../../exercise5/dotnet/README.md) to create you Kafka Stream.
If you want to create the destination topic for this application, by hand,
checkout [exercise 2](../../exercise2/README.md)

To Serialize the `Customer` and `Address` classes we should tell the `AvroSerializer` what is the schema for those
contracts, and those classes should implement the `ISpecificRecord` interface. Check out the auto generated codes for
the schemas.

we are now ready to implement the stream topology:

1. Map the values from the cdc representation to a `Customer` event, using `MapValues`
2. Change the old key (`customer_id`) to a new key `username`, using `Map` and return `KeyValuePair`
3. publish to the destination topic `customer-transformed-topic`
4. Hint: You will want to serialize the key using a regular `StringSerDes` for the key and the default `SchemaAvroSerDes<T>` for the value.

#### Notes
Try to map the entity on your own first to get a feel how you can work with the generated avro classes.
After you made your own steps and don't feel like typing everything out, you can use the mapping function provided below.
<details>
<summary>SPOILER: Mapping function</summary>

```csharp
private static Customer? MapToCustomer(Envelope envelope)
{
    return Customer.Create(
        Guid.Parse(envelope.after.customer_id), envelope.after.user_name,
        envelope.after.full_name,
        envelope.after.email,
        new Address(envelope.after.delivery_address, envelope.after.delivery_zipcode,
            envelope.after.delivery_city),
        string.IsNullOrEmpty(envelope.after.billing_address)
            ? null
            : new Address(envelope.after.billing_address, envelope.after.billing_zipcode,
                envelope.after.billing_city)
    );
}
```
</details>

Please try to solve this exercise yourself to get the most of it.
<details>
<summary>Should you get stuck, you can have a look at the solution here:</summary>
<details>

<summary>After generating the cdc representative C# classes, use the following for the stream topology</summary>

```csharp
var kafkaConfig = _options.CurrentValue;
        
// 1. Define the configuration
// 2. Create Serde instances
var config = new StreamConfig<SchemaAvroSerDes<Key>, SchemaAvroSerDes<Value>>
{
    ApplicationId = kafkaConfig.ApplicationId,
    BootstrapServers = string.Join(",", kafkaConfig.Brokers),
    AutoOffsetReset = AutoOffsetReset.Earliest,

    SchemaRegistryUrl = kafkaConfig.SchemaRegistryUrl,
    AutoRegisterSchemas = true
};

// 3. Build the Topology
var streamBuilder = new StreamBuilder();
var cdcStream = streamBuilder.Stream(kafkaConfig.CdcTopic,
    new SchemaAvroSerDes<Key>(), new SchemaAvroSerDes<Envelope>(), named: "Transformer - Import");

cdcStream.MapValues(MapToCustomer)
    .Map((_, v) => KeyValuePair.Create(v!.Username, v))
    .To<StringSerDes, SchemaAvroSerDes<Customer>>(kafkaConfig.TransformerTopic,
        named: "Transformer Export");

var topology = streamBuilder.Build();

// 4. Create and start the kafka Stream
var stream = new KafkaStream(topology, config);
await stream.StartAsync(stoppingToken);
```

</details>

## Congratulations

Perfect, you completed the whole pipeline for transferring data from your legacy app to the new one,
now open the portals for the [Legacy App](http://localhost:9091) and
the [New Workshop Management App](http://localhost:9090),
change or add some data in the legacy app, and you should see them near realtime in the new app.

## Related Documents

* [Schema Registry API usage Examples](https://docs.confluent.io/platform/current/schema-registry/develop/using.html)
* [Register a new version of a schema](https://docs.confluent.io/platform/current/schema-registry/develop/using.html#register-a-new-version-of-a-schema-under-the-subject-kafka-value)
* [Apache.Avro.Tools](https://www.nuget.org/packages/Apache.Avro.Tools/)
* [Creating schemas from .NET types](https://engineering.chrobinson.com/dotnet-avro/guides/cli-create/)
* [Streamiz Apache Avro SerDes, nuget package](https://www.nuget.org/packages/Streamiz.Kafka.Net.SchemaRegistry.SerDes.Avro)
* [Add AVDL Support to a .NET Project](https://dev.to/cainux/add-avdl-support-to-a-net-project-1hoo)
* [Avro Specific .NET Example](https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/examples/AvroSpecific/README.md)
* [Avro Generic .NET Example](https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/examples/AvroGeneric/Program.cs)
* [Decoupling Systems with Apache Kafka, Schema Registry and Avro](https://www.confluent.io/blog/decoupling-systems-with-apache-kafka-schema-registry-and-avro/)
* [KafkaFlow Documentation](https://farfetch.github.io/kafkaflow/docs/)
* [Introduction to KafkaFlow](https://guiferreira.me/archive/2023/a-better-way-to-kafka-event-driven-applications-with-csharp/)



<hr />

<details>

<summary>
Generating Avro Schema files from C# class **We do not recommend this approach, this section is just for your information**
</summary>

```bash
dotnet tool install --global Chr.Avro.Cli --version 10.2.4
```

Run the following command to generate the schema and add it to the `avdl` folder:

```bash
dotnet avro create --type Transformer.Models.Customer --assembly dotnet/Transformer/Transformer/bin/Debug/net8.0/Transformer.dll
```

**PS:** Bear in mind, since the command is using an assembly, make sure you have run the build on the project before
running the previous command

</details>

<hr />
