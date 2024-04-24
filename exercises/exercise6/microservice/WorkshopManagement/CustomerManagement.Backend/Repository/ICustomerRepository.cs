using CustomerManagement.Backend.Configurations;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Transformer.Models;

namespace CustomerManagement.Backend.Repository;

public interface ISaveCustomer
{
    Task RegisterCustomer(CustomerDbModel customer, CancellationToken cancellationToken);
}

public interface IFetchCustomer
{
    Task<CustomerDbModel> GetCustomer(Guid id, CancellationToken cancellationToken);
    Task<ICollection<CustomerDbModel>> GetAll(CancellationToken cancellationToken);
}

public interface ICustomerRepository : ISaveCustomer, IFetchCustomer
{
}

public class CustomerRepository : ICustomerRepository
{
    private readonly IMongoCollection<CustomerDbModel> _collection;
    private readonly DatabaseSettings _databaseSettings;

    public CustomerRepository(
        IOptions<DatabaseSettings> databaseSettingsOption, IMongoCollection<CustomerDbModel> collection)
    {
        _collection = collection;
        _databaseSettings = databaseSettingsOption.Value;
    }

    public async Task RegisterCustomer(CustomerDbModel customer, CancellationToken cancellationToken)
    {
        await _collection.ReplaceOneAsync(x => x.Id == customer.Id, customer, new ReplaceOptions()
        {
            IsUpsert = true
        }, cancellationToken);
    }

    public Task<CustomerDbModel> GetCustomer(Guid id, CancellationToken cancellationToken) =>
        _collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

    public async Task<ICollection<CustomerDbModel>> GetAll(CancellationToken cancellationToken) =>
        await _collection.Find(x => true).ToListAsync(cancellationToken);
}
