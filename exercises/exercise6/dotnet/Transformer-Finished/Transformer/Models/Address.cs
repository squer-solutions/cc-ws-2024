using Ardalis.GuardClauses;
using Avro;
using Avro.Specific;

namespace Transformer.Models;

public partial class Address : ISpecificRecord
{
    public Address(string lien1, string zipcode, string city)
    {
        Lien1 = Guard.Against.NullOrWhiteSpace(lien1);
        Zipcode = Guard.Against.NullOrWhiteSpace(zipcode);
        City = Guard.Against.NullOrWhiteSpace(city);
    }
}
