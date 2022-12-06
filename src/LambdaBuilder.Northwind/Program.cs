using LambdaBuilder;
using LambdaBuilder.Northwind;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;

var hostBuilder = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration(cb =>
    {
        cb.AddJsonFile("appsettings.json", false);
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddDbContext<NorthwindContext>(optionBuilder =>
        {
            optionBuilder.UseSqlServer(ctx.Configuration.GetConnectionString("default"));
        });
    })
    .ConfigureLogging(log =>
    {
        log.ClearProviders();
    })
    .Build();

var dbContext = hostBuilder.Services.GetService<NorthwindContext>();

string[] cultures = new[] { "en-US", "en-GB", "tr-TR", "zh-Hans", "zh-Hant" };

foreach (var item in cultures)
{
    CultureInfo us = new CultureInfo(item);
    var datetimeformatString = us.DateTimeFormat.ShortDatePattern;
    string shortTimeFormatString = us.DateTimeFormat.ShortTimePattern;

    Console.WriteLine($"{datetimeformatString} {shortTimeFormatString}");
    Console.WriteLine();
}


OrderFilter("filters/orderfilter_shipname.json");

void OrderFilter(string filterfile)
{
    // Generate mock request --------------------------------------------------
    var jsonFilter = File.ReadAllText(filterfile);

    // Send to database -------------------------------------------------------
    var data = dbContext.Orders.ApplyFilterAndSort(jsonFilter).Result;

    foreach (var item in data)
    {
        Console.WriteLine($"{item.ShippedDate}");
    }
}

