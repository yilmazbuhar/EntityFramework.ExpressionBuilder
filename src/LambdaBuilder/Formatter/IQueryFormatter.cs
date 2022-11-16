namespace LambdaBuilder;

public interface IQueryFormatter
{
    /// <summary>
    /// For DI
    /// we can select which formatter tobe use on appsettings file
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Compile string to <see cref="QueryItem"/> from given format
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    Task<List<QueryItem>> Compile(string query);
}
