using System;
using GraphQL.Types;

namespace GraphQL.Server.Operation
{
    public class OperationValues
    {
        public object Input { get; set; }
        public InputField[] Fields { get; set; }
        public string FieldName { get; set; }
        public object Output { get; set; }
        public ResolveFieldContext<object> Context { get; set; }
        public Attribute[] FunctionAttributes { get; set; }
    }
}
