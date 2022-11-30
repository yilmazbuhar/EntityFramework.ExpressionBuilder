namespace LambdaBuilder;

public class QueryItem
{
    public string Member { get; set; }
    public string Operator { get; set; }
    public string Value { get; set; }
    public bool Active { get; set; }

    public override string ToString()
    {
        return $" key: {Member} operator: {Operator} value: {Value}";
    }
}