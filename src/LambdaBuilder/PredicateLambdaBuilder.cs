using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace LambdaBuilder
{
    public enum SortDirection : short
    {
        ASC = 1,
        DESC = 2
    }

    public static class SearchExtension
    {
        /// <summary>
        /// Default formatter <see cref="JsonQueryFormatter"/>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="query"></param>
        /// <param name="queryFormatter"></param>
        /// <returns></returns>
        public static async Task<IQueryable<TEntity>> ApplyFilterAndSort<TEntity>(this IQueryable<TEntity> source, string query, IQueryFormatter queryFormatter) where TEntity : class
        {
            if (queryFormatter == null)
                queryFormatter = new JsonQueryFormatter();

            var predicateBuilder = new PredicateLambdaBuilder();
            var lambda = await predicateBuilder.GenerateConditionLambda<TEntity>(query, queryFormatter);

            var sort = await predicateBuilder.GenerateSortLambda<TEntity>("sdvsa");

            source = source.Where(lambda);
            source = source.OrderByDescending(sort);

            return source;
        }
    }

    public static class SortExtensions
    {
        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string field, SortDirection sortDirection) where TEntity : class
        {
            var orderExpression = source.Expression;

            var type = typeof(TEntity);
            var parameter = Expression.Parameter(type, "orderparameter");

            PropertyInfo property = SearchProperty(typeof(TEntity), field);
            MemberExpression propertyAccess = Expression.MakeMemberAccess(parameter, property);

            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            var command = sortDirection == SortDirection.ASC ? "OrderBy" : "OrderByDescending";
            orderExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType }, orderExpression, Expression.Quote(orderByExpression));

            return source.Provider.CreateQuery<TEntity>(orderExpression);
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string sortProperty, ListSortDirection sortOrder)
        {
            var type = typeof(T);
            var parameter = Expression.Parameter(type, "p");

            var property = type.GetProperty(sortProperty);

            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            var typeArguments = new Type[] { type, property.PropertyType };
            var methodName = sortOrder == ListSortDirection.Ascending ? "OrderBy" : "OrderByDescending";
            var resultExp = Expression.Call(typeof(Queryable), methodName, typeArguments, source.Expression, Expression.Quote(orderByExp));

            return source.Provider.CreateQuery<T>(resultExp);
        }

        private static PropertyInfo SearchProperty(Type type, string propertyName)
        {
            foreach (var item in type.GetProperties())
                if (item.Name.ToLower() == propertyName.ToLower())
                    return item;
            return null;
        }
    }


    public class PredicateLambdaBuilder
    {
        

        private static PropertyInfo GetProperty(Type type, string propertyName)
        {
            string typeName = string.Empty;
            if (propertyName.Contains("."))
            {
                //name was specified with typename - so pull out the typename
                typeName = propertyName.Substring(0, propertyName.IndexOf("."));
                propertyName = propertyName.Substring(propertyName.IndexOf(".") + 1);
            }

            PropertyInfo prop = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            if (prop == null)
            {
                var baseTypesAndInterfaces = new List<Type>();
                if (type.BaseType != null) baseTypesAndInterfaces.Add(type.BaseType);
                baseTypesAndInterfaces.AddRange(type.GetInterfaces());
                foreach (Type t in baseTypesAndInterfaces)
                {
                    prop = GetProperty(t, propertyName);
                    if (prop != null)
                    {
                        if (!string.IsNullOrEmpty(typeName) && t.Name != typeName)
                            continue; //keep looking as the typename was not found
                        break;
                    }
                }
            }
            return prop;
        }

        public async Task<Expression<Func<T, bool>>> GenerateConditionLambda<T>(string query, IQueryFormatter _formatter)
        {
            if (string.IsNullOrEmpty(query))
                return null;

            if (_formatter == null)
                _formatter = new JsonQueryFormatter();

            Condition filterList = await _formatter.Compile(query);
            Expression<Func<T, bool>> predicate = null;

            var parameter = Expression.Parameter(typeof(T), nameof(T));
            foreach (var filter in filterList.Where)
            {
                Expression<Func<T, bool>>? returnExp = null;

                PropertyInfo propertyInfo = (typeof(T).GetProperty(filter.Member));
                var property = Expression.Property(parameter, propertyInfo);
                var constant = ToExpressionConstant(propertyInfo, filter.Value);

                returnExp = filter.Operator switch
                {
                    var method when method ==
                        "contains" => Contains<T>(parameter, property, constant),
                        "startswith" => StartsWith<T>(parameter, property, constant),
                        "notcontains" => NotContains<T>(parameter, property, constant),
                        "notstartwith" => NotStartsWith<T>(parameter, property, constant),
                        "equal" => Expression.Lambda<Func<T, bool>>(Expression.Equal(property, constant), parameter),
                        "notequal" => Expression.Lambda<Func<T, bool>>(Expression.NotEqual(property, constant), parameter),
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

        public async Task<Expression<Func<TEntity, object>>> GenerateSortLambda<TEntity>(string fieldname)
        {
            //IQueryable<TEntity> entities = null;
            #region Working Model 1
            //var type=typeof(TEntity);
            //var param = Expression.Parameter(type);
            //var body = Expression.Property(param, fieldname);

            //return Expression.Lambda<Func<TEntity, object>>(body, param);
            #endregion

            #region Working Model 2
            var param = Expression.Parameter(typeof(TEntity), string.Empty);

            //normally one would use Expression.Property(param, sortField), but that doesnt work
            //when working with interfaces where the sortField is defined on a base interface.
            //so instead we search for the Property through our own GetProperty method and use it to build the 
            //Expression property
            PropertyInfo propertyInfo = GetProperty(typeof(TEntity), fieldname);
            var property = Expression.Property(param, propertyInfo);

            return Expression.Lambda<Func<TEntity, object>>(Expression.Convert(property, typeof(object)), param);
            #endregion


            #region Working Model 3
            //var type = typeof(TEntity);
            //var parameter = Expression.Parameter(type, "p");
            //var property = type.GetProperty(fieldname);
            //var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            //return Expression.Lambda<Func<TEntity, object>>(propertyAccess, parameter); 
            #endregion
            //var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExpression));
            //return source.Provider.CreateQuery<TEntity>(resultExpression);

            //return entities.Provider.CreateQuery<TEntity>(resultExpression);
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

        /// <summary>
        /// Contains with OrdinalIgnoreCase
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="property"></param>
        /// <param name="constant"></param>
        /// <returns></returns>
        private Expression<Func<T, bool>> Contains<T>(ParameterExpression parameter, Expression property, Expression constant)
        {
            var stringComparisonParameter = Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison));

            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
            MethodCallExpression expMethod = Expression.Call(property, method, new Expression[] { constant, stringComparisonParameter });

            return Expression.Lambda<Func<T, bool>>(expMethod, parameter);
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
        /// <param name="ltoe">Pass true for lass than or equal</param>
        /// <returns></returns>
        private Expression<Func<T, bool>> LessThan<T>(ParameterExpression paramExp, Expression left, Expression right, bool ltoe)
        {
            if (ltoe)
                return Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(left, right));

            return Expression.Lambda<Func<T, bool>>(Expression.LessThan(left, right));
        }
    }
}