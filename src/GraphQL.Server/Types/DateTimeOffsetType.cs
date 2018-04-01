using GraphQL.Language.AST;
using GraphQL.Types;
using System;

namespace GraphQL.Server.Types
{
    public class DateTimeOffsetType : ScalarGraphType
    {
        public DateTimeOffsetType()
        {
            Name = "DateTimeOffset";
        }

        public override object Serialize(object value)
        {
            return ParseValue(value);
        }

        public override object ParseValue(object value)
        {
            DateTimeOffset result;
            if (DateTimeOffset.TryParse(value?.ToString().Trim('\"'), out result))
            {
                return result;
            }
            return null;
        }

        public override object ParseLiteral(IValue value)
        {
            if (value is StringValue)
            {
                return ParseValue(((StringValue)value).Value);
            }
            return null;
        }
    }
}