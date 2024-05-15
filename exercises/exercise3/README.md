# Kafka Connect

## Building images with docker

Some images in this section need to be built. To do this, simply run docker compose with the `--build` flag:

```bash
docker compose up -d --build
```

## Spin up a Database

In the docker-compose of this exercise, the following container has been added to spin up a postgres database.

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

Use a user interface of choice if you want to explore the database. Alternatively, to connect using the terminal: 

```bash
docker exec -it postgres bash -c 'psql -U $POSTGRES_USER $POSTGRES_DB'
```

To list the all tables:

```bash
\dt
```

Then you can run sql queries:

```bash
select * from customers;
```

```bash
select * from orders;
```

## Create Kafka Connect Image

For adding Kafka Connect, the following has been added to the docker-compose.

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

```bash
curl localhost:8083/connectors
```

Expected response:
```
curl localhost:8083/connectors
[]% # Returns an empty array since no connectors exist at the moment.    
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

```bash
curl -X PUT \
  localhost:8083/connectors/jdbc_source_connector_customers/config \
  -H 'Content-Type: application/json' \
  -v \
  -d '{
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
}'
```
Expected response:
```
> PUT /connectors/jdbc_source_connector_customers/config HTTP/1.1
> Host: localhost:8083
> User-Agent: curl/8.4.0
> Accept: */*
> Content-Type: application/json
> Content-Length: 616
> 
< HTTP/1.1 201 Created
```

## Control Center

While the terminal is an all-powerful tool, sometimes it would be nice to see these information in one place as a dashboard. Thankfully, we can use the Confluent Control Center to do just that. The docker-compose has already beene extended with these lines:

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

Now you can navigate around and see topics, connectors, etc.

When you check carefully you will see that there are messages in your topic. <br>
Go to your postgres database and update a customer or insert a new one, then get back to topics and hit refresh. <br>
If everything is working correctly, you will see a new record with the updated/inserted information.

Take a careful look, what is weird? Yes, That's right, the messages do not have `key`, here transformers could help to
convert a column into a message key.

Drop the existing connector first:

```http request
DELETE http://localhost:8083/connectors/jdbc_source_connector_customers
```

```bash
curl -X DELETE localhost:8083/connectors/jdbc_source_connector_customers -v
```
Expected response:
```
> DELETE /connectors/jdbc_source_connector_customers HTTP/1.1
> Host: localhost:8083
> User-Agent: curl/8.4.0
> Accept: */*
> 
< HTTP/1.1 204 No Content
```

And add the following lines to the `body` of the curl/http command for creating a new connector:

```json
"transforms": "copyFieldToKey, extractKeyFromStruct",
"transforms.copyFieldToKey.type": "org.apache.kafka.connect.transforms.ValueToKey",
"transforms.copyFieldToKey.fields": "user_name",
"transforms.extractKeyFromStruct.type": "org.apache.kafka.connect.transforms.ExtractField$Key",
"transforms.extractKeyFromStruct.field": "user_name"
```

Full request:

```http request
PUT {{CONNECTORS}}/jdbc_source_connector_customers/config
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
  "value.converter.schema.registry.url": "http://schema-registry:8081",

  "transforms": "copyFieldToKey, extractKeyFromStruct",
  "transforms.copyFieldToKey.type": "org.apache.kafka.connect.transforms.ValueToKey",
  "transforms.copyFieldToKey.fields": "user_name",
  "transforms.extractKeyFromStruct.type": "org.apache.kafka.connect.transforms.ExtractField$Key",
  "transforms.extractKeyFromStruct.field": "user_name"
  
}
```

```bash
curl -X PUT \
  localhost:8083/connectors/jdbc_source_connector_customers/config \
  -H 'Content-Type: application/json' \
  -v \
  -d '{
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
  "value.converter.schema.registry.url": "http://schema-registry:8081",
  "transforms": "copyFieldToKey, extractKeyFromStruct",
  "transforms.copyFieldToKey.type": "org.apache.kafka.connect.transforms.ValueToKey",
  "transforms.copyFieldToKey.fields": "user_name",
  "transforms.extractKeyFromStruct.type": "org.apache.kafka.connect.transforms.ExtractField$Key",
  "transforms.extractKeyFromStruct.field": "user_name"
}'
```
Expected response:
```
> PUT /connectors/jdbc_source_connector_customers/config HTTP/1.1
> Host: localhost:8083
> User-Agent: curl/8.4.0
> Accept: */*
> Content-Type: application/json
> Content-Length: 957
>
< HTTP/1.1 201 Created
```

Here we set a pipeline of transformers and string of comma-separated values. Then, for each transfomer in the pipeline 
we have defined what it should do.

The first one copies the value of the specified field into a struct, that looks like 
`Struct{user_name=<value_of_the_column>}` and the second one extracts the field from this struct.

Now, again change something in the postgres data, and compare the result with the previous messages.

## Exercise

1. Create a JDBC source connector for the orders table. Use `customer_id` as message key.
2. Use a mask transformer to [mask](https://kafka.apache.org/documentation.html#org.apache.kafka.connect.transforms.MaskField) the `order_id` field with `xxxx-xxxxxx`

<details>

<summary>If you want to see the solution and not figuring out by yourself</summary>

You could change the `transforms` pipeline, and add settings for the new `maskSsn` transformer:  

```json
"transforms": "copyFieldToKey, extractKeyFromStruct, maskOrderId",
"transforms.maskOrderId.type": "org.apache.kafka.connect.transforms.MaskField$Value",
"transforms.maskOrderId.fields": "order_id",
"transforms.maskOrderId.replacement": "xxxx-xxxxxx"
```
</details>

## Congratulations

Great work! So far we have created a `JDBC Source Connector` that pushes messages automatically to a topic,
when the `ts` field in the database gets updated! 

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
* [Externalize Secrets - Kafka Connect Security](https://docs.confluent.io/platform/current/connect/security.html#externalize-secrets)
