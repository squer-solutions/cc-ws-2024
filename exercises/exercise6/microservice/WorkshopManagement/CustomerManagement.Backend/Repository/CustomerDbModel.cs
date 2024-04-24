using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Transformer.Models;

namespace CustomerManagement.Backend.Repository;

public sealed class CustomerDbModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required AddressDbModel DeliveryAddress { get; init; }
    public required AddressDbModel BillingAddress { get; init; }

    private CustomerDbModel()
    {
    }

    public static CustomerDbModel Of(Customer customerEvent) =>
        new()
        {
            Id = customerEvent.Id, FirstName = customerEvent.FirstName, LastName = customerEvent.LastName,
            Username = customerEvent.Username, Email = customerEvent.Email,
            DeliveryAddress = AddressDbModel.Of(customerEvent.DefaultDeliveryAddress),
            BillingAddress = AddressDbModel.Of(customerEvent.DefaultBillingAddress),
        };
}

public sealed class AddressDbModel
{
    public required string Line1 { get; init; }
    public required string ZipCode { get; init; }
    public required string City { get; init; }

    private AddressDbModel()
    {
    }

    public static AddressDbModel Of(Address addressEvent) =>
        new()
        {
            Line1 = addressEvent.Lien1, ZipCode = addressEvent.Zipcode, City = addressEvent.City
        };
}
