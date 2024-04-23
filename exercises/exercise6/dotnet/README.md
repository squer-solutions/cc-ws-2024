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

Take the schema property value out and create `avdl/customers-v1-avsc` file.

To generate `C#` classes representing this schema file we need some `dotnet` tools to be installed: 

```bash
dotnet tool install --global Apache.Avro.Tools --version 1.11.3
```

This tool extends the command line with a new command `avrogen`, run `avrogen --help` to see its parameters and flags; 
we will use this tool to generate `C#` classes for the avro schemas.

Download the schema for the `Key` as well:

```http request
### Get the schema by its id - Key Schema
GET http://localhost:8081/schemas/ids/1
```

Now, we could generate the required `C#` files, put the downloaded schema files in the following folder: `exercise6/dotnet/TheNewApplication/Transformer/avdl/`
and run the following commands: 


```bash
avrogen -s ./dotnet/TheNewApplication/Transformer/avdl/cdc-customers-v1.avsc ./dotnet/TheNewApplication/Transformer/Generated/ --skip-directories 
# --namespace cdc.public.customers:Legacy.Cdc.Generated
```

```bash
avrogen -s ./dotnet/TheNewApplication/Transformer/avdl/cdc-customers-key-v1.avsc ./dotnet/TheNewApplication/Transformer/Generated/ --skip-directories 
# --namespace cdc.public.customers:Legacy.Cdc.Generated
```

Now, we are ready to consume the events from the `cdc.public.customers` topic using Kafka Streams: 

```bash
dotnet add package Streamiz.Kafka.Net.SchemaRegistry.SerDes.Avro --version 1.5.1
```

Follow the steps you learned in the [exercise 5](../../exercise5/dotnet/README.md) to create you Kafka Stream.
If you want to create the destination topic for this application, by hand, checkout [exercise 2](../../exercise2/README.md)

To Serialize the `Customer` and `Address` classes we should tell the AvroSerializer what is the schema for those contracts, 
and those classes should implement the `ISpecificRecord` interface. We could do that manually, or in some automated way like above!

<details>

<summary>Generate Schema from `C#` classes</summary>

To generate schemas from `C#` classes, you need another dotnet tool,

```bash
dotnet tool install --global Chr.Avro.Cli --version 10.2.4
```

Run the following command to generate the schema and add it to the `avdl` folder: 

```bash
dotnet avro create --type Transformer.Models.Customer --assembly dotnet/TheNewApplication/Transformer/bin/Debug/net8.0/Transformer.dll
```

**PS:** Bear in mind, since the command is using an assembly, make sure you have run the build on the project before running the previous command 


```bash
avrogen -s ./dotnet/TheNewApplication/Transformer/avdl/Customer-Transformer.avsc ./dotnet/TheNewApplication/Transformer/TemporaryGeneratedCode/ --skip-directories
```

You could now regenerate the C# classes and copy the missing ones to your actual class.

</details>

The schema and the classes are already added to the project for the sake of time! 

we are now ready to implement the stream topology:

1. Map the values from the cdc representation to a `Customer` event.
2. Change the old key (`customer_id`) to a new key `username`
3. publish to the destination topic `customer-transformed-topic`


```bash
```


```
dotnet avro create --type Transformer.Models.Customer --assembly dotnet/TheNewApplication/Transformer/bin/Debug/net8.0/Transformer.dll
```

### Related Documents

* [Schema Registry API](https://docs.confluent.io/platform/current/schema-registry/develop/using.html)
* [Apache.Avro.Tools](https://www.nuget.org/packages/Apache.Avro.Tools/)
* [Streamiz Apache Avro SerDes, nuget package](https://www.nuget.org/packages/Streamiz.Kafka.Net.SchemaRegistry.SerDes.Avro)
