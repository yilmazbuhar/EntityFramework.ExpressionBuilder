using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionTest
{
    public static class PredicateBuilderExtensions
    {
        
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> AndAlso<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(left, right), parameter);
        }

        private class ReplaceExpressionVisitor
            : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }

        public static Expression ToExprConstant(PropertyInfo prop, string value)
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
