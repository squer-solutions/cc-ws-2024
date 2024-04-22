using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SQUER.Workshop.LegacyApp.Models;

namespace SQUER.Workshop.LegacyApp.Controllers;

public class CustomersController : Controller
{
    private readonly IDbConnection _dbConnection;

    public CustomersController(IConfiguration configuration)
    {
        _dbConnection = new NpgsqlConnection(configuration.GetConnectionString("Postgres"));
    }

    [HttpGet]
    public IActionResult Index()
    {
        var customers = _dbConnection.Query<Customer>("select * from public.customers");

        return View(customers.ToList());
    }

    [HttpGet("new")]
    [HttpGet("edit/{customerId}")]
    public IActionResult GetCustomerInfo(string customerId)
    {
        Customer customer;

        if (!string.IsNullOrWhiteSpace(customerId))
        {
            customer = _dbConnection.QuerySingle<Customer>("select * from public.customers where customer_id = '" +
                                                           customerId + "'");

            return View(customer);
        }
        else
        {
            return View();
        }
    }


    [HttpPost("{customerId?}")]
    public IActionResult UpdateCustomerInfo(Guid? customerId, Customer customer)
    {
        if (customerId.HasValue)
            _dbConnection.Execute(
                $"UPDATE public.customers SET ssn = '{customer.Ssn}'," +
                $" email = '{customer.Email}', user_name = '{customer.User_name}', full_name = '{customer.Full_name}', " +
                $"delivery_address = '{customer.Delivery_address}', delivery_zipcode = '{customer.Delivery_zipcode}'," +
                $" delivery_city = '{customer.Delivery_city}', " +
                $"billing_address = '{customer.Billing_address}', billing_zipcode = '{customer.Billing_zipcode}'," +
                $" billing_city = '{customer.Billing_city}' WHERE customer_id = '{customer.Customer_id}'");
        else
        {
            _dbConnection.Execute(
                $"INSERT INTO public.customers (customer_id, ssn, email, user_name, full_name," +
                $" delivery_address, delivery_zipcode, delivery_city, billing_address, billing_zipcode, billing_city)" +
                $" VALUES ('{Guid.NewGuid()}', '{customer.Ssn}', '{customer.Email}', '{customer.User_name}', '{customer.Full_name}', '{customer.Delivery_address}', " +
                $"'{customer.Delivery_zipcode}', '{customer.Delivery_city}', '{customer.Billing_address}'," +
                $" '{customer.Billing_zipcode}', '{customer.Billing_city}')");
        }

        return RedirectToAction(nameof(Index));
    }
}
