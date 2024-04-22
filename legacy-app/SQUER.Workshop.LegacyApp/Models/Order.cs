namespace SQUER.Workshop.LegacyApp.Models;

public class Order
{
    public Guid Customre_id { get; set; }
    public Guid Order_id { get; set; }
    public decimal Unit_price { get; set; }
    public int Quantity { get; set; }
}
