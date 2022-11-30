using LambdaBuilder.Infra;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace LambdaBuilder
{

    public class PredicateLambdaBuilder
    {
        /// <summary>
        /// Generic property finder. Supports subtypes
        /// </summary>
        /// <typeparam name="TEntity">Base type to generating lambda</typeparam>
        /// <param name="parameter">Pass existing parameter to prevent create new parameter</param>
        /// <param name="member"></param>
        /// <returns></returns>
        public Expression CreateProperty<TEntity>(ParameterExpression parameter, string member)
        {
            //var p = Expression.Parameter(typeof(T));
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

        public async Task<Expression<Func<TEntity, bool>>> GenerateConditionLambda<TEntity>(List<QueryItem> conditions)
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
                var constant = Expression.Constant(condition.Value, property.Type);

                var types = ReflectionHelper.GetTypeOf<IOperator>();
                foreach (var item in types)
                {
                    var instance = (IOperator)Activator.CreateInstance(item);
                    if (item.Name == condition.Operator)
                        returnExp = instance.Invoke<TEntity>(parameter, property, constant);
                }

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
    }
}