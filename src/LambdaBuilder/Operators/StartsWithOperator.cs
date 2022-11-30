using System.Linq.Expressions;
using System.Reflection;

namespace LambdaBuilder
{
    public class StartsWithOperator : IOperator
    {
        public string Name => "startswith";

        public Expression<Func<TEntity, bool>> Invoke<TEntity>(ParameterExpression paramExp, Expression left, Expression right)
        {
            Expression<Func<TEntity, bool>> returnExp;

            MethodInfo method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            MethodCallExpression expMethod = Expression.Call(left, method, right);

            return Expression.Lambda<Func<TEntity, bool>>(expMethod, paramExp);
        }
    }
}