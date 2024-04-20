# Kafka Connect 

## Spin up a Database

To add a postgres database to your existing docker compose files, add the following part to them:

```yaml
  postgresql:
    image: postgres:latest
    container_name: postgres
    environment:
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    ports:
      - "5432:5432"
    volumes:
      - ./scripts/database/initialize-database.sql:/docker-entrypoint-initdb.d/initialize-database.sql
    networks:
      - kafka_network
```

Use a user interface of choice if you want to explore the database, to connect using terminal: 

```bash
docker exec -it postgres bash -c 'psql -U $POSTGRES_USER $POSTGRES_DB'
```

then run you sql queries: 

```bash
select * from customers;
```

```bash
select * from orders;
```

## Create Kafka Connect Image

To create the Kafka Connect you need to have the following in your docker-compose file:

```yaml
  connect:
    image: local-image/cc-ws-2024-kafka-connect-jdbc:latest
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
      CONNECT_BOOTSTRAP_SERVERS: broker:29092 # this should refer to the advertised listeners on the broker
      CONNECT_REST_PORT: 8083
      CONNECT_REST_ADVERTISED_HOST_NAME: "connect"
      CONNECT_GROUP_ID: compose-kafka-connect-group
      CONNECT_CONFIG_STORAGE_TOPIC: _kafka-connect-configs
      CONNECT_OFFSET_STORAGE_TOPIC: _kafka-connect-offsets
      CONNECT_STATUS_STORAGE_TOPIC: _kafka-connect-status
      CONNECT_CONFIG_STORAGE_REPLICATION_FACTOR: "1"
      CONNECT_OFFSET_STORAGE_REPLICATION_FACTOR: "1"
      CONNECT_STATUS_STORAGE_REPLICATION_FACTOR: "1"
      CONNECT_PLUGIN_PATH: "/usr/share/java,/usr/share/confluent-hub-components"
      CONNECT_KEY_CONVERTER: org.apache.kafka.connect.storage.StringConverter
      CONNECT_VALUE_CONVERTER: org.apache.kafka.connect.json.JsonConverter
      CONNECT_LOG4J_ROOT_LOGLEVEL: "INFO"
      CONNECT_LOG4J_LOGGERS: "org.apache.kafka.connect.runtime.rest=WARN,org.reflections=ERROR"
    networks:
      - kafka_network
```

Now you could run curl or http requests against the worker inside the kafka connect container:

This request will return a list of active connectors in this container

```http request
GET http://localhost:8083/connectors
```

To create a **source connector** run the following command: 

```http request
PUT http://localhost:8083/connectors/jdbc_source_connector_customers/config
Content-Type: application/json

{
  "tasks.max": "1",
  "connector.class": "io.confluent.connect.jdbc.JdbcSourceConnector",
  "connection.url": "jdbc:postgresql://postgres:5432/squer_db",
  "connection.user": "postgres",
  "connection.password": "postgres",
  
  "mode": "timestamp",
  "timestamp.column.name": "ts",
  
  "table.whitelist": "customers", 
  "topic.prefix": "postgres_customers_",

  "key.converter": "org.apache.kafka.connect.storage.StringConverter",
  "value.converter": "io.confluent.connect.avro.AvroConverter",
  "value.converter.schemas.enable": "false",
  "value.converter.schema.registry.url": "http://schema-registry:8081"
}

```

## Control Center

It would be nice if there were a way to be able to see all these information in one place as a dashboard, well, there is,
add the following lines in your docker-compose file and run `docker compose up`

```yaml
  control-center:
    image: confluentinc/cp-enterprise-control-center:7.6.1
    hostname: control-center
    container_name: control-center
    networks:
      - kafka_network
    depends_on:
      - broker
      - schema-registry
      - connect
    ports:
      - "9021:9021"
    environment:
      CONTROL_CENTER_BOOTSTRAP_SERVERS: 'broker:29092'
      CONTROL_CENTER_CONNECT_HEALTHCHECK_ENDPOINT: '/connectors'
      CONTROL_CENTER_SCHEMA_REGISTRY_URL: "http://schema-registry:8081"
      CONTROL_CENTER_REPLICATION_FACTOR: 1
      CONTROL_CENTER_INTERNAL_TOPICS_PARTITIONS: 1
      CONTROL_CENTER_MONITORING_INTERCEPTOR_TOPIC_PARTITIONS: 1
      CONFLUENT_METRICS_TOPIC_REPLICATION: 1
      PORT: 9021
```

Now you coudld navigate around and see topics, connectors, etc.

if you check carefully you will see that there are messages in you topic;
go to your postgres database update a customer or insert a new one, then get back to topics and refresh, 
there should be a new message.

Take a careful look, what is weird? Yes, That's right, the messages do not have `key`, here transformers could help to
convert a column into a message key.

Drop the existing connector first:

```http request
DELETE http://localhost:8083/connectors/jdbc_source_connector_customers
```

and add the following lines to the `body` of the curl/http command for creating a new connector:

```json
"transforms": "copyFieldToKey, extractKeyFromStruct",
"transforms.copyFieldToKey.type": "org.apache.kafka.connect.transforms.ValueToKey",
"transforms.copyFieldToKey.fields": "user_name",
"transforms.extractKeyFromStruct.type": "org.apache.kafka.connect.transforms.ExtractField$Key",
"transforms.extractKeyFromStruct.field": "user_name"
```

Here we set a pipeline of transformers, an string of comma-separated values, then for each transfomer in the pipeline 
we have defined what it should do.

The first one copies the value of the specified field into a  struct, that looks like 
`Struct{user_name=<value_of_the_column>}` and the second one extracts the field from this struct.

Now, again change something in the postgres data, and compare the result with the previous messages.

## Exercise

1. Create a JDBC source connector for the orders table. Use `customer_id` as message key.
2. Use a mask transformer to [mask](https://kafka.apache.org/documentation.html#org.apache.kafka.connect.transforms.MaskField)
`ssn` field with `xxxx-xxxxxx`

<details>

<summary>If you want to see the solution and not figuring out by yourself</summary>

You could change the `transforms` pipeline, and add settings for the new `maskSsn` transformer:  

```json
"transforms": "copyFieldToKey, extractKeyFromStruct, maskSsn",

"transforms.maskSsn.type": "org.apache.kafka.connect.transforms.MaskField$Value",
"transforms.maskSsn.fields": "ssn",
"transforms.maskSsn.replacement": "xxxx-xxxxxx"
```
</details>

## Congratulations

Great work! So far we have created a `JDBC Source Connector` that pushes messages automatically to a topic,
when the `ts` filed in the database gets updated! 

## Related Documents

* [Run Postgres on Docker](https://hub.docker.com/_/postgres)
* [Kafka Connect](https://docs.confluent.io/platform/current/connect/index.html)
* [Install Connectors using Confluent Hub](https://docs.confluent.io/platform/current/connect/install.html)
* [Kafka Connect REST Interface](https://docs.confluent.io/platform/current/connect/references/restapi.html)
* [Kafka Connectors](https://docs.confluent.io/platform/current/connect/kafka_connectors.html)
* [JDBC Source & Sink Connector](https://docs.confluent.io/kafka-connectors/jdbc/current/source-connector/overview.html)
* [Included Transformations in Kafka](https://kafka.apache.org/documentation.html#connect_included_transformation)
* [Control Center Documentation](https://docs.confluent.io/platform/current/platform-quickstart.html)
* [Docker Image Configuration Reference for Confluent Platform](https://docs.confluent.io/platform/current/installation/docker/config-reference.html#required-ak-configurations-for-kraft-mode)
