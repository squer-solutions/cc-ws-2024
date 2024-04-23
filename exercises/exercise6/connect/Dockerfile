FROM confluentinc/cp-kafka-connect:7.6.1

RUN confluent-hub install --no-prompt debezium/debezium-connector-postgresql:2.5.3
RUN confluent-hub install --no-prompt confluentinc/kafka-connect-avro-converter:latest

