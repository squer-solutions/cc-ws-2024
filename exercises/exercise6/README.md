# Putting it Altogether

In the [Exercise 4](../exercise4/README.md), we have learned how to set up a CDC pipeline and in 
[Exercise 5](../exercise5) about what is Kafka Streams and how they are working. 

In this Exercise we will put all of them together to push the changes from the 
[legacy application](../../legacy-app/SQUER.Workshop.LegacyApp/Dockerfile) to our new services.

To bring up all your environments use this [docker-compose file](docker-compose.yaml).

**Reminder**: open terminal in `exercise6` path and type the following command: 

```bash
docker compose up -d --build 
```

Then you need to activate the CDC for `customers` table:

```http request
### Create a SOURCE connector from Postgres
PUT {{CONNECTORS}}/debezium_source_connector_customers/config
Content-Type: application/json

{
  "tasks.max": "1",
  "connector.class": "io.debezium.connector.postgresql.PostgresConnector",
  "database.hostname": "postgres",
  "database.port": "5432",
  "database.user": "postgres",
  "database.password": "postgres",
  "database.dbname": "squer_db",
  
  "snapshot.mode": "initial",
  
  "table.include.list": "public.customers", 
  "topic.prefix": "cdc",

  "key.converter": "io.confluent.connect.avro.AvroConverter",
  "key.converter.schema.registry.url": "http://schema-registry:8081",
  "value.converter.schemas.enable": "false",
  "value.converter": "io.confluent.connect.avro.AvroConverter",
  "value.converter.schema.registry.url": "http://schema-registry:8081"  
}

```

**PS:** For framework dependent materials check out the [java](./java/README.md) and [dotnet](./dotnet/README.md) subfolders. 