using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using CacheManager.Core;
using GraphQL.Language.AST;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace GraphQL.Client
{
    public class GraphClient
    {
        public static readonly string CacheIgnoreHeader = "cache-ignore-headers";
        public string ApiUrl { get; set; }
        public HttpClient HttpClient { get; private set; }
        public Dictionary<string, string> Headers { get; set; }
        private List<IGraphQuery> Queries { get; set; }
        private ICacheManager<object> CacheManager { get; set; }

        protected static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        public GraphClient(string apiUrl, HttpClient httpClient, Dictionary<string, string> headers = null, ICacheManager<object> cacheManager = null)
        {
            ApiUrl = apiUrl;
            HttpClient = httpClient;
            Headers = headers;
            if (HttpClient.BaseAddress == null)
            {
                HttpClient.BaseAddress = new Uri(ApiUrl);
            }
            Queries = new List<IGraphQuery>();
            CacheManager = cacheManager;
        }

        public GraphQuery<T> AddQuery<T>(string operation, T instance)
        {
            var query = new GraphQuery<T>(operation);
            Queries.Add(query);
            return query;
        }
        public GraphQuery<TOutput> AddQuery<TInput, TOutput>(string operation, TInput input, TOutput output)
        {
            if (input != null)
            {
                var inputString = GetInputString(input);
                operation = string.IsNullOrEmpty(inputString) ? $"{operation}" : $"{operation}({inputString})";
            }
            var query = new GraphQuery<TOutput>(operation);
            Queries.Add(query);
            return query;
        }
        public GraphQuery<JToken> AddSelectionQuery<TInput>(string operation, TInput input, IEnumerable<Field> selections) where TInput : class
        {
            if (input != null)
            {
                var inputString = GetInputString(input);
                operation = string.IsNullOrEmpty(inputString) ? $"{operation}" : $"{operation}({inputString})";
            }
            var query = new GraphQuery<JToken>(operation, selections);
            Queries.Add(query);
            return query;
        }

        public Task<GraphOutput> RunMutationsAsync(string name = "m", object variables = null, Func<GraphOutput, DateTime> cacheUntil = null)
        {
            return RunQueryTypeAsync("mutation", name, variables, cacheUntil);
        }
        public async Task<GraphOutput<T>> RunMutationAsync<T>(string operation, T instance, string name = "q", object variables = null, Func<T, DateTime> cacheUntil = null)
        {
            var query = AddQuery(operation, instance);
            var output = await RunMutationsAsync(name, variables, GetCacheUntilWrapper(cacheUntil, query)).ConfigureAwait(false);
            return new GraphOutput<T> { Data = query.Data, Errors = output.Errors };
        }
        public Task<GraphOutput> RunQueriesAsync(string name = "q", object variables = null, Func<GraphOutput, DateTime> cacheUntil = null)
        {
            return RunQueryTypeAsync("query", name, variables, cacheUntil);
        }
        public async Task<GraphOutput<T>> RunQueryAsync<T>(string operation, T instance, string name = "q", object variables = null, Func<T, DateTime> cacheUntil = null)
        {
            var query = AddQuery(operation, instance);
            var output = await RunQueriesAsync(name, variables, GetCacheUntilWrapper(cacheUntil, query)).ConfigureAwait(false);
            return new GraphOutput<T> { Data = query.Data, Errors = output.Errors };
        }

        public GraphOutput RunMutations(string name = "m", object variables = null, Func<GraphOutput, DateTime> cacheUntil = null)
        {
            return RunMutationsAsync(name, variables, cacheUntil).Result;
        }
        public GraphOutput<T> RunMutation<T>(string operation, T instance, string name = "m", object variables = null, Func<T, DateTime> cacheUntil = null)
        {
            return RunMutationAsync(operation, instance, name, variables, cacheUntil).Result;
        }
        public TOutput RunMutation<TInput, TOutput>(string operation, TInput input, TOutput output, Func<TOutput, DateTime> cacheUntil = null)
        {
            if (input != null)
            {
                var inputString = GetInputString(input);
                operation = string.IsNullOrEmpty(inputString) ? $"{operation}" : $"{operation}({inputString})";
            }
            var graphOutput = RunMutationAsync(operation, output, cacheUntil: cacheUntil).Result;

            if (graphOutput.Errors.Any())
            {
                throw new GraphClientException(JsonConvert.SerializeObject(graphOutput.Errors));
            }
            return graphOutput.Data;
        }

        public GraphOutput RunQueries(string name = "q", object variables = null)
        {
            return RunQueriesAsync(name, variables).Result;
        }
        public GraphOutput<T> RunQuery<T>(string operation, T instance, string name = "q", object variables = null, Func<T, DateTime> cacheUntil = null)
        {
            return RunQueryAsync(operation, instance, name, variables, cacheUntil).Result;
        }
        public TOutput RunQuery<TInput, TOutput>(string operation, TInput input, TOutput output, Func<TOutput, DateTime> cacheUntil = null)
        {
            if (input != null)
            {
                var inputString = GetInputString(input);
                operation = string.IsNullOrEmpty(inputString) ? $"{operation}" : $"{operation}({inputString})";
            }
            var graphOutput = RunQueryAsync(operation, output, cacheUntil: cacheUntil).Result;

            if (graphOutput.Errors.Any())
            {
                throw new GraphClientException(JsonConvert.SerializeObject(graphOutput.Errors));
            }
            return graphOutput.Data;
        }

        private async Task<GraphOutput> RunQueryTypeAsync(string queryType, string name, object variables, Func<GraphOutput, DateTime> cacheUntil)
        {
            // Build query
            var queries = Queries.Select(q => $"{q.Operation}{q.GetSelectFields()}");
            var fullQuery = $"{queryType} {name}{{{string.Join(" ", queries)}}}";
            var input = new
            {
                query = fullQuery,
                variables = variables == null ? null : JsonConvert.SerializeObject(variables)
            };
            // Post query
            string cacheKey = null;
            GraphOutput output = null;
            if (CacheManager != null && cacheUntil != null)
            {
                var headers = ConvertHeaders(HttpClient.DefaultRequestHeaders);
                cacheKey = GetCacheKey(input, headers);
                var cacheKeyExists = CacheManager.Exists(cacheKey);
                output = cacheKeyExists ? CacheManager.Get<GraphOutput>(cacheKey) : null;
            }
            if (output == null)
            {
                var inputContent = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
                if (Headers != null)
                {
                    foreach (var header in Headers)
                    {
                        if (inputContent.Headers.Contains(header.Key))
                        {
                            inputContent.Headers.Remove(header.Key);
                        }
                        inputContent.Headers.Add(header.Key, header.Value);
                    }
                }
                var response = HttpClient.PostAsync("", inputContent);
                //var response = await HttpClient.PostAsJsonAsync("", input).ConfigureAwait(false);
                var responseContent = await response.Result.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.Result.IsSuccessStatusCode)
                {
                    throw new HttpException((int)response.Result.StatusCode, responseContent);
                }
                var json = JObject.Parse(responseContent);
                var errors = json.Value<JArray>("errors");
                output = new GraphOutput
                {
                    Data = json.Value<JObject>("data"),
                    Errors = errors == null ? new GraphOutputError[0] : errors.ToObject<GraphOutputError[]>(),
                    Query = fullQuery,
                    Variables = input.variables
                };
            }
            // Populate output objects
            if (output.Data != null)
            {
                foreach (var pair in output.Data)
                {
                    var query = Queries.FirstOrDefault(q => q.Operation.Contains(pair.Key));
                    if (query == null) continue;
                    query.SetOutput(pair.Value);
                }
            }
            Queries.Clear();
            if (CacheManager != null)
            {
                if (cacheUntil != null)
                {
                    CacheManager.AddOrUpdate(cacheKey, output, _ => output);
                    var cacheUntilValue = cacheUntil(output);
                    if (cacheUntilValue != DateTime.MinValue)
                    {
                        CacheManager.Expire(cacheKey, cacheUntilValue);
                    }
                }
            }
            return output;
        }

        private Dictionary<string, string> ConvertHeaders(HttpRequestHeaders headers)
        {
            var output = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                output[header.Key] = string.Join(",", header.Value);
            }
            return output;
        }

        private string GetCacheKey(object input, Dictionary<string, string> headers)
        {
            var cacheIgnoreHeader = headers.ContainsKey(CacheIgnoreHeader) ? headers[CacheIgnoreHeader] : null;
            var cacheIgnoreHeaderKeys = cacheIgnoreHeader?.Split(',').Select(v => v.ToLower()) ?? new string[0];
            var cacheHeaders = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                var headerKey = header.Key.ToLower();
                if (headerKey == CacheIgnoreHeader || cacheIgnoreHeaderKeys.Contains(headerKey)) continue;
                cacheHeaders.Add(header.Key, header.Value);
            }
            return $"{JsonConvert.SerializeObject(input)}.{JsonConvert.SerializeObject(cacheHeaders)}";
        }

        protected Func<GraphOutput, DateTime> GetCacheUntilWrapper<T>(Func<T, DateTime> cacheUntil, GraphQuery<T> query)
        {
            Func<GraphOutput, DateTime> cacheUntilWrapper = null;
            if (cacheUntil != null) cacheUntilWrapper = o => cacheUntil(query.Data);
            return cacheUntilWrapper;
        }
        protected string GetInputString<TInput>(TInput input)
        {
            var serializer = new JsonSerializer()
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            serializer.Converters.Add(new GraphEnumConverter());
            var stringWriter = new StringWriter();
            using (var writer = new JsonTextWriter(stringWriter))
            {
                writer.QuoteName = false;
                serializer.Serialize(writer, input);
            }
            var serializedObject = stringWriter.ToString();
            return serializedObject.Substring(1, serializedObject.Length - 2);
        }
    }

    public class GraphClient<TInterface> : GraphClient where TInterface : class
    {
        public GraphClient(string apiUrl, HttpClient httpClient, Dictionary<string, string> headers = null, ICacheManager<object> cacheManager = null) : base(apiUrl, httpClient, headers, cacheManager)
        {
        }

        public TOutput RunMutation<TInput, TOutput>(Expression<Func<TInterface, Func<TInput, TOutput>>> expression, TInput input, Func<TOutput, DateTime> cacheUntil = null)
            where TOutput : class, new()
        {
            var operation = MakeOperation(expression.Body, input);
            var graphOutput = RunMutation(operation, new TOutput(), cacheUntil: cacheUntil);
            return HandleOutput(graphOutput);
        }

        public TOutput RunMutation<TInput, TOutput>(Expression<Func<TInterface, Func<TInput, object>>> expression, TInput input, TOutput output, Func<TOutput, DateTime> cacheUntil = null)
        {
            var operation = MakeOperation(expression.Body, input);
            var graphOutput = RunMutation(operation, output, cacheUntil: cacheUntil);
            return HandleOutput(graphOutput);
        }

        public TOutput RunQuery<TInput, TOutput>(Expression<Func<TInterface, Func<TInput, TOutput>>> expression, TInput input, Func<TOutput, DateTime> cacheUntil = null)
             where TOutput : class, new()
        {
            var operation = MakeOperation(expression.Body, input);
            var graphOutput = RunQuery(operation, new TOutput(), cacheUntil: cacheUntil);
            return HandleOutput(graphOutput);
        }

        public TOutput RunQuery<TInput, TOutput>(Expression<Func<TInterface, Func<TInput, object>>> expression, TInput input, TOutput output, Func<TOutput, DateTime> cacheUntil = null)
        {
            var operation = MakeOperation(expression.Body, input);
            var graphOutput = RunQuery(operation, output, cacheUntil: cacheUntil);
            return HandleOutput(graphOutput);
        }

        public TOutput RunSelection<TInput, TOutput>(Expression<Func<TInterface, Func<TInput, object>>> expression, TInput input, IEnumerable<Field> selections, QueryType queryType, string name = "q", object variables = null, bool throwErrors = true) where TInput : class
        {
            var operation = MakeOperation(expression.Body, input);
            var query = AddSelectionQuery<TInput>(operation, null, selections);
            var output = queryType == QueryType.Query ? RunQueriesAsync(name, variables).Result : RunMutationsAsync(name, variables).Result;
            if (throwErrors)
            {
                output.ThrowErrors();
            }
            return query.Data != null ? query.Data.ToObject<TOutput>() : default(TOutput);
        }

        private string MakeOperation<TInput>(Expression expression, TInput input)
        {
            var unaryExpression = (UnaryExpression)expression;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodInfoExpression = (ConstantExpression)methodCallExpression.Object;
            var methodInfo = (MethodInfo)methodInfoExpression.Value;

            var operation = methodInfo.Name.ToCamelCase();

            if (input != null)
            {
                var inputString = GetInputString(input);
                operation = string.IsNullOrEmpty(inputString) ? $"{operation}" : $"{operation}({inputString})";
            }
            return operation;
        }

        private TOutput HandleOutput<TOutput>(GraphOutput<TOutput> graphOutput)
        {
            if (graphOutput.Errors.Any())
            {
                throw new GraphClientException(JsonConvert.SerializeObject(graphOutput.Errors));
            }
            return graphOutput.Data;
        }
    }
}
