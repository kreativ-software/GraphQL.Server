using System;
using System.Collections.Generic;

namespace GraphQL.Client
{
    public class ClientError : Exception
    {
        public ClientError(string message)
            : base(message)
        {
        }

        public ClientError(string message, Exception exception)
            : base(message, exception)
        {
            var ex = exception?.InnerException ?? exception;
        }

        public IEnumerable<ErrorLocation> Locations { get; private set; }

        public string Code { get; set; }

        public List<string> Path { get; private set; } = new List<string>();

        public new Dictionary<string, object> Data { get; private set; } = null;
    }

    public class ErrorLocation
    {
        public int Line { get; set; }
        public int Column { get; set; }
    }
}
