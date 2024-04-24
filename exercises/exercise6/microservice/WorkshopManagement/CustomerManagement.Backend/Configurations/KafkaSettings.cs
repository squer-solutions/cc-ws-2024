namespace CustomerManagement.Backend.Configurations;

public class KafkaSettings
{
    public const string Section = "Kafka";

    public KafkaSettings()
    {
    }

    public IEnumerable<string> Brokers { get; init; }
    public string SchemaRegistryUrl { get; init; }
    public string TransformerTopic { get; init; }
    public string GroupId { get; set; }
}
