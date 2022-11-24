namespace LambdaBuilder;

public class Condition
{
    public List<QueryItem> Where { get; set; }
    public List<SortItem> Sort { get; set; }
    public Paging Paging { get; set; }
}