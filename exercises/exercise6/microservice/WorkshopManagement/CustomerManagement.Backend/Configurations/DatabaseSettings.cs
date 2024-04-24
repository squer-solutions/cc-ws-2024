namespace CustomerManagement.Backend.Configurations;

public class DatabaseSettings
{
    public const string Section = nameof(DatabaseSettings);
    public DatabaseSettings()
    {
    }
    
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string CustomersCollectionName { get; set; } = null!;
}
