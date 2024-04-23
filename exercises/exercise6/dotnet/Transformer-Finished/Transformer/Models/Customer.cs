using Avro;
using Avro.Specific;

namespace Transformer.Models;

public class Customer : ISpecificRecord
{
    public Guid Id { get; private set; }

    public string Username { get; set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }

    public Address DefaultDeliveryAddress { get; private set; }
    public Address DefaultBillingAddress { get; private set; }

    public Customer()
    {
    }

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

    public virtual object Get(int fieldPos)
    {
        switch (fieldPos)
        {
            case 0: return DefaultBillingAddress;
            case 1: return DefaultDeliveryAddress;
            case 2: return Email;
            case 3: return FirstName;
            case 4: return Id;
            case 5: return LastName;
            case 6: return Username;
            default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
        }

        ;
    }

    public virtual void Put(int fieldPos, object fieldValue)
    {
        switch (fieldPos)
        {
            case 0:
                DefaultBillingAddress = (Address)fieldValue;
                break;
            case 1:
                DefaultDeliveryAddress = (Address)fieldValue;
                break;
            case 2:
                Email = (string)fieldValue;
                break;
            case 3:
                FirstName = (string)fieldValue;
                break;
            case 4:
                Id = (Guid)fieldValue;
                break;
            case 5:
                LastName = (string)fieldValue;
                break;
            case 6:
                Username = (string)fieldValue;
                break;
            default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
        }

        ;
    }

    public Schema Schema => Schema.Parse(
        @"{""type"":""record"",""name"":""Customer"",""namespace"":""Transformer.Models"",""fields"":[{""name"":""DefaultBillingAddress"",""type"":{""type"":""record"",""name"":""Address"",""namespace"":""Transformer.Models"",""fields"":[{""name"":""City"",""type"":""string""},{""name"":""Lien1"",""type"":""string""},{""name"":""Zipcode"",""type"":""string""}]}},{""name"":""DefaultDeliveryAddress"",""type"":""Address""},{""name"":""Email"",""type"":""string""},{""name"":""FirstName"",""type"":""string""},{""name"":""Id"",""type"":{""type"":""string"",""logicalType"":""uuid""}},{""name"":""LastName"",""type"":""string""},{""name"":""Username"",""type"":""string""}]}");
}
