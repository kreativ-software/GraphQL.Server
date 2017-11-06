using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HandlebarsDotNet;
using System.IO;
using GraphQL.Server;
using GraphQL;

namespace Sample.Service.Controllers
{
    [Route("")]
    [Produces("text/html")]
    public class HomeController : Controller
    {
        private static Func<object, string> _indexTemplate;
        public static Func<object, string> IndexTemplate
        {
            get
            {
                if (_indexTemplate == null)
                {
                    _indexTemplate = Handlebars.Compile(System.IO.File.ReadAllText("wwwroot/Index.html"));
                }
                return _indexTemplate;
            }
        }

        private Schema _schema;

        public HomeController(Schema schema)
        {
            _schema = schema;
        }

        [HttpGet("")]
        public string Index()
        {
            var model = new
            {
                Version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version
            };
            return IndexTemplate(model);
        }

        [HttpPost("")]
        public async Task<ExecutionResult> PostAsync()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = reader.ReadToEnd();
            }

            var result = await _schema
                .WithQuery(body)
                .Execute();
            return result;
        }
    }
}
