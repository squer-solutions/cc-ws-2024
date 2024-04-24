using Microsoft.AspNetCore.SignalR;

namespace CustomerManagement.Backend.Hubs;

public class CustomerHub : Hub<ICustomerClient>
{
    public async Task BroadcastCustomerTransformed(CustomerDto dto)
    {
        await Clients.All.CustomerTransformed(dto);
    }
}

public interface ICustomerClient
{
    Task CustomerTransformed(CustomerDto customer);
}

public sealed class CustomerDto
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
}
