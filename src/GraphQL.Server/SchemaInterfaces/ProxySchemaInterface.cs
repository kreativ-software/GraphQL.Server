using GraphQL.Client;
using System;

namespace GraphQL.Server.Interfaces
{
    class ProxySchemaInterface : ISchemaInterface
    {
        private Type _type;
        private ITransport _transport;

        public ProxySchemaInterface(Type type, ITransport transport)
        {
            _type = type;
            _transport = transport;
        }
    }
}
