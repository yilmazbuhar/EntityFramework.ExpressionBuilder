using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace LambdaBuilder
{
    public static class SortExtensions
    {
        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, List<SortItem> sortitems) where TEntity : class
        {
            var orderExpression = source.Expression;

            if (!sortitems.Any())
                return source.Provider.CreateQuery<TEntity>(orderExpression);

            string command = null;
            int i = 0;
            foreach (var sortitem in sortitems)
            {
                if (i == 0) command = sortitem.SortDirection == SortDirection.ASC ? "OrderBy" : "OrderByDescending";
                else command = sortitem.SortDirection == SortDirection.ASC ? "ThenBy" : "ThenByDescending";

                var type = typeof(TEntity);
                var parameter = Expression.Parameter(type, "orderparameter");

                PropertyInfo property = SearchProperty(typeof(TEntity), sortitem.Field);
                MemberExpression propertyAccess = Expression.MakeMemberAccess(parameter, property);

                var orderByExpression = Expression.Lambda(propertyAccess, parameter);

                orderExpression = Expression.Call(typeof(Queryable),
                    command,
                    new Type[] { type, property.PropertyType },
                    orderExpression, Expression.Quote(orderByExpression));

                i++;
            }

            return source.Provider.CreateQuery<TEntity>(orderExpression);
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string sortProperty, ListSortDirection sortOrder)
        {
            var type = typeof(T);
            var parameter = Expression.Parameter(type, "p");

            var property = type.GetProperty(sortProperty);

            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            var typeArguments = new Type[] { type, property.PropertyType };
            var methodName = sortOrder == ListSortDirection.Ascending ? "OrderBy" : "OrderByDescending";
            var resultExp = Expression.Call(typeof(Queryable), methodName, typeArguments, source.Expression, Expression.Quote(orderByExp));

            return source.Provider.CreateQuery<T>(resultExp);
        }

        private static PropertyInfo SearchProperty(Type type, string propertyName)
        {
            foreach (var item in type.GetProperties())
                if (item.Name.ToLower() == propertyName.ToLower())
                    return item;
            return null;
        }
    }
}