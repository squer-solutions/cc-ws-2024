using CustomerManagement.Backend.Configurations;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CustomerManagement.Backend.HostedServices;

public class MongoInitializerService(MongoClient mongoClient, IOptions<DatabaseSettings> mongoSettings) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
        await database.CreateCollectionAsync(mongoSettings.Value.CustomersCollectionName,
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
