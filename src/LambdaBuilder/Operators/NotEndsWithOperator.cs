using System.Linq.Expressions;
using System.Reflection;

namespace LambdaBuilder
{
    public class NotEndsWithOperator : IOperator
    {
        public string Name => "notendswith";

        public Expression<Func<TEntity, bool>> Invoke<TEntity>(ParameterExpression paramExp, Expression left, Expression right)
        {
            Expression<Func<TEntity, bool>> returnExp;

            MethodInfo method = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
            MethodCallExpression expMethod = Expression.Call(left, method, right);
            UnaryExpression negateExpression = Expression.Not(expMethod);

            return Expression.Lambda<Func<TEntity, bool>>(negateExpression, paramExp);
        }
    }
}