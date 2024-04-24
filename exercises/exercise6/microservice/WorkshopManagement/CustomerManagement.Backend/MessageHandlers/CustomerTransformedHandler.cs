using CustomerManagement.Backend.Hubs;
using CustomerManagement.Backend.Repository;
using KafkaFlow;
using Microsoft.AspNetCore.SignalR;
using Transformer.Models;

namespace CustomerManagement.Backend.MessageHandlers;

public class CustomerTransformedHandler : IMessageHandler<Customer>
{
    private readonly ILogger<CustomerTransformedHandler> _logger;
    private readonly ISaveCustomer _repository;
    private readonly IHubContext<CustomerHub, ICustomerClient> _customerHubContext;

    public CustomerTransformedHandler(
        ILogger<CustomerTransformedHandler> logger,
        ISaveCustomer repository , 
        IHubContext<CustomerHub, ICustomerClient> customerHubContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repository = repository;
        _customerHubContext = customerHubContext;
    }

    public async Task Handle(IMessageContext context, Customer customer)
    {
        _logger.LogInformation("Customer transformed: {Customer}", customer);
        
        var customerDto = new CustomerDto
        {
            Username = customer.Username, Email = customer.Email,
            FirstName = customer.FirstName, LastName = customer.LastName
        };

        await _repository.RegisterCustomer(CustomerDbModel.Of(customer), CancellationToken.None);
        await _customerHubContext.Clients.All.CustomerTransformed(customerDto);
    }
}
