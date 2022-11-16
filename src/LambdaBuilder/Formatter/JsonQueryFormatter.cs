using System.Text;
using System.Text.Json;

namespace LambdaBuilder;

public class JsonQueryFormatter : IQueryFormatter
{
    public string Name => "jsonformatter";

    public async Task<List<QueryItem>> Compile(string query)
    {
        var queryItems = await JsonSerializer.DeserializeAsync<List<QueryItem>>(new MemoryStream(Encoding.UTF8.GetBytes(query)));

        // todo: It may doesn't right decision
        ArgumentNullException.ThrowIfNull(queryItems, "queryitems");

        return queryItems
            .Where(x => x.Active)
            .ToList();
    }
}
