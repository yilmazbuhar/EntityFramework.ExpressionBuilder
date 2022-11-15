using Microsoft.Extensions.Options;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionTest
{
    public interface ILambdaBuilder
    {
        Expression<Func<T, bool>> CreateLambda<T>(string query, bool roundDecimal = false);
    }

    public class LambdaBuilder : ILambdaBuilder
    {
        private readonly IQueryFormatter _formatter;
        private readonly LambdaBuilderSettings _settings;

        public LambdaBuilder(IEnumerable<IQueryFormatter> formatters, IOptions<LambdaBuilderSettings> settings)
        {
            _formatter = formatters.First(f => f.Name == settings.Value.Formatter);
        }

        /// <summary>
        /// Set constant for given <see cref="PropertyInfo"/>
        /// </summary>
        /// <param name="prop"><see cref="PropertyInfo"/> of member</param>
        /// <param name="value">String value for constant. This value will convert to prop type</param>
        /// <returns></returns>
        public Expression ToExpressionConstant(PropertyInfo prop, string value)
        {
            if (string.IsNullOrEmpty(value))
                return Expression.Constant(null);
            else
            {
                var fullName = prop.PropertyType.FullName;
                if (prop.PropertyType.FullName.Contains("System.DateTime") && prop.PropertyType.FullName.Contains("System.Nullable"))
                    fullName = "System.DateTime";
                else if (prop.PropertyType.FullName.Contains("System.Guid") && prop.PropertyType.FullName.Contains("System.Nullable"))
                    fullName = "System.Guid";
                else if (
                    (!prop.PropertyType.IsGenericType && prop.PropertyType.IsEnum)
                    || (prop.PropertyType.IsGenericType && Nullable.GetUnderlyingType(prop.PropertyType).BaseType == typeof(Enum))
                    )
                    fullName = "System.Enum";


                object val;
                switch (fullName)
                {
                    case "System.Guid":
                        val = Guid.Parse(value);
                        break;
                    case "System.DateTime":
                        if (DateTime.TryParseExact(value, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                            val = result;
                        else
                            val = DateTime.ParseExact(value, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                        break;
                    case "System.Enum":
                        val = Int32.Parse(value);
                        break;
                    default:
                        Type t = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        val = Convert.ChangeType(value, Type.GetType(t.FullName));
                        break;
                }
                return Expression.Constant(val);
            }
        }

        public Expression<Func<T, bool>> CreateLambda<T>(string query, bool roundDecimal = false)
        {
            if (string.IsNullOrEmpty(query))
                return null;

            List<QueryItem> filterList = _formatter.Compile(query);
            Expression<Func<T, bool>> predicate = null;

            var parameterExpression = Expression.Parameter(typeof(T), nameof(T));
            foreach (var filter in filterList)
            {
                Expression<Func<T, bool>>? returnExp = null;

                PropertyInfo propertyInfo = (typeof(T).GetProperty(filter.Member));
                var property = Expression.Property(parameterExpression, propertyInfo);
                var constant = ToExpressionConstant(propertyInfo, filter.Value);

                returnExp = filter.Operator switch
                {
                    var method when method ==
                    "contains" => Contains<T>(parameterExpression, property, constant),
                    "startswith" => StartsWith<T>(parameterExpression, property, constant),
                    "notcontains" => NotContains<T>(parameterExpression, property, constant),
                    "notstartwith" => NotStartsWith<T>(parameterExpression, property, constant),
                    "equal" => Expression.Lambda<Func<T, bool>>(Expression.Equal(property, constant), parameterExpression),
                    "notequal" => Expression.Lambda<Func<T, bool>>(Expression.NotEqual(property, constant), parameterExpression),
                    "" => null
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

        /// <summary>
        /// Contains with OrdinalIgnoreCase
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramExp"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private Expression<Func<T, bool>> Contains<T>(ParameterExpression paramExp, Expression left, Expression right)
        {
            var stringComparisonParameter =
                Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison));

            Expression<Func<T, bool>> returnExp;

            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
            MethodCallExpression expMethod = Expression.Call(left, method, new Expression[] { right, stringComparisonParameter });

            return Expression.Lambda<Func<T, bool>>(expMethod, paramExp);
        }

        /// <summary>
        /// Start with. This method only work with <see cref="System.String"/> type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramExp"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private Expression<Func<T, bool>> StartsWith<T>(ParameterExpression paramExp, Expression left, Expression right)
        {
            Expression<Func<T, bool>> returnExp;

            MethodInfo method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            MethodCallExpression expMethod = Expression.Call(left, method, right);

            return Expression.Lambda<Func<T, bool>>(expMethod, paramExp);
        }

        /// <summary>
        /// Not start with
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramExp"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private Expression<Func<T, bool>> NotStartsWith<T>(ParameterExpression paramExp, Expression left, Expression right)
        {
            Expression<Func<T, bool>> returnExp;

            MethodInfo method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            MethodCallExpression expMethod = Expression.Call(left, method, right);
            UnaryExpression negateExpression = Expression.Not(expMethod);

            return Expression.Lambda<Func<T, bool>>(negateExpression, paramExp);
        }

        /// <summary>
        /// Not contains with ignore case
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramExp"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private Expression<Func<T, bool>> NotContains<T>(ParameterExpression paramExp, Expression left, Expression right)
        {
            var stringComparisonParameter =
                Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison));

            Expression<Func<T, bool>> returnExp;

            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
            MethodCallExpression expMethod = Expression.Call(left, method, new Expression[] { right, stringComparisonParameter });
            UnaryExpression negateExpression = Expression.Not(expMethod);

            return Expression.Lambda<Func<T, bool>>(negateExpression, paramExp);
        }

        /// <summary>
        /// Greater than or greater than or equal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramExp"></param>
        /// <param name="left">Parameter</param>
        /// <param name="right">Value</param>
        /// <param name="gtoe">Pass true for greater than or equal</param>
        /// <returns></returns>
        private Expression<Func<T, bool>> GreaterThan<T>(ParameterExpression paramExp, Expression left, Expression right, bool gtoe)
        {
            if (gtoe)
                return Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(left, right));

            return Expression.Lambda<Func<T, bool>>(Expression.GreaterThan(left, right));
        }

        /// <summary>
        /// Less than or less than or equal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramExp"></param>
        /// <param name="left">Parameter</param>
        /// <param name="right">Value</param>
        /// <param name="gtoe">Pass true for greater than or equal</param>
        /// <returns></returns>
        private Expression<Func<T, bool>> LessThan<T>(ParameterExpression paramExp, Expression left, Expression right, bool gtoe)
        {
            if (gtoe)
                return Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(left, right));

            return Expression.Lambda<Func<T, bool>>(Expression.LessThan(left, right));
        }
    }
}

//public static class ExtensionMethods
//{
//    public static IQueryable<T> Where<T>(this IQueryable<T> query, string selector, string comparer, string value)
//    {
//        var target = Expression.Parameter(typeof(T));

//        return query.Provider.CreateQuery<T>(CreateWhereClause(target, query.Expression, selector, comparer, value));
//    }

//    static Expression CreateWhereClause(ParameterExpression target, Expression expression, string selector, string comparer, string value)
//    {
//        var predicate = Expression.Lambda(CreateComparison(target, selector, comparer, value), target);

//        return Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { target.Type },
//            expression, Expression.Quote(predicate));
//    }

//    static Expression CreateComparison(ParameterExpression target, string selector, string comparer, string value)
//    {
//        var memberAccess = CreateMemberAccess(target, selector);
//        var actualValue = Expression.Constant(value, typeof(string));

//        return Expression.Call(memberAccess, comparer, null, actualValue);
//    }

//    static Expression CreateMemberAccess(Expression target, string selector)
//    {
//        return selector.Split('.').Aggregate(target, (t, n) => Expression.PropertyOrField(t, n));
//    }
//}