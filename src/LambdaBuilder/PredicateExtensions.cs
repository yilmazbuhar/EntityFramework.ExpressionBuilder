namespace LambdaBuilder
{
    public static class PredicateExtensions
    {
        /// <summary>
        /// Applying filter, sort and paging.
        /// </summary>
        /// <typeparam name="TEntity">Main <see cref="IQueryable"/>type.</typeparam>
        /// <param name="source">Main <see cref="IQueryable"/>type.</param>
        /// <param name="query">String query. Default format is json</param>
        /// <param name="queryFormatter">Formatter for query string. Default formatter <see cref="JsonQueryFormatter"/></param>
        /// <returns></returns>
        public static async Task<IQueryable<TEntity>> ApplyFilterAndSort<TEntity>(this IQueryable<TEntity> source,
            string query,
            IQueryFormatter queryFormatter) where TEntity : class
        {
            if (queryFormatter == null)
                queryFormatter = new JsonQueryFormatter();

            var conditions = await queryFormatter.Compile(query);

            var predicateBuilder = new PredicateLambdaBuilder();
            var lambda = await predicateBuilder.GenerateConditionLambda<TEntity>(conditions.Where);

            source = lambda == null ? source : source.Where(lambda);
            source = conditions.Sort == null ? source : source.OrderBy(conditions.Sort);
            source = conditions.Paging == null ? source : source
                .Skip(conditions.Paging.PageCount * conditions.Paging.PageSize)
                .Take(conditions.Paging.PageSize);

            return source;
        }
    }
}