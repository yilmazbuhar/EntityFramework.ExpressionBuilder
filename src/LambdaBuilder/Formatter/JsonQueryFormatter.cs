using System.Text;
using System.Text.Json;

namespace LambdaBuilder;

public class JsonQueryFormatter : IQueryFormatter
{
    public string Name => "jsonformatter";

    public async Task<QueryConditions> Compile(string query)
    {
        var condition = await JsonSerializer.DeserializeAsync<QueryConditions>(new MemoryStream(Encoding.UTF8.GetBytes(query)));

        // todo: It may doesn't right decision
        ArgumentNullException.ThrowIfNull(condition, "queryitems");

        condition.Where = condition.Where
            .Where(x => x.Active)
            .ToList();

        return condition;
    }
}
