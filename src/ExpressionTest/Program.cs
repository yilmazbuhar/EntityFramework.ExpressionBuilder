// See https://aka.ms/new-console-template for more information
using ExpressionTest;
using System.Linq.Expressions;
using System.Reflection;

//var lambda = GetExpression<Foo>("Bar", "s");
////Foo foo = new Foo { Bar = "aabca" };
////bool test = lambda.Compile()(foo);

//List<Foo> foolist = new List<Foo> {
//    new (){ Bar = "John" },
//    new (){ Bar = "Soap" },
//    new (){ Bar = "Simon" },
//    new (){ Bar = "Kyle" },
//    new (){ Bar = "Roach" }
//};

//var filtered = foolist.Where(lambda.Compile());

//static Expression<Func<T, bool>> GetExpression<T>(string propertyName, string propertyValue)
//{
//    var parameterExp = Expression.Parameter(typeof(T), "type");
//    var propertyExp = Expression.Property(parameterExp, propertyName);
//    MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
//    var someValue = Expression.Constant(propertyValue, typeof(string));
//    var someValue2 = Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison));
//    var containsMethodExp = Expression.Call(propertyExp, method, new Expression[] { someValue, someValue2 });

//    return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
//}

using CustomerDbContext customerdb = new CustomerDbContext();

var teamid = Guid.NewGuid();
await customerdb.Team.AddAsync(new() { Title = "GhostTeam", Id = teamid });
await customerdb.SaveChangesAsync();

await customerdb.Customer.AddAsync(new() { Name = "John", Surname = "Price", TeamId = teamid });
await customerdb.Customer.AddAsync(new() { Name = "Soap", Surname = "Mactavish", TeamId = teamid });
await customerdb.Customer.AddAsync(new() { Name = "Simon", Surname = "Riley", TeamId = teamid });
await customerdb.Customer.AddAsync(new() { Name = "Kyle", Surname = "Garrick", TeamId = teamid });
await customerdb.Customer.AddAsync(new() { Name = "Roach", Surname = "Sanderson", TeamId = teamid });

var result = await customerdb.SaveChangesAsync();

var jsonFilter = File.ReadAllText("filterdata.json");

PredicateBuilder predicateBuilder = new PredicateBuilder();
Expression<Func<Person, bool>> predicate = predicateBuilder.GenerateFilterPredicate<Person>(jsonFilter);

var filteredcustomers = customerdb.Customer.Where(predicate).ToList();
Console.WriteLine($"Filter for {jsonFilter}");
Console.WriteLine($"------------------------------------");
foreach (var item in filteredcustomers)
{
    Console.WriteLine(item.ToString());
}


Console.Read();

class Foo
{
    public string Bar { get; set; }
}
