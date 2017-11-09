using Newtonsoft.Json.Linq;

namespace GraphQL.Client.Transport
{
    public interface ITransportOutput
    {
        JObject Data { get; }
        string Operation { get; }
        string Variables { get; }
    }
}