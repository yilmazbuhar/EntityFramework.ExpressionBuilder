using System.Linq.Expressions;
using System.Reflection;

namespace LambdaBuilder
{
    public class NotContainsOperator : IOperator
    {
        public string Name => "notcontains";

        public Expression<Func<TEntity, bool>> Invoke<TEntity>(ParameterExpression paramExp, Expression left, Expression right)
        {
            var stringComparisonParameter =
                Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison));

            Expression<Func<TEntity, bool>> returnExp;

            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
            MethodCallExpression expMethod = Expression.Call(left, method, new Expression[] { right, stringComparisonParameter });
            UnaryExpression negateExpression = Expression.Not(expMethod);

            return Expression.Lambda<Func<TEntity, bool>>(negateExpression, paramExp);
        }
    }
}