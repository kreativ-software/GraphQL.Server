using GraphQL.Types;
using System.Collections.Generic;

namespace GraphQL.Server.Types
{
    interface ISchemaInfo
    {
        void AddFields(Schema schema, List<TypeDefinition> typeDefinitions);
    }
}
