using Ardalis.GuardClauses;
using Avro;
using Avro.Specific;

namespace Transformer.Models;

public record Address : ISpecificRecord
{
    public string Lien1 { get; private set; }
    public string Zipcode { get; private set; }
    public string City { get; private set; }

    public Address(string lien1, string zipcode, string city)
    {
        Lien1 = Guard.Against.NullOrWhiteSpace(lien1);
        Zipcode = Guard.Against.NullOrWhiteSpace(zipcode);
        City = Guard.Against.NullOrWhiteSpace(city);
    }

    public virtual object Get(int fieldPos)
    {
        switch (fieldPos)
        {
            case 0: return City;
            case 1: return Lien1;
            case 2: return Zipcode;
            default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
        }

        ;
    }

    public virtual void Put(int fieldPos, object fieldValue)
    {
        switch (fieldPos)
        {
            case 0:
                City = (string)fieldValue;
                break;
            case 1:
                Lien1 = (string)fieldValue;
                break;
            case 2:
                Zipcode = (string)fieldValue;
                break;
            default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
        }

        ;
    }

    public Schema Schema => Schema.Parse(
        "{\"type\":\"record\",\"name\":\"Address\",\"namespace\":\"Transformer.Models\",\"fields\":[{\"na" +
        "me\":\"City\",\"type\":\"string\"},{\"name\":\"Lien1\",\"type\":\"string\"},{\"name\":\"Zipcode\",\"" +
        "type\":\"string\"}]}");
}
