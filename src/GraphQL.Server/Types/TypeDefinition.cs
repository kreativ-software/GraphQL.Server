using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQL.Server.Types
{
    public class TypeDefinition
    {
        public Type GraphType { get; private set; }
        public TypeKind Kind { get; private set; }

        public TypeDefinition(Type graphType, TypeKind kind)
        {
            GraphType = graphType;
            BasicType = 
            Kind = kind;
        }
    }
}
