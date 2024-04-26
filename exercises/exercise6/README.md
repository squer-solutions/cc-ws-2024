# Putting it Altogether

In [exercise 4](../exercise4/README.md), we have learned how to set up a CDC pipeline and in 
[exercise 5](../exercise5) about what Kafka Streams is and how they are working. 

In this exercise we will put all of them together to push the changes from the 
[legacy application](../../legacy-app/SQUER.Workshop.LegacyApp/Dockerfile) to our new services.

To bring up all your environments use this [docker-compose file](docker-compose.yaml).

**Reminder**: open your terminal in `exercise6` path and type the following command: 

```bash
docker compose up -d --build 
```

Then you need to activate CDC for `customers` table:

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
```bash
curl -X PUT \
  localhost:8083/connectors/debezium_source_connector_customers/config \
  -H 'Content-Type: application/json' \
  -v \
  -d '{
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
}'
```

# Next steps
For further instructions, please refer to the README in the dotnet or java folder:
* .NET: [README](dotnet/README.md)
* Java: [README](java/README.md)


**PS:** To see the architecture of the system navigate to http://localhost:7070

**PS:** For framework dependent materials check out the [java](./java/README.md) and [dotnet](./dotnet/README.md) subfolders. 
