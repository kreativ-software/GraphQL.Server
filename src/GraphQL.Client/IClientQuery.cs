using Newtonsoft.Json.Linq;

namespace GraphQL.Client
{
    public interface IClientQuery
    {
        object Output { get; }
        string QueryString { get; }
        void SetOutput(JToken value);
    }
}
