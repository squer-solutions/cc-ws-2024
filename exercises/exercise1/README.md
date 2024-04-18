# Broker

## Spin up Kafka

To have a kafka broker on your local machine, you could use docker and for that you need a docker compose file.

In this [docker compose file](docker-compose.yaml) you could find two services:

1. broker: this is the kafka broker itself
2. schema registry: this is required to register the schema of the messages tha will be published to the topics of kafka

To run this open the `terminal` application, and enter to the folder `exercise1` and run the following command:

```bash
docker compose up -it
```

The command will fetch the images for the kafka broker and schema registry and creates a container for each of them, 
they are running in interactive mode, you could see all the logs on the terminal for each service, as soon as you quite 
terminal or hit `Ctrl + C` those containers will stop. 

To clean up resources, run 

```bash
docker compose down
```

if you want to run the containers in the background and use the terminal afterwards, run: 

```bash
docker compose up -d
```

This will run the containers in detached mode from terminal.

## Congratulations

Great work! Now you have a broker running and ready to accept requests!

## Related Documents:

* [Docker Images From Confluent](https://hub.docker.com/search?q=confluentinc%2Fcp)
