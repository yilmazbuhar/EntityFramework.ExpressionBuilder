using System.Linq.Expressions;

namespace LambdaBuilder
{
    public class LessThanOperator : IOperator
    {
        public string Name => "lessthanorequal";

        public Expression<Func<TEntity, bool>> Invoke<TEntity>(ParameterExpression paramExp, Expression left, Expression right)
        {
            return Expression.Lambda<Func<TEntity, bool>>(Expression.LessThan(left, right));
        }
    }
}