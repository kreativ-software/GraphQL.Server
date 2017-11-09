using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GraphQL.Client.Transport
{
    public class HttpTransport : ITransport
    {
        public HttpClient HttpClient { get; private set; }
        public string ServiceUrl { get; private set; }

        public HttpTransport(string serviceUrl)
        {
            ServiceUrl = serviceUrl;
            HttpClient = new HttpClient()
            {
                BaseAddress = new System.Uri(serviceUrl)
            };
            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<ITransportOutput> SendAsync(string operation, string variables)
        {
            var query = new
            {
                query = operation,
                variables = variables
            };
            var queryString = new StringContent(JsonConvert.SerializeObject(query));
            var response = await HttpClient.PostAsync("", queryString).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new HttpTransportOutput(content, operation, response.IsSuccessStatusCode, variables);
        }
    }
}