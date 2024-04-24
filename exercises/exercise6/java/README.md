
```bash
java -jar ./java/transformer/avro-tools-1.11.3.jar compile schema ./java/transformer/src/main/resources/avro/cdc-customers-v1.avsc ./java/transformer/src/main/java
java -jar ./java/transformer/avro-tools-1.11.3.jar compile schema ./java/transformer/src/main/resources/avro/cdc-customers-key-v1.avsc ./java/transformer/src/main/java
java -jar ./java/transformer/avro-tools-1.11.3.jar compile schema ./java/transformer/src/main/resources/avro/Customer-Transformer.avsc ./java/transformer/src/main/java
```
