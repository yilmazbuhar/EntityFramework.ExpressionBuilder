using LambdaBuilder;
using LambdaBuilder.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var serviceProvider = new ServiceCollection()
    .AddLambdaBuilder(configuration)
    .AddOptions()
    .Configure<LambdaBuilderSettings>(configuration.GetSection("LambdaBuilderSettings"))
    .BuildServiceProvider();

using CustomerDbContext customerdb = new CustomerDbContext();
//var _predicateLambdaBuilder = serviceProvider.GetRequiredService<PredicateLambdaBuilder>();
var _predicateLambdaBuilder = new PredicateLambdaBuilder();

// Save master data -------------------------------------------------------
var teamid = Guid.NewGuid();
await customerdb.Team.AddAsync(new() { Title = "GhostTeam", Id = teamid });
await customerdb.SaveChangesAsync();

await customerdb.Customer.AddAsync(new() { Name = "John", Surname = "Price", TeamId = teamid });
await customerdb.Customer.AddAsync(new() { Name = "Soap", Surname = "Mactavish", TeamId = teamid });
await customerdb.Customer.AddAsync(new() { Name = "Simon", Surname = "Riley", TeamId = teamid });
await customerdb.Customer.AddAsync(new() { Name = "Kyle", Surname = "Garrick", TeamId = teamid });
await customerdb.Customer.AddAsync(new() { Name = "Roach", Surname = "Sanderson", TeamId = teamid });

var result = await customerdb.SaveChangesAsync();

// Generate mock request --------------------------------------------------
var jsonFilter = File.ReadAllText("filterdata.json");

// Send to database -------------------------------------------------------
var filteredcustomers = await customerdb.Customer
    .ApplyFilterAndSort(jsonFilter, null);

// Print result -----------------------------------------------------------
Console.WriteLine($"Filter for {jsonFilter}");

Console.WriteLine($"\nData ------------------------------------");
foreach (var item in customerdb.Customer)
{
    Console.WriteLine(item.ToString());
}

Console.WriteLine($"\nFiltered Data ------------------------------------");
foreach (var item in filteredcustomers)
{
    Console.WriteLine(item.ToString());
}


//var team = new Team
//{
//    Title = "GhostTeam",
//    Id = teamid
//};

//var person = new Person
//{
//    Name = "John",
//    Surname = "Price",
//    TeamId = teamid,
//    Team = team
//};

//ReflectionHelper.GetProperty(typeof(Person), "Team.Title").GetValue(person);

Console.Read();