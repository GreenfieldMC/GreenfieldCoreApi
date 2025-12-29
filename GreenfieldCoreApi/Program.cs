using GreenfieldCoreApi;
using GreenfieldCoreServices.Services.Tasks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.ConfigureConfiguration(builder.Environment);
builder.ConfigureSerilog();

try
{
    builder.Services.ConfigureCaching();
    builder.Services.ConfigureServices();
    builder.Services.ConfigureDatabases();
    builder.Services.ConfigureWebServices();
    builder.Services.ConfigureAuthentication(builder.Configuration);
    builder.Services.ConfigureCommandServices();
    builder.Services.ConfigureScheduledTasks();

    var app = builder.Build();

    await app.Services.PerformDatabaseMigrations();
    app.Services.GetRequiredService<TaskStartSignalService>().SignalStart();

    app.ConfigureWebApplication();

    app.Run();   
} 
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
