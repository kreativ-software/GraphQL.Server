using GraphQL.Client.Transport;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GraphQL.Client
{
    public class Client
    {
        public ITransport Transport { get; private set; }
        private List<IClientQuery> _queries;

        public Client(ITransport transport)
        {
            Transport = transport;
            _queries = new List<IClientQuery>();
        }

        public ClientQuery<TOutput> AddQuery<TOutput>(string queryString)
        {
            var query = new ClientQuery<TOutput>(queryString);
            _queries.Add(query);
            return query;
        }

        public IClientQuery AddQuery(Type outputType, string queryString)
        {
            var clientQueryType = typeof(ClientQuery<>).MakeGenericType(outputType);
            var query = (IClientQuery)Activator.CreateInstance(clientQueryType, queryString);
            _queries.Add(query);
            return query;
        }

        public async Task<ClientOutput> RunAsync(OperationType operationType, string name = null, string variables = null)
        {
            var operation = operationType == OperationType.Mutation ? "mutation" : "query";
            var queryStrings = string.Join("\r\n", _queries.Select(q => q.QueryString));
            operation = $"{operation} {name ?? string.Empty} {{ {queryStrings} }}";
            var transportOutput = await Transport.SendAsync(operation, variables).ConfigureAwait(false);
            var output =  new ClientOutput(transportOutput);
            if (output.Success)
            {
                foreach (var pair in output.Data)
                {
                    var query = _queries.FirstOrDefault(q => q.QueryString.StartsWith(pair.Key));
                    if (query == null) continue;
                    query.SetOutput(pair.Value);
                }
            }
            _queries.Clear();
            return output;
        }
    }
}