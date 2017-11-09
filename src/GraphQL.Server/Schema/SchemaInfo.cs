using GraphQL.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GraphQL.Server.Types
{
    class SchemaInfo<TSchema> : ISchemaInfo
    {
        private Type _mutationType;
        private Type _queryType;
        private Type[] _excludedInputTypes = new[] { typeof(ResolveFieldContext) };

        public SchemaInfo()
        {
            var properties = typeof(TSchema).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var mutationProperty = properties.FirstOrDefault(p => p.Name.Equals(nameof(Mutation), StringComparison.OrdinalIgnoreCase));
            var queryProperty = properties.FirstOrDefault(p => p.Name.Equals(nameof(Query), StringComparison.OrdinalIgnoreCase));

            _mutationType = mutationProperty?.PropertyType;
            _queryType = queryProperty?.PropertyType;
        }

        public void AddFields(Schema schema, List<TypeDefinition> typeDefinitions)
        {
            if (_mutationType == null) return;
            foreach (var methodInfo in _mutationType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (methodInfo.IsSpecialName ||
                    methodInfo.ReturnType == typeof(void)) continue;

                var parameters = methodInfo.GetParameters().Where(p => !_excludedInputTypes.Contains(p.ParameterType)).ToList();
                if (parameters.Any())
                {
                    parameters.ForEach(p => RegisterField(schema, typeDefinitions, p.ParameterType, TypeKind.Input));
                }
                RegisterField(schema, typeDefinitions, methodInfo.ReturnType, TypeKind.Output);
            }
        }

        private TypeDefinition RegisterField(Schema schema, List<TypeDefinition> typeDefinitions, Type type, TypeKind input)
        {
            if (typeDefinitions.Any(td => td.GraphType.FullName == type.FullName)) return;

        }
    }
}
