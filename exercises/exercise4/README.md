# CDC & Debezium 

## Spin up a Debezium Connect Container

To use the Debezium connectors we need to have a custom docker file in which 
the Postgres Connector is installed or is accessible in that container. This is how the [Dockerfile](./connect/Dockerfile) will look like: 

```dockerfile
FROM confluentinc/cp-kafka-connect:7.6.1

RUN confluent-hub install --no-prompt debezium/debezium-connector-postgresql:2.5.3
RUN confluent-hub install --no-prompt confluentinc/kafka-connect-avro-converter:latest

```

In the above-mentioned file, two libraries are installed, one is the [debezium source connector for postgres](https://debezium.io/documentation/reference/stable/connectors/postgresql.html) 
and the other one is AvroConverter for the connector to use to write to the kafka topics

Afterward, you need to configre the Kafka Connect container to use this image: 

```yaml
  connect:
    image: local-image/cc-ws-2024-debezium-connect-jdbc:latest
    build:
      context: .
      dockerfile: ./connect/Dockerfile
    container_name: connect
    depends_on:
      - broker
      - schema-registry
      - postgresql
    ports:
      - "8083:8083"
    environment:
      CONNECT_BOOTSTRAP_SERVERS: broker:29092 # this should refer to the advertised listeners on the broker with PLAINTEXT
      CONNECT_REST_PORT: 8083
      CONNECT_NAME: "connect-cc-ws-2024"
      CONNECT_REST_ADVERTISED_HOST_NAME: "cc-ws-connect"
      CONNECT_GROUP_ID: compose-kafka-connect-group
      
      CONNECT_CONFIG_STORAGE_TOPIC: _kafka-connect-configs
      CONNECT_CONFIG_STORAGE_REPLICATION_FACTOR: 1
      CONNECT_CONFIG_STORAGE_PARTITIONS: 25
      CONNECT_OFFSET_STORAGE_TOPIC: _kafka-connect-offsets
      CONNECT_OFFSET_STORAGE_PARTITIONS: 25
      CONNECT_OFFSET_FLUSH_INTERVAL_MS: 10000
      CONNECT_OFFSET_STORAGE_REPLICATION_FACTOR: 1
      CONNECT_STATUS_STORAGE_TOPIC: _kafka-connect-status
      CONNECT_STATUS_STORAGE_PARTITIONS: 25
      CONNECT_STATUS_STORAGE_REPLICATION_FACTOR: 1
      
      CONNECT_PLUGIN_PATH: "/usr/share/java,/usr/share/confluent-hub-components"
      
      CONNECT_KEY_CONVERTER: io.confluent.connect.avro.AvroConverter
      CONNECT_VALUE_CONVERTER: io.confluent.connect.avro.AvroConverter

      CONNECT_INTERNAL_KEY_CONVERTER: io.confluent.connect.avro.AvroConverter
      CONNECT_INTERNAL_VALUE_CONVERTER: io.confluent.connect.avro.AvroConverter

      CONNECT_KEY_CONVERTER_SCHEMA_REGISTRY_URL: http://schema-registry:8081
      CONNECT_VALUE_CONVERTER_SCHEMA_REGISTRY_URL: http://schema-registry:8081
      
      CONNECT_LOG4J_ROOT_LOGLEVEL: "INFO"
      CONNECT_LOG4J_LOGGERS: "org.apache.kafka.connect.runtime.rest=WARN,org.reflections=ERROR"
    
    networks:
      - kafka_network
```
The important part of this file is the `build` part, that uses the Dockerfile to create the container.

For this workshop, and the development setup, the [Debezium/Postgres](https://hub.docker.com/r/debezium/postgres) images is used, and in that some configurations 
are already enabled to make the CDC setup easier.

In the [Initialization Script](./scripts/database/initialize-database.sql) for the database one line needs to be added to configure postgres 
to populate the before state of the  records that are changed.

```sql
ALTER TABLE public.customers REPLICA IDENTITY FULL;
```

## Create the Connector

Like the previous sample, you could uise http requests to create connectors:

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

Activate CDC for the `public.orders` table and create a separate connector for it

## Congratulations

Great work! So far we have activated `CDC` and created a `Debezium Postgres Connector` that captures the changes 
and pushes them automatically to a target topic, when **any field** in your record changes. 

## Related Documents

[Debezium Architecture](https://debezium.io/documentation/reference/stable/architecture.html)
[Debezium Docker Images](https://hub.docker.com/search?q=debezium%2F)
[Debezium Connector for PostgreSQL](https://debezium.io/documentation/reference/stable/connectors/postgresql.html)
[Debezium Connector PostgreSQL Example Configuration](https://debezium.io/documentation/reference/stable/connectors/postgresql.html#postgresql-example-configuration)
