// See https://aka.ms/new-console-template for more information
using LambdaBuilder;
using LambdaBuilder.Northwind;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var hostBuilder = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration(cb => {
        cb.AddJsonFile("appsettings.json", false);
    })
    .ConfigureServices((ctx,services) => {
        services.AddDbContext<NorthwindContext>(optionBuilder => {
            optionBuilder.UseSqlServer(ctx.Configuration.GetConnectionString("default"));
        });
    })
    .ConfigureLogging(x => {
        x.ClearProviders();
    })
    .Build();

var dbContext = hostBuilder.Services.GetService<NorthwindContext>();


// Generate mock request --------------------------------------------------
var jsonFilter = File.ReadAllText("filterdata.json");

// Send to database -------------------------------------------------------
var data = await dbContext.Orders.ApplyFilterAndSort(jsonFilter, null);

foreach (var item in data)
{
    Console.WriteLine($"{item.ShipName} -> {item.OrderDate}");
}
