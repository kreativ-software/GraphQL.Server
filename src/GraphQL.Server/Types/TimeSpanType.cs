using GraphQL.Language.AST;
using GraphQL.Types;
using System;

namespace GraphQL.Server.Types
{
    public class TimeSpanType : ScalarGraphType
    {
        public TimeSpanType()
        {
            Name = "TimeSpan";
        }

        public override object Serialize(object value)
        {
            return ParseValue(value);
        }

        public override object ParseValue(object value)
        {
            TimeSpan result;
            if (TimeSpan.TryParse(value?.ToString().Trim('\"'), out result))
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