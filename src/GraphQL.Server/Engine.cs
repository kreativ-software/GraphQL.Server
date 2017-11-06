using GraphQL.Client;
using GraphQL.Server.Interfaces;
using GraphQL.Types;
using System;
using System.Collections.Generic;

namespace GraphQL.Server
{
    public class Engine
    {
        private Schema _schema;
        private List<ISchemaInterface> _schemaInterfaces;

        public static Engine New()
        {
            return new Engine();
        }

        private Engine()
        {
            _schemaInterfaces = new List<ISchemaInterface>();
        }

        public Engine AddSchema<TSchemaInterface>() where TSchemaInterface : class
        {
            _schemaInterfaces.Add(new SchemaInterface(typeof(TSchemaInterface)));
            return this;
        }

        public Engine AddProxy<TSchemaInterface>(ITransport transport) where TSchemaInterface : class
        {
            _schemaInterfaces.Add(new ProxySchemaInterface(typeof(TSchemaInterface), transport));
            return this;
        }

        public Engine Compile()
        {
            if (_schema == null) CompileSchema();
            return this;
        }

        private void CompileSchema()
        {
            _schema = new Schema
        }
    }
}
