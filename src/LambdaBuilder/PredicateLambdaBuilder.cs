using LambdaBuilder.Infra;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace LambdaBuilder
{
    public class PredicateLambdaBuilder
    {
        static Expression BuildPredicate<T>(string member)
        {
            var p = Expression.Parameter(typeof(T));
            Expression body = p;
            foreach (var subMember in member.Split('.'))
            {
                body = Expression.PropertyOrField(body, subMember);
            }
            //return Expression.Lambda<Func<T, bool>>(Expression.Equal(body, Expression.Constant(value, body.Type)), p);
            return body;
        }

        public async Task<Expression<Func<T, bool>>> GenerateConditionLambda<T>(List<QueryItem> conditions)
        {
            Expression<Func<T, bool>> predicate = null;

            var parameter = Expression.Parameter(typeof(T), nameof(T));
            foreach (var condition in conditions)
            {
                Expression<Func<T, bool>>? returnExp = null;

                PropertyInfo propertyInfo = (typeof(T).GetProperty(condition.Member));
                //var property = Expression.Property(parameter, propertyInfo);
                var property = BuildPredicate<T>(condition.Member);
                var constant = ToExpressionConstant(propertyInfo, condition.Value);

                returnExp = condition.Operator switch
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

                if (string.IsNullOrEmpty(condition.LogicalOperator) || condition.LogicalOperator.ToLower() == "and")
                {
                    predicate = predicate == null ? returnExp : returnExp.And(predicate);
                }
                else if (condition.LogicalOperator.ToLower() == "or")
                {
                    predicate = predicate == null ? returnExp : predicate = returnExp.Or(predicate);
                }
            }

            return predicate;
        }

        public async Task<Expression<Func<TEntity, object>>> GenerateSortLambda<TEntity>(SortItem sortitem)
        {
            //if (Formatter == null)
            //    Formatter = new JsonQueryFormatter();

            //Condition filterList = await Formatter.Compile(query);

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
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(typeof(TEntity), sortitem.Field);
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