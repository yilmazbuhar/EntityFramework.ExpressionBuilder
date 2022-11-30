using System.Linq.Expressions;

namespace LambdaBuilder
{
    public interface IOperator
    {
        string Name { get; }
        Expression<Func<TEntity, bool>> Invoke<TEntity>(ParameterExpression paramExp, Expression left, Expression right);
    }
}