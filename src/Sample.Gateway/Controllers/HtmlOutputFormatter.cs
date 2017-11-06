using Microsoft.AspNetCore.Mvc.Formatters;

namespace Sample.Gateway.Controllers
{
    public class HtmlOutputFormatter : StringOutputFormatter
    {
        public HtmlOutputFormatter()
        {
            SupportedMediaTypes.Add("text/html");
        }
    }
}
