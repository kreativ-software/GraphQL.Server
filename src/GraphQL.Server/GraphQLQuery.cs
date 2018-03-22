using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace GraphQL.Server
{
    public class GraphQLQuery
    {
        public string Query { get; set; }
        public Dictionary<string, object> Variables { get; set; }

        public Inputs GetInputs()
        {
            if (Variables == null) return null;
            var inputVariables = Variables.ToDictionary(kv => kv.Key, kv => Deserialize(kv.Value));
            return new Inputs(inputVariables);
        }

        private object Deserialize(object val)
        {
            if (val is JArray)
            {
                var array = val as JArray;
                return array.Select(item => Deserialize(item)).ToArray();
            }
            if (val is JObject)
            {
                var obj = val as JObject;
                var dictionary = obj.ToObject<Dictionary<string, object>>();
                return dictionary.ToDictionary(kv => kv.Key, kv => Deserialize(kv.Value));
            }
            if (val is JValue)
            {
                return (val as JValue).Value;
            }
            return val;
        }
    }
}