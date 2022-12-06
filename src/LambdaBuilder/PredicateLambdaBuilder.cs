using LambdaBuilder.Infra;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace LambdaBuilder
{

    public class PredicateLambdaBuilder
    {
        static readonly ConcurrentDictionary<string, IOperator> operators =
            new ConcurrentDictionary<string, IOperator>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Generic property finder. Supports subtypes.
        /// </summary>
        /// <typeparam name="TEntity">Base type to generating lambda</typeparam>
        /// <param name="parameter">Pass existing parameter to prevent create new parameter</param>
        /// <param name="member"></param>
        /// <returns></returns>
        public Expression CreateProperty<TEntity>(ParameterExpression parameter, string member)
        {
            Expression body = parameter;
            foreach (var subMember in member.Split('.'))
            {
                body = Expression.PropertyOrField(body, subMember);
            }

            //var constant = Expression.Constant(value, body.Type);
            //var method = Expression.Equal(body, constant);
            //var ex = Expression.Lambda<Func<T, bool>>(method, p);

            return body;
        }

        private IOperator GetOperatorInstance(string key)
        {
            var types = ReflectionHelper.GetTypeOf<IOperator>();
            foreach (var item in types)
            {
                var instance = (IOperator)Activator.CreateInstance(item);
                if (instance.Name == key)
                {
                    return instance;
                }
            }

            return null;
        }

        /// <summary>
        /// Creating lambda for given query items.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public async Task<Expression<Func<TEntity, bool>>> GenerateConditionLambda<TEntity>(List<QueryItem> conditions, 
            CultureInfo cultureInfo,
            string logicalOperator = "AND")
        {
            Expression<Func<TEntity, bool>> predicate = null;
            var parameter = Expression.Parameter(typeof(TEntity));

            foreach (var condition in conditions)
            {
                Expression<Func<TEntity, bool>>? returnExp = null;

                //PropertyInfo propertyInfo = (typeof(T).GetProperty(condition.Member));
                //var property = Expression.Property(parameter, propertyInfo); //var property = Expression.PropertyOrField(parameter, condition.Member);
                //var constant = Expression.Constant(condition.Value, property.Type); //ToExpressionConstant(propertyInfo, condition.Value);

                var property = CreateProperty<TEntity>(parameter, condition.Member);
                // todo: Constant for decimal and datetime
                var constant = ToExpressionConstant(property.Type, condition.Value, cultureInfo);// Expression.Constant(condition.Value, property.Type);

                //todo: performans tunning
                var operatorInstance = operators.GetOrAdd(condition.Operator, GetOperatorInstance);

                ArgumentNullException.ThrowIfNull(operatorInstance, "operatorInstance");

                returnExp = operatorInstance.Invoke<TEntity>(parameter, property, constant);

                if (string.IsNullOrEmpty(logicalOperator) || logicalOperator.ToLower() == "and")
                {
                    predicate = predicate == null ? returnExp : returnExp.And(predicate);
                }
                else if (logicalOperator.ToLower() == "or")
                {
                    predicate = predicate == null ? returnExp : predicate = returnExp.Or(predicate);
                }
            }

            return predicate;
        }

        /// <summary>
        /// Generate sorting lambda for given <see cref="SortItem"/>
        /// </summary>
        /// <typeparam name="TEntity">Base parameter type</typeparam>
        /// <param <see cref="SortItem"/> name="sortitem">Contains direction and member</param>
        /// <returns></returns>
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

        public static bool IsNullableType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        //public static bool IsDateTimeType(Type type)
        //{
        //    if (type == null)
        //    {
        //        throw new ArgumentNullException(nameof(type));
        //    }

        //    return type.Equals(typeof(DateTime));
        //}

        private string GetTypeName(Type type)
        {
            if (IsNullableType(type))
            {
                //var a = Nullable.GetUnderlyingType(type);
                return Nullable.GetUnderlyingType(type).FullName;
            }

            return type.FullName;

            //var fullName = type.FullName;
            //if (type.FullName.Contains("System.DateTime") && type.FullName.Contains("System.Nullable"))
            //    fullName = "System.DateTime";
            //else if (type.FullName.Contains("System.Guid") && type.FullName.Contains("System.Nullable"))
            //    fullName = "System.Guid";
            //else if (
            //    (!type.IsGenericType && type.IsEnum)
            //    || (type.IsGenericType && Nullable.GetUnderlyingType(type).BaseType == typeof(Enum))
            //    )
            //    fullName = "System.Enum";

            //return fullName;
        }

        /// <summary>
        /// Set constant for given <see cref="PropertyInfo"/>
        /// </summary>
        /// <param name="prop"><see cref="PropertyInfo"/> of member</param>
        /// <param name="value">String value for constant. This value will convert to proprty type</param>
        /// <returns></returns>
        public Expression ToExpressionConstant(Type type, string value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty(value))
                return Expression.Constant(null);

            var typeFullname = GetTypeName(type);

            object val;
            switch (typeFullname)
            {
                case "System.Guid":
                    val = Guid.Parse(value);
                    break;
                case "System.DateTime":
                    val = CultureFormatter.ParseDateTimeStringFromCulture(value, cultureInfo);
                    break;
                case "System.Enum":
                    val = Int32.Parse(value);
                    break;
                default:
                    Type t = Nullable.GetUnderlyingType(type) ?? type;
                    val = Convert.ChangeType(value, Type.GetType(t.FullName));
                    break;
            }
            return Expression.Constant(val, type);

        }


    }
}