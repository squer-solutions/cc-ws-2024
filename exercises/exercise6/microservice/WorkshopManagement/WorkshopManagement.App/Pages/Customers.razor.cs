using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using WorkshopManagement.App.Models;
using WorkshopManagement.App.Services;

namespace WorkshopManagement.App.Pages;

public partial class Customers : ComponentBase
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public ICustomerService CustomerService { get; set; }

    private HubConnection _hubConnection = default!;

    private List<CustomerDto> CustomersDto { get; set; } = new();


    protected override async Task OnInitializedAsync()
    {
        var customers = await CustomerService.GetCustomers();
        CustomersDto = [..customers];

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/customers/hub"))
            .Build();

        _hubConnection.On<CustomerDto>("CustomerTransformed", dto =>
        {
            Console.WriteLine(dto);
            var customer = CustomersDto.FirstOrDefault(x => x.Email == dto.Email);
            if(customer is not null)
                CustomersDto.Remove(customer);
            
            CustomersDto.Add(dto);
            InvokeAsync(StateHasChanged);
        });

        await _hubConnection.StartAsync();
    }

    public async ValueTask DisposeAsync() => await _hubConnection.StopAsync();
}
