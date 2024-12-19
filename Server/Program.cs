using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using Serilog;
using Serilog.Events;
using System.Security.AccessControl;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine("Logs", "server-log-.txt"),
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Information,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

Log.Information("Starting web application");
Log.Warning("This is a test warning");
Log.Error("This is a test error");

try
{
    Log.Information("Starting web application");

    // Add Serilog to the application
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddDbContext<DataDbContext>(options => 
        options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
        Log.Information("Running in Development mode");
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
        Log.Information("Running in Production mode");
    }

    using (var scope = app.Services.CreateScope())
    {
        var path = "App_Data";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Log.Information("Created App_Data directory");
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<DataDbContext>();
        try
        {
            dbContext.Database.Migrate();
            Log.Information("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating the database");
            throw;
        }
    }

    app.UseHttpsRedirection();
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
    app.UseRouting();

    app.MapRazorPages();
    app.MapControllers();
    app.MapFallbackToFile("index.html");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}