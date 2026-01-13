using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.Interface;
using MasterScheduler.Shared.Logging;
using MasterScheduler.Shared.Service;
using MasterScheduler.Worker;
using Quartz.Spi;
using Serilog;

//var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddHostedService<Worker>();
//DatabaseHelper.Initialize();
//var host = builder.Build();
//host.Run();

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Sink(new MySqliteSink()) // This is the 'Sink'
    .CreateLogger();
builder.Logging.AddSerilog();
// IMPORTANT: Tell .NET this is a Windows Service

//builder.Services.AddWindowsService(options =>
//{
//    options.ServiceName = "MasterScheduler";
//});

builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<IJobRepository, JobRepository>();
builder.Services.AddSingleton<IScheduledJobStore, ScheduledJobStore>();



// Register worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// Move init AFTER host is built
using (var scope = host.Services.CreateScope())
{
    DatabaseHelper.Initialize();
}

host.Run();