using MasterScheduler.Shared.Data;
using MasterScheduler.Worker;

//var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddHostedService<Worker>();
//DatabaseHelper.Initialize();
//var host = builder.Build();
//host.Run();

var builder = Host.CreateApplicationBuilder(args);

// IMPORTANT: Tell .NET this is a Windows Service

//builder.Services.AddWindowsService(options =>
//{
//    options.ServiceName = "MasterScheduler";
//});



// Register worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// Move init AFTER host is built
using (var scope = host.Services.CreateScope())
{
    DatabaseHelper.Initialize();
}

host.Run();