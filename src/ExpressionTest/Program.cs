using LambdaBuilder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var serviceProvider = new ServiceCollection()
    //.AddSingleton<IPredicateLambdaBuilder, PredicateLambdaBuilder>()
    //.AddDbContext<CustomerDbContext>(optionsAction => { 
    //    optionsAction.use
    //})
    .AddLambdaBuilder(configuration)
    .AddOptions()
    .Configure<LambdaBuilderSettings>(configuration.GetSection("LambdaBuilderSettings"))
    .BuildServiceProvider();



using CustomerDbContext customerdb = new CustomerDbContext();
var _predicateLambdaBuilder = serviceProvider.GetRequiredService<PredicateLambdaBuilder>();

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

// Generate condition -----------------------------------------------------
Expression<Func<Person, bool>> predicate = await _predicateLambdaBuilder.CreateLambda<Person>(jsonFilter);

// Send to database -------------------------------------------------------
var filteredcustomers = customerdb.Customer.Where(predicate).OrderBy(x => x.Id).ThenBy(x => x.TeamId).ToList();

// Print result -----------------------------------------------------------
Console.WriteLine($"Filter for {jsonFilter}");
Console.WriteLine($"------------------------------------");
foreach (var item in filteredcustomers)
{
    Console.WriteLine(item.ToString());
}

Console.Read();

//bool ComparePerson(Person person)
//{
//    return person.Id == Guid.Empty;
//}