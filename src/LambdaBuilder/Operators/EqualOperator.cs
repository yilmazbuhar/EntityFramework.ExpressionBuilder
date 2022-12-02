using System.Linq.Expressions;

namespace LambdaBuilder
{
    public class EqualOperator : IOperator
    {
        public string Name => "equal";

        public Expression<Func<TEntity, bool>> Invoke<TEntity>(ParameterExpression paramExp, Expression left, Expression right)
        {
            return Expression.Lambda<Func<TEntity, bool>>(Expression.Equal(left, right), paramExp);
        }
    }
}