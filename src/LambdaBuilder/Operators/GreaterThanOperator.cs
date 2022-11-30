using System.Linq.Expressions;

namespace LambdaBuilder
{
    public class GreaterThanOperator : IOperator
    {
        public string Name => "greaterthan";

        public Expression<Func<TEntity, bool>> Invoke<TEntity>(ParameterExpression paramExp, Expression left, Expression right)
        {
            return Expression.Lambda<Func<TEntity, bool>>(Expression.GreaterThan(left, right));
        }
    }
}