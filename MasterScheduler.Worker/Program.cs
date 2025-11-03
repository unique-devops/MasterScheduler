using MasterScheduler.Shared.Data;
using MasterScheduler.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
DatabaseHelper.Initialize();
var host = builder.Build();
host.Run();
