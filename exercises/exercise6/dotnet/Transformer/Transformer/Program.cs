using Transformer;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<TransformerService>();

var host = builder.Build();
host.Run();
