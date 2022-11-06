// See https://aka.ms/new-console-template for more information
using ExpressionTest;
using System.Linq.Expressions;

var jsonFilter = File.ReadAllText("filterdata.json");

PredicateBuilder predicateBuilder = new PredicateBuilder();
Expression<Func<Customer, bool>> predicate = predicateBuilder.GenerateFilterPredicate<Customer>(jsonFilter);
_ = predicate.Body;
predicate = PredicateBuilderExtensions.AndAlso(predicate, null);


Console.Read();
