using GraphQL.Client.Transport;

namespace GraphQL.Server.Types
{
    class ProxySchemaInfo<TSchema> : ISchemaInfo
    {
        private ITransport _transport;

        public ProxySchemaInfo(ITransport transport)
        {
            _transport = transport;
        }
    }
}
