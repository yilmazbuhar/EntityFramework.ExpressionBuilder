using System.Linq.Expressions;

namespace LambdaBuilder
{
    public interface IWhereClause
    {
        //public IQueryable<T> Where<T>(this IQueryable<T> query, string selector, string comparer, string value)
        //{
        //    var target = Expression.Parameter(typeof(T));

        //    return query.Provider.CreateQuery<T>(CreateWhereClause(target, query.Expression, selector, comparer, value));
        //}

        public Expression CreateWhereClause(ParameterExpression target, Expression expression, string selector, string comparer, string value)
        {
            var predicate = Expression.Lambda(CreateComparison(target, selector, comparer, value), target);

            return Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { target.Type },
                expression, Expression.Quote(predicate));
        }

        public Expression CreateComparison(ParameterExpression target, string selector, string comparer, string value)
        {
            var memberAccess = CreateMemberAccess(target, selector);
            var actualValue = Expression.Constant(value, typeof(string));

            return Expression.Call(memberAccess, comparer, null, actualValue);
        }

        public Expression CreateMemberAccess(Expression target, string selector)
        {
            return selector.Split('.').Aggregate(target, (t, n) => Expression.PropertyOrField(t, n));
        }
    }

    public class WhereClause : IWhereClause
    {
        public static IQueryable<T> Where<T>(this IQueryable<T> query, string selector, string comparer, string value)
        {
            var target = Expression.Parameter(typeof(T));

            return query.Provider.CreateQuery<T>(CreateWhereClause(target, query.Expression, selector, comparer, value));
        }

        static Expression CreateWhereClause(ParameterExpression target, Expression expression, string selector, string comparer, string value)
        {
            var predicate = Expression.Lambda(CreateComparison(target, selector, comparer, value), target);

            return Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { target.Type },
                expression, Expression.Quote(predicate));
        }

        static Expression CreateComparison(ParameterExpression target, string selector, string comparer, string value)
        {
            var memberAccess = CreateMemberAccess(target, selector);
            var actualValue = Expression.Constant(value, typeof(string));

            return Expression.Call(memberAccess, comparer, null, actualValue);
        }

        static Expression CreateMemberAccess(Expression target, string selector)
        {
            return selector.Split('.').Aggregate(target, (t, n) => Expression.PropertyOrField(t, n));
        }
    }
}
