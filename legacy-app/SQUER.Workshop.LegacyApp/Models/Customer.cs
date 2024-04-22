namespace SQUER.Workshop.LegacyApp.Models;

public class Customer
{
    public Guid Customer_id { get; set; }
    public string Ssn { get; set; }
    public string Email { get; set; }
    public string User_name { get; set; }
    public string Full_name { get; set; }
    public string Delivery_address { get; set; }
    public string Delivery_zipcode { get; set; }
    public string Delivery_city { get; set; }
    public string Billing_address { get; set; }
    public string Billing_zipcode { get; set; }
    public string Billing_city { get; set; }
    
}
