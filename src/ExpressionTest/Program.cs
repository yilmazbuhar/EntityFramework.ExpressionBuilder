// See https://aka.ms/new-console-template for more information
using ExpressionTest;
using System.Linq.Expressions;

using CustomerDbContext customerdb = new CustomerDbContext();

var teamid = Guid.NewGuid();
await customerdb.Team.AddAsync(new() { Title = "GhostTeam", Id = teamid });
await customerdb.SaveChangesAsync();

await customerdb.Customer.AddAsync(new() { Name = "John", Surname = "Price", TeamId=teamid  });
await customerdb.Customer.AddAsync(new() { Name = "Soap", Surname = "Mactavish", TeamId = teamid });
await customerdb.Customer.AddAsync(new() { Name = "Simon", Surname = "Riley", TeamId = teamid });
await customerdb.Customer.AddAsync(new() { Name = "Kyle", Surname = "Garrick", TeamId = teamid });
await customerdb.Customer.AddAsync(new() { Name = "Roach", Surname = "Sanderson", TeamId = teamid });

var result = await customerdb.SaveChangesAsync();

var jsonFilter = File.ReadAllText("filterdata.json");

PredicateBuilder predicateBuilder = new PredicateBuilder();
Expression<Func<Person, bool>> predicate = predicateBuilder.GenerateFilterPredicate<Person>(jsonFilter);

var filteredcustomers = customerdb.Customer.Where(predicate).ToList();


Console.Read();
