using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HandlebarsDotNet;

namespace Sample.Gateway.Controllers
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

        //private GraphQLEngine _engine;

        //public HomeController(GraphQLEngine engine)
        //{
        //    _engine = engine;
        //}

        [HttpGet("")]
        public string Index()
        {
            var model = new
            {
                Version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version
            };
            return IndexTemplate(model);
        }

        //[HttpPost("")]
        //public async Task<object> PostAsync()
        //{
        //    string body;
        //    using (var reader = new StreamReader(Request.Body))
        //    {
        //        body = reader.ReadToEnd();
        //    }

        //    var result = await _engine
        //        .NewExecutor()
        //        .WithRequest(body)
        //        .Execute();
        //    return result;
        //}
    }
}
