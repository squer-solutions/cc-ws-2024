namespace StreamingApp;

public class StreamingAppSettings
{
    public const string ConfigSection = "Settings";
    public StreamingAppSettings()
    {
    }
    
    public required string ApplicationId { get; init; }
    public required IEnumerable<string> BootstrapServers { get; init; }
    public required string SourceTopic { get; init; }
    public required string DestinationTopic { get; init; }
}
