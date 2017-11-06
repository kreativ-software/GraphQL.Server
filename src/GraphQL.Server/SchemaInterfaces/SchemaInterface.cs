using System;

namespace GraphQL.Server.Interfaces
{
    class SchemaInterface : ISchemaInterface
    {
        private Type _type;

        public SchemaInterface(Type type)
        {
            _type = type;
        }
    }
}
