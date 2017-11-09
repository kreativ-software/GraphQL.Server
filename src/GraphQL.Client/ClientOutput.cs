using GraphQL.Client.Transport;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client
{
    public class ClientOutput
    {
        public JObject Data { get; private set; }
        public ClientError[] Errors { get; private set; }
        public bool Success { get; }
        public ITransportOutput TransportOutput { get; private set; }
        
        public ClientOutput(ITransportOutput transportOutput)
        {
            TransportOutput = transportOutput;

            Data = TransportOutput.Data.Value<JObject>("data");
            var errors = TransportOutput.Data.Value<JArray>("errors");
            Errors = errors == null ? new ClientError[0] : errors.ToObject<ClientError[]>();
            Success = Data != null && Errors.Length == 0;
        }
    }
}