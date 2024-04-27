# CDC & Debezium 

## Spin up a Debezium Connect Container
Before starting, have a close look at the new docker compose for this exercise.

To use the Debezium connectors we need to have a custom docker file in which 
the Postgres Connector is installed or is accessible in that container. This is how the [Dockerfile](./connect/Dockerfile) will look like:

These changes are already reflected in the docker-compose of this exercise.

```dockerfile
FROM confluentinc/cp-kafka-connect:7.6.1

RUN confluent-hub install --no-prompt debezium/debezium-connector-postgresql:2.5.3
RUN confluent-hub install --no-prompt confluentinc/kafka-connect-avro-converter:latest

```

In the above-mentioned file, two libraries are installed, one is the [debezium source connector for postgres](https://debezium.io/documentation/reference/stable/connectors/postgresql.html) 
and the other one is AvroConverter for the connector to use to write to the kafka topics

For this workshop, and the development setup, the [Debezium/Postgres](https://hub.docker.com/r/debezium/postgres) images is used, and in that some configurations 
are already enabled to make the CDC setup easier.

In the [Initialization Script](./scripts/database/initialize-database.sql) for the database one line has been added to configure postgres 
to populate the `before` state in the captured change events.

```sql
ALTER TABLE public.customers REPLICA IDENTITY FULL;
```

## Create the Connector

Like the previous sample, you can use http requests to create connectors:

```http request
### Create a SOURCE connector from Postgres
PUT http://localhost:8083/connectors/debezium_source_connector_customers/config
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
  "topic.prefix": "cdc_customers_",

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
  "topic.prefix": "cdc_customers_",

  "key.converter": "io.confluent.connect.avro.AvroConverter",
  "key.converter.schema.registry.url": "http://schema-registry:8081",
  "value.converter.schemas.enable": "false",
  "value.converter": "io.confluent.connect.avro.AvroConverter",
  "value.converter.schema.registry.url": "http://schema-registry:8081"  
}'
```
Expected response:
```
> PUT /connectors/debezium_source_connector_customers/config HTTP/1.1
> Host: localhost:8083
> User-Agent: curl/8.4.0
> Accept: */*
> Content-Type: application/json
> Content-Length: 690
> 
< HTTP/1.1 201 Created
```

Connect to your database, either via an IDE of choice or terminal, select a customer and change its data: 

```bash
docker exec -it postgres bash -c 'psql -U $POSTGRES_USER $POSTGRES_DB'
```

```postgresql
select * from public.customers;
```

```postgresql
UPDATE public.customers SET delivery_address = 'Teststrasse 23/2', delivery_zipcode = '1210', delivery_city = 'Wien' WHERE customer_id = '31e7a241-d570-4961-981d-4aea2b20d22e'
```

Now, you could check the messages via the control center. Make sure the message is populated correctly.

## Exercise
Your task is to create a new connector that captures changes from the orders `public.orders` table.

To also capture `before-events`, don't forget to set the `REPLICA IDENTITY` to `FULL`

## Congratulations

Great work! So far we have activated `CDC` and created a `Debezium Postgres Connector` that captures the changes 
and pushes them automatically to a target topic, when **any field** in your record changes. 

## Related Documents

* [Debezium Architecture](https://debezium.io/documentation/reference/stable/architecture.html)
* [Debezium Docker Images](https://hub.docker.com/search?q=debezium%2F)
* [Debezium PostgreSQL CDC Source Connector](https://www.confluent.io/hub/debezium/debezium-connector-postgresql)
* [Debezium Connector for PostgreSQL](https://debezium.io/documentation/reference/stable/connectors/postgresql.html)
* [Debezium Connector PostgreSQL Example Configuration](https://debezium.io/documentation/reference/stable/connectors/postgresql.html#postgresql-example-configuration)
* [PostgreSQL CDC Source (Debezium) Connector for Confluent Cloud](https://docs.confluent.io/cloud/current/connectors/cc-postgresql-cdc-source-v2-debezium/cc-postgresql-cdc-source-v2-debezium.html)
* [List of Available Connectors](https://docs.confluent.io/cloud/current/connectors/index.html)
