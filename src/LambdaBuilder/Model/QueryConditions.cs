namespace LambdaBuilder;

public class QueryConditions
{
    /// <summary>
    /// Determine filter parameters. To ignore filtering, pass null.
    /// </summary>
    public List<QueryItem> Where { get; set; }

    /// <summary>
    /// Determine sorting parameters. To ignore sorting, pass null.
    /// </summary>
    public List<SortItem> Sort { get; set; }

    /// <summary>
    /// Determine paging parameters. To ignore, pass null.
    /// </summary>
    public Paging Paging { get; set; }
}