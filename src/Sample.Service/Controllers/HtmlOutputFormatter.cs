using Microsoft.AspNetCore.Mvc.Formatters;

namespace Sample.Service.Controllers
{
    public class HtmlOutputFormatter : StringOutputFormatter
    {
        public HtmlOutputFormatter()
        {
            SupportedMediaTypes.Add("text/html");
        }
    }
}
