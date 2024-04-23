using DefaultNamespace;
using Transformer;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<TransformerService>();

builder.Services.Configure<TransformerConfiguration>(
    builder.Configuration.GetRequiredSection(TransformerConfiguration.Section));

var host = builder.Build();
host.Run();
