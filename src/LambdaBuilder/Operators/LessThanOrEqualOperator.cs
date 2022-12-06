using System.Linq.Expressions;

namespace LambdaBuilder
{
    public class LessThanOrEqualOperator : IOperator
    {
        public string Name => "lessthan";

        public Expression<Func<TEntity, bool>> Invoke<TEntity>(ParameterExpression paramExp, Expression left, Expression right)
        {
            return Expression.Lambda<Func<TEntity, bool>>(Expression.LessThanOrEqual(left, right), paramExp);
        }
    }
}