using Avro;
using Avro.Specific;

namespace Transformer.Models;

public partial class Customer : ISpecificRecord
{
    public static Customer? Create(
        Guid id, string username, string fullname, string email, Address deliveryAddress,
        Address? billingAddress)
    {
        var containsComma = fullname.Contains(',');
        var names = containsComma ? fullname.Split(',') : fullname.Split(' ');

        var firstName = containsComma ? names[^1] : names[0];
        var lastName = containsComma ? names[0] : names[^1];

        return new Customer
        {
            Id = id, Username = username,
            FirstName = firstName, LastName = lastName, Email = email,
            DefaultDeliveryAddress = deliveryAddress,
            DefaultBillingAddress = billingAddress ?? deliveryAddress
        };
    }
}
