namespace LambdaBuilder;

public class SortItem
{
    public string Field { get; set; }
    public string Direction { get; set; } = "ASC";
    public SortDirection SortDirection => Direction.ToLower() == "asc" ? SortDirection.ASC : SortDirection.DESC;
}
