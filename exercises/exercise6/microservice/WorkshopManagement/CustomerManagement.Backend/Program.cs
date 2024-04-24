using CustomerManagement.Backend.Configurations;
using CustomerManagement.Backend.HostedServices;
using CustomerManagement.Backend.Hubs;
using CustomerManagement.Backend.MessageHandlers;
using CustomerManagement.Backend.Repository;
using KafkaFlow;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using AutoOffsetReset = KafkaFlow.AutoOffsetReset;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetRequiredSection(DatabaseSettings.Section));


builder.Services.AddHostedService<MongoInitializerService>();
builder.Services.AddSingleton<MongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptionsMonitor<DatabaseSettings>>().CurrentValue;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoCollection<CustomerDbModel>>(sp =>
{
    var settings = sp.GetRequiredService<IOptionsMonitor<DatabaseSettings>>().CurrentValue;
    var mongoClient = sp.GetRequiredService<MongoClient>();

    var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName);

    return mongoDatabase.GetCollection<CustomerDbModel>(settings.CustomersCollectionName);
});

builder.Services.AddSingleton<IFetchCustomer, CustomerRepository>();
builder.Services.AddSingleton<ISaveCustomer, CustomerRepository>();
builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
});
builder.Services.AddSignalR();

KafkaSettings kafkaSettings = new();
builder.Configuration.GetRequiredSection(KafkaSettings.Section).Bind(kafkaSettings);

Console.WriteLine("--------------------------------");
Console.WriteLine(kafkaSettings.Brokers.First());
Console.WriteLine(kafkaSettings.GroupId);
Console.WriteLine(kafkaSettings.SchemaRegistryUrl);
Console.WriteLine(kafkaSettings.TransformerTopic);
Console.WriteLine("--------------------------------");

builder.Services.AddKafkaFlowHostedService(kafka => kafka
    .UseMicrosoftLog()
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(kafkaSettings.Brokers)
        .WithSchemaRegistry(config => config.Url = kafkaSettings.SchemaRegistryUrl)
        .AddConsumer(consumer => consumer
            .Topic(kafkaSettings.TransformerTopic)
            .WithGroupId(kafkaSettings.GroupId)
            .WithBufferSize(100)
            .WithWorkersCount(1)
            .WithAutoOffsetReset(AutoOffsetReset.Earliest)
            .AddMiddlewares(middlewares => middlewares
                .AddSchemaRegistryAvroDeserializer()
                .AddTypedHandlers(handler => handler
                    .AddHandler<CustomerTransformedHandler>()
                    .WhenNoHandlerFound(_ => Console.WriteLine("No Handler found")))))
    )
);

var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// This is necessary to serve the blazor application files 
app.UseBlazorFrameworkFiles();
// this is definitely necessary because the
// blazor application has some static files, that required to be served
app.UseStaticFiles();

app.MapHub<CustomerHub>("/customers/hub");

app.MapGet("api/customers", async (IFetchCustomer repository, CancellationToken cancellationToken) =>
    {
        var customers = await repository.GetAll(cancellationToken);

        return customers.Select(c => new CustomerDto
        {
            Username = c.Username, FirstName = c.FirstName, LastName = c.LastName, Email = c.Email
        });
    })
    .WithName("GetCustomers")
    .WithOpenApi();

// if the requested route does not exists, then route it to the index.html file
app.MapFallbackToFile("index.html");
app.Run();
