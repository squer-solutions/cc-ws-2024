# Broker

## Spin up Kafka

Using docker is the easiest way to spin up a local kafka broker on your machine. 

In this [docker compose file](docker-compose.yaml) you will find two services:

1. Broker: this is the kafka broker itself
2. Schema registry: this is required to register the schema of the messages that will be published to the topics of kafka


### Starting the containers
To run this open the `terminal` application, and enter to the folder `exercise1` and run the following command:

```bash
docker compose up -d
```

The command will fetch the images for the kafka broker and schema registry and creates a container for each of them. They are then started in detached mode, meaning you can use your terminal for other tasks.

### Verify their status
To verify the status, you can run
```bash
docker ps
```

The output should look similar to this
```
CONTAINER ID   IMAGE                                   COMMAND                  CREATED         STATUS         PORTS                                                                                  NAMES
a1249a90ad07   confluentinc/cp-schema-registry:7.6.1   "/etc/confluent/dock…"   4 seconds ago   Up 1 second    0.0.0.0:8081->8081/tcp, :::8081->8081/tcp                                              schema-registry
b9803ead4ea1   confluentinc/cp-kafka:7.6.1             "/etc/confluent/dock…"   4 seconds ago   Up 2 seconds   0.0.0.0:9092->9092/tcp, :::9092->9092/tcp, 0.0.0.0:9101->9101/tcp, :::9101->9101/tcp   broker
```

### Entering interactive mode
To enter interactive mode for a specific container, you can run
```bash
docker exec -it <CONTAINER_ID OR NAME> bash
```
This will connect you to a shell inside the docker container

### Stopping the containers
To stop the containers, run 

```bash
docker compose stop
```

### Cleaning up

To remove the containers and clean up resources, run 
```bash
docker compose down
```

## Congratulations

Great work! Now you have a broker running and ready to accept requests!

## Related Documents:

* [Docker Images From Confluent](https://hub.docker.com/search?q=confluentinc%2Fcp)
