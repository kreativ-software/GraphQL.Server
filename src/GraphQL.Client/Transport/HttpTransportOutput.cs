using Newtonsoft.Json.Linq;

namespace GraphQL.Client.Transport
{
    public class HttpTransportOutput : ITransportOutput
    {
        public string Content { get; private set; }
        public JObject Data { get; private set; }
        public string Operation { get; private set; }
        public string Variables { get; private set; }

        public HttpTransportOutput(string content, string operation, bool success, string variables)
        {
            Content = content;
            Data = JObject.Parse(content);
            Operation = operation;
            Variables = variables;
        }
    }
}