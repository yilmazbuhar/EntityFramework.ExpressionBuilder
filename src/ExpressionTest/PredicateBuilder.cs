using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionTest
{
    public class PredicateBuilder
    {

        public Expression<Func<T, bool>> GenerateFilterPredicate<T>(string query, bool roundDecimal = false)
        {
            if (string.IsNullOrEmpty(query))
                return null;

            // query item format may change for each ui library
            // so we must seperate this formatter
            List<QueryItem> filterList = JsonConvert.DeserializeObject<List<QueryItem>>(query).Where(x => x.Active).ToList();
            Expression<Func<T, bool>> predicate = null;

            foreach (var filter in filterList)
            {
                bool didBreak = false;
                ParameterExpression paramExp;
                Expression left = null, right = null;
                Expression<Func<T, bool>> returnExp = null;
                PropertyInfo property = null;

                //subtypes
                string[] fieldPath = filter.Member.Split('.');
                paramExp = Expression.Parameter(typeof(T));

                foreach (string field in fieldPath)
                {
                    Type type = (field == fieldPath[0]) ? typeof(T) : property.PropertyType;
                    PropertyInfo[] properties = type.GetProperties();
                    property = properties.FirstOrDefault(x => x.Name == field);
                    if (property == null)
                    {
                        didBreak = true;
                        break;
                    }
                    left = Expression.Property((field == fieldPath[0]) ? paramExp : left, field);
                }

                right = Expression.Convert(PredicateBuilderExtensions.ToExprConstant(property, filter.Value), property.PropertyType);

                Expression<Func<T, bool>> newresult = filter.Operator switch
                {
                    var x when x == "contains" => returnExp = Contains<T>(paramExp, left, right),
                    "startswith" => returnExp = StartsWith<T>(paramExp, left, right),
                    "notcontains" => returnExp = NotContains<T>(paramExp, left, right),
                    "notstartwith" => returnExp = NotStartsWith<T>(paramExp, left, right),
                    // todo: type control for system.datetime or nullable types
                    "equal" => returnExp = Expression.Lambda<Func<T, bool>>(Expression.Equal(left, right), paramExp),
                };

                if (string.IsNullOrEmpty(filter.LogicalOperator) || filter.LogicalOperator.ToLower() == "and")
                {
                    predicate = predicate == null ? returnExp : returnExp.And(predicate);
                }
                else if (filter.LogicalOperator.ToLower() == "or")
                {
                    predicate = predicate == null ? returnExp : predicate = returnExp.Or(predicate);
                }

            }

            return predicate;
        }

        private Expression<Func<T, bool>> NotStartsWith<T>(ParameterExpression paramExp, Expression left, Expression right)
        {
            Expression<Func<T, bool>> returnExp;
            MethodInfo method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            MethodCallExpression expMethod = Expression.Call(left, method, right);
            UnaryExpression negateExpression = Expression.Not(expMethod);
            returnExp = Expression.Lambda<Func<T, bool>>(negateExpression, paramExp);
            return returnExp;
        }

        private Expression<Func<T, bool>> NotContains<T>(ParameterExpression paramExp, Expression left, Expression right)
        {
            Expression<Func<T, bool>> returnExp;
            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            MethodCallExpression expMethod = Expression.Call(left, method, right);
            UnaryExpression negateExpression = Expression.Not(expMethod);
            returnExp = Expression.Lambda<Func<T, bool>>(negateExpression, paramExp);
            return returnExp;
        }

        private Expression<Func<T, bool>> StartsWith<T>(ParameterExpression paramExp, Expression left, Expression right)
        {
            Expression<Func<T, bool>> returnExp;
            MethodInfo method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            MethodCallExpression expMethod = Expression.Call(left, method, right);
            returnExp = Expression.Lambda<Func<T, bool>>(expMethod, paramExp);
            return returnExp;
        }

        private static Expression<Func<T, bool>> Contains<T>(ParameterExpression paramExp, Expression left, Expression right)
        {
            Expression<Func<T, bool>> returnExp;
            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            MethodCallExpression expMethod = Expression.Call(left, method, right);
            returnExp = Expression.Lambda<Func<T, bool>>(expMethod, paramExp);
            return returnExp;
        }
    }
}
