using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SQUER.Workshop.LegacyApp.Models;

namespace SQUER.Workshop.LegacyApp.Controllers;

[Route("orders")]
public class OrdersController : Controller
{
    private readonly IDbConnection _dbConnection;

    public OrdersController(IConfiguration configuration)
    {
        _dbConnection = new NpgsqlConnection(configuration.GetConnectionString("Postgres"));
    }

    [HttpGet("{customerId}")]
    public IActionResult GetBuyTickets(string customerId)
    {
        ViewBag.CustomerId = customerId;
        return View(new Order());
    }


    [HttpPost("{customerId}")]
    public IActionResult BuyTickets(string customerId, Order order)
    {
        _dbConnection.Execute("INSERT INTO public.orders(order_id, customer_id, quantity, unit_price)" +
                              $" VALUES ( '{Guid.NewGuid()}', '{customerId}', {order.Quantity}, {order.Unit_price} )");

        return Redirect($"/Customers/Index");
    }
}
