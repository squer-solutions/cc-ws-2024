# Interact with the Broker

## Spin up the Broker

From [exercise one](../exercise1/README.md) you already know how to spin up a kafka broker, enter the folder `exercise2`
and run the following command: (if you have brought the service down from the previous exercise)

```bash
docker compose up -d
```

After the services are up and running, we need to exec into the running container of the broker:

```bash
docker exec -it broker bash
```

When you are inside the container, run the following command to create your first topic:

```bash
kafka-topics --create --topic my-first-topic --replication-factor 1 --partitions 3 --bootstrap-server localhost:9092
```

Congratulations! You have created your very first topic!

## Producing Messages

For now, to produce messages we will use a simple `console producer` provided by the confluent and kafka for ease of use
and testing purposes.

Use the above-mentioned approach to be in the container's console (if you are not yet)

Run the following command to produce a message into the created topic: 

```bash
kafka-console-producer\
  --topic my-first-topic\
  --bootstrap-server localhost:9092\
  --property "parse.key=true"\
  --property "key.separator=:"

# The options passed via the --producer-property flag indicate that we will provide a key and that the separator to the value will be a semicolon
```

After you hit enter the prompt is still active for the **Next Message**


## Consume Messages

To consume messages, open another terminal, and get into the broker's container:

```bash
docker exec -it broker bash
```

In the console use the following command to consume messages from the beginning of time from the specified topic

```bash
kafka-console-consumer\
  --topic my-first-topic\
  --bootstrap-server localhost:9092\
  --from-beginning\
  --property "key.separator=:"\
  --property "print.key=true" 
  
  # also prints the key on the console, if not specified only the value is shown
```

If you are just interested in the new messages then remove the `--from-beginning` flag from the command.

## Congratulations

Great work! We just created our first topic and produced a message where both its **key** and **value** are of type `string`

## Related Documents

* [Confluent CLI](https://docs.confluent.io/confluent-cli/current/overview.html): The Confluent command-line interface (CLI), `confluent`, enables developers to manage both Confluent Cloud and Confluent Platform
* [kCat](https://github.com/edenhill/kcat): A tool to interact with kafka
