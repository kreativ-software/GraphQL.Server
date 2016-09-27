using Newtonsoft.Json;

namespace GraphQL
{
    [JsonConverter(typeof(ExecutionResultJsonConverter))]
    public class ExecutionResult
    {
        public object Data { get; set; }

        public ExecutionErrors Errors { get; set; }
    }
}
