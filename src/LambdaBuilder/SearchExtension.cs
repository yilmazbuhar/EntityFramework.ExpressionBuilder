namespace LambdaBuilder
{
    public static class SearchExtension
    {
        /// <summary>
        /// Default formatter <see cref="JsonQueryFormatter"/>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="query"></param>
        /// <param name="queryFormatter"></param>
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
            //var sort = await predicateBuilder.GenerateSortLambda<TEntity>(conditions.Sort[0]);

            source = lambda == null ? source : source.Where(lambda);
            source = source.OrderBy(conditions.Sort);

            return source;
        }
    }
}