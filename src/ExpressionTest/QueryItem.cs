// See https://aka.ms/new-console-template for more information
public class QueryItem
{
    public string Member { get; set; }
    public string Operator { get; set; }
    public string Value { get; set; }
    public string LogicalOperator { get; set; }
    public bool Active { get; set; }

    public override string ToString()
    {
        return $" key: {Member} operator: {Operator} value: {Value}";
    }
}