using System.Linq.Expressions;
using System.Reflection;

namespace LambdaBuilder
{
    public class NotStartsWithOperator : IOperator
    {
        public string Name => "notstartswith";

        public Expression<Func<TEntity, bool>> Invoke<TEntity>(ParameterExpression paramExp, Expression left, Expression right)
        {
            Expression<Func<TEntity, bool>> returnExp;

            MethodInfo method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            MethodCallExpression expMethod = Expression.Call(left, method, right);
            UnaryExpression negateExpression = Expression.Not(expMethod);

            return Expression.Lambda<Func<TEntity, bool>>(negateExpression, paramExp);
        }
    }
}