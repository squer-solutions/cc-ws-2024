namespace DefaultNamespace;

public class TransformerConfiguration
{
    public const string Section = "Kafka";

    public TransformerConfiguration()
    {
    }

    public string ApplicationId { get; init; }
    public IEnumerable<string> Brokers { get; init; }
    public string SchemaRegistryUrl { get; init; }
    public string CdcTopic { get; init; }
    public string TransformerTopic { get; init; }
}
