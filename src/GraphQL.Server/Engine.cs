using GraphQL.Client;
using GraphQL.Client.Transport;
using GraphQL.Server.Types;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GraphQL.Server
{
    public class Engine
    {
        private Schema _schema;
        private List<ISchemaInfo> _schemaInfos;
        private List<TypeDefinition> _typeDefinitions;

        public static Engine New()
        {
            return new Engine();
        }

        private Engine()
        {
            _schemaInfos = new List<ISchemaInfo>();
            _typeDefinitions = new List<TypeDefinition>();
        }

        public Engine AddSchema<TSchemaInfo>() where TSchemaInfo : class
        {
            _schemaInfos.Add(new SchemaInfo<TSchemaInfo>());
            return this;
        }

        public Engine AddProxy<TSchemaInfo>(ITransport transport) where TSchemaInfo : class
        {
            _schemaInfos.Add(new ProxySchemaInfo<TSchemaInfo>(transport));
            return this;
        }

        public Engine AddTypes(Assembly assembly)
        {
            var inputType = typeof(InputObjectGraphType);
            var outputType = typeof(ObjectGraphType);
            var inputTypes = assembly.GetExportedTypes().Where(t => inputType.IsAssignableFrom(t));
            var outputTypes = assembly.GetExportedTypes().Where(t => outputType.IsAssignableFrom(t));

            _typeDefinitions.AddRange(inputTypes.Select(t => new TypeDefinition(t, TypeKind.Input)));
            _typeDefinitions.AddRange(outputTypes.Select(t => new TypeDefinition(t, TypeKind.Output)));
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
            {
                Mutation = new Mutation(),
                Query = new Query()
            };
            var typeInfoList = RegisterTypes();
            foreach (var schemaInfo in _schemaInfos)
            {
                schemaInfo.AddFields(_schema, _typeDefinitions);
                //mutation.AddFields(schemaInfo.GetMutationFields());
                //query.AddFields(schemaInfo.GetQueryFields());
            }

            
        }

        private List<TypeInfo> RegisterTypes()
        {
            // Discover all types
            //foreach (var schemaInfo in _schemaInfos)
            //{
            //    var schemaTypeDefinitions = schemaInfo.GetTypeDefinitions();
            //    _typeDefinitions.AddRange(schemaTypeDefinitions);
            //}
            // Remove duplicates
            for (var ct = _typeDefinitions.Count - 1; ct >= 0; ct--)
            {
                if (_typeDefinitions.Count(td => td.GraphType.FullName == _typeDefinitions[ct].GraphType.FullName) > 1)
                {
                    _typeDefinitions.RemoveAt(ct);
                }
            }
            // Register types
            var methodInfo = typeof(Schema).GetMethod(nameof(_schema.RegisterType));
            _typeDefinitions.ForEach(td => methodInfo.MakeGenericMethod(td.GraphType).Invoke(_schema, null));
            return _typeDefinitions.Select(td => Activator.CreateInstance(td.))
        }
    }
}
