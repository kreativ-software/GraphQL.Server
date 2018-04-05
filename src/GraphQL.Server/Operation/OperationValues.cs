using System;

namespace GraphQL.Server.Operation
{
    public class OperationValues
    {
        public object Input { get; set; }
        public InputField[] Fields { get; set; }
        public string FieldName { get; set; }
        public Func<object, InputField[], object> Method { get; set; }
    }
}
