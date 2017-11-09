using Newtonsoft.Json.Linq;

namespace GraphQL.Client
{
    public class ClientQuery<TOutput> : IClientQuery
    {
        public TOutput Data { get; private set; }
        public object Output => Data;
        public string QueryString { get; private set; }

        public ClientQuery(string queryString)
        {
            QueryString = queryString;
        }

        public void SetOutput(JToken token)
        {
            if (token != null)
            {
                Data = token.ToObject<TOutput>();
            }
        }
    }
}