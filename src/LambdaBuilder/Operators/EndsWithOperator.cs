using System.Linq.Expressions;
using System.Reflection;

namespace LambdaBuilder
{
    public class EndsWithOperator : IOperator
    {
        public string Name => "endswith";

        public Expression<Func<TEntity, bool>> Invoke<TEntity>(ParameterExpression paramExp, Expression left, Expression right)
        {
            Expression<Func<TEntity, bool>> returnExp;

            MethodInfo method = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
            MethodCallExpression expMethod = Expression.Call(left, method, right);

            return Expression.Lambda<Func<TEntity, bool>>(expMethod, paramExp);
        }
    }
}