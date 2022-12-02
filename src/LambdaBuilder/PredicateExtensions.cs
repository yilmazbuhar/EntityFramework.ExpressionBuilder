namespace LambdaBuilder
{
    public static class PredicateExtensions
    {
        /// <summary>
        /// Applying filter, sort and paging. Formatter for query is <see cref="JsonQueryFormatter"/>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static async Task<IQueryable<TEntity>> ApplyFilterAndSort<TEntity>(this IQueryable<TEntity> source,
            string query) where TEntity : class
        {
            return await source.ApplyFilterAndSort<TEntity>(query, new JsonQueryFormatter());
        }

        /// <summary>
        /// Applying filter, sort and paging.
        /// </summary>
        /// <typeparam name="TEntity">Main <see cref="IQueryable"/>type.</typeparam>
        /// <param name="source">Main <see cref="IQueryable"/>type.</param>
        /// <param name="query">String query. Default format is json</param>
        /// <param name="queryFormatter">Formatter for query string. Default formatter <see cref="JsonQueryFormatter"/></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<IQueryable<TEntity>> ApplyFilterAndSort<TEntity>(this IQueryable<TEntity> source,
            string query,
            IQueryFormatter queryFormatter) where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(queryFormatter, "queryFormatter");

            var conditions = await queryFormatter.Compile(query);

            var predicateBuilder = new PredicateLambdaBuilder();
            var lambda = await predicateBuilder.GenerateConditionLambda<TEntity>(conditions.Where, conditions.LogicalOperator);

            source = lambda == null ? source : source.Where(lambda);
            source = conditions.Sort == null ? source : source.OrderBy(conditions.Sort);
            source = conditions.Paging == null ? source : source
                .Skip(conditions.Paging.PageCount * conditions.Paging.PageSize)
                .Take(conditions.Paging.PageSize);

            return source;
        }
    }
}