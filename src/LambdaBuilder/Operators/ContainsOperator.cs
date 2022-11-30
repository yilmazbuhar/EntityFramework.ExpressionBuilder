using System.Linq.Expressions;
using System.Reflection;

namespace LambdaBuilder
{
    public class ContainsOperator : IOperator
    {
        public string Name => "contains";

        public Expression<Func<TEntity, bool>> Invoke<TEntity>(ParameterExpression parameter, Expression property, Expression constant)
        {
            var stringComparisonParameter = Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison));

            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
            MethodCallExpression expMethod = Expression.Call(property, method, new Expression[] { constant, stringComparisonParameter });

            return Expression.Lambda<Func<TEntity, bool>>(expMethod, parameter);
        }
    }
}