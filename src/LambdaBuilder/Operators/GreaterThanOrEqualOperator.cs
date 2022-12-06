using System.Linq.Expressions;

namespace LambdaBuilder
{
    public class GreaterThanOrEqualOperator : IOperator
    {
        public string Name => "greaterthanorequal";

        public Expression<Func<TEntity, bool>> Invoke<TEntity>(ParameterExpression paramExp, Expression left, Expression right)
        {
            return Expression.Lambda<Func<TEntity, bool>>(Expression.GreaterThanOrEqual(left, right), paramExp);
        }
    }
}