using Newtonsoft.Json;

namespace ExpressionTest
{
    public interface IQueryFormatter
    {
        /// <summary>
        /// for di
        /// we can select which formatter tobe use on appsettings file
        /// </summary>
        string Name { get;}
        List<QueryItem> Compile(string query);
    }

    public class JsonQueryFormatter : IQueryFormatter
    {
        public string Name => "jsonformatter";

        public List<QueryItem> Compile(string query)
        {
            return JsonConvert.DeserializeObject<List<QueryItem>>(query).Where(x => x.Active).ToList();
        }
    }
}
