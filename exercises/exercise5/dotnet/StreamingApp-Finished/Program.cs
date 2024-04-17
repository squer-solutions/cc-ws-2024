using StreamingApp;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<StreamingAppSettings>(
    builder.Configuration.GetRequiredSection(StreamingAppSettings.ConfigSection));

builder.Services.AddHostedService<StreamingAppWorker>();

var host = builder.Build();
host.Run();
