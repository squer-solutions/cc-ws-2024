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

Take the schema property value out and create `avdl/cdc-customers-v1-avsc` file.

To generate `C#` classes representing this schema file we need some `dotnet` tools to be installed:

```bash
dotnet tool install --global Apache.Avro.Tools --version 1.11.3
```

This tool extends the command line with a new command `avrogen`, run `avrogen --help` to see its parameters and flags;
we will use this tool to generate `C#` classes for the avro schemas.

Download the schema for the `Key` as well,and take the schema property value out and
create `avdl/cdc-customers-key-v1-avsc` file.

```http request
### Get the schema by its id - Key Schema
GET http://localhost:8081/schemas/ids/1
```

Now, we could generate the required `C#` files, put the downloaded schema files in the following
folder: `exercise6/dotnet/Transformer/Transformer/avdl/`
and run the following commands:

```bash
avrogen -s ./dotnet/Transformer/Transformer/avdl/cdc-customers-v1.avsc ./dotnet/Transformer/Transformer/Generated/ --skip-directories 
```

```bash
avrogen -s ./dotnet/Transformer/Transformer/avdl/cdc-customers-key-v1.avsc ./dotnet/Transformer/Transformer/Generated/ --skip-directories 
```

The schema for the new contracts is already located at the `exercise6/dotnet/Transformer/Transformer/avdl/`; also a
generated code is located in the
`exercise6/dotnet/Transformer/Transformer/Models/Generated`, but if you want to try it for yourself run the following:

```bash
avrogen -s ./dotnet/Transformer/Transformer/avdl/Customer-Transformer.avsc ./dotnet/Transformer/Transformer/Models/Generated --skip-directories
```

**PS:** The schema and the classes are already added to the project for the sake of time!

One more thing, we also need to register the new `Customer-Transformed-Topic` schema for
the `customer-transformed-topic`,
go to the [Control Center](http://localhost:9021/), click on the **cluster card**, choose `Topics` from the left menu
and click on the `+ Add Topic` button, give it the name `customer-transformed-topic` and click on
the `Create with defaults` button,
after the topic is created, go to the `Schema` tab for the topic and for the `value` click on `Set schema` button,
upload the
contents of the [Customer-Transformed.avsc](./transformer/src/main/resources/avro/Customer-Transformer.avsc), and click
on
the `Create button`. The Avro Serializer will automatically register the schema if it does not found it in the schem
registry,
however, it is always a good practice to create your topics and their schemas, either for the value or key, beforehand,
so you have full control over the configuration and how they should be set up.

<hr />

<details>

<summary>

It is always better to create the schema files by hand and then generate the events from this contract,
however, to generate schema from `C#` classes, you need another dotnet tool. **We do not recommend this approach, this
section is just for your information**

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

<details>

<summary>After generating the cdc representative C# classes, use the following for the stream topology</summary>

```csharp
cdcStream.MapValues(envelope => Customer.Create(
                Guid.Parse(envelope.after.customer_id), envelope.after.user_name,
                envelope.after.full_name,
                envelope.after.email,
                new Address(envelope.after.delivery_address, envelope.after.delivery_zipcode,
                    envelope.after.delivery_city),
                string.IsNullOrEmpty(envelope.after.billing_address)
                    ? null
                    : new Address(envelope.after.billing_address, envelope.after.billing_zipcode,
                        envelope.after.billing_city)
            ))
            .Map((_, v) => KeyValuePair.Create(v!.Username, v))
            .To<StringSerDes, SchemaAvroSerDes<Customer>>(kafkaConfig.TransformerTopic,
                named: "Transformer Export");
```

</details>


## Congratulations

Perfect, you completed the whole pipeline for transferring data from your legacy app to the new one, 
now open the portals for the [Legacy App](http://localhost:9091) and the [New Workshop Management App](http://localhost:9090),
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
