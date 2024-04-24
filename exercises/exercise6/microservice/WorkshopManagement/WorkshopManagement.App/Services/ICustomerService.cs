using System.Collections.Immutable;
using System.Net.Http.Json;
using WorkshopManagement.App.Models;

namespace WorkshopManagement.App.Services;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetCustomers();
}

public class CustomerService(HttpClient httpClient)
    : ICustomerService
{
    public async Task<IEnumerable<CustomerDto>> GetCustomers()
    {
        var customers = await httpClient.GetFromJsonAsync<List<CustomerDto>>("api/customers");
        return customers?.AsReadOnly() ?? Array.Empty<CustomerDto>().AsReadOnly();
    }
}
