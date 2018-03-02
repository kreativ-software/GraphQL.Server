using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Server
{
    public class GraphQLQuery
    {
        public string Query { get; set; }
        public Dictionary<string, object> Variables { get; set; }

        public Inputs GetInputs()
        {
            //if (string.IsNullOrEmpty(Variables)) return null;
            //var variables = Deserialize(Variables);
            return Variables == null ? null : new Inputs(Variables);
        }

        private Dictionary<string, object> Deserialize(string json)
        {
            var output = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (output == null) return null;
            var keys = output.Keys.ToList();
            for (var ct = 0; ct < output.Count; ct++)
            {
                if (output[keys[ct]] is JObject) output[keys[ct]] = Deserialize((output[keys[ct]] as JObject).ToString());
            }
            return output;
        }
    }
}