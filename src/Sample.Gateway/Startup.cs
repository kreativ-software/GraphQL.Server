using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sample.Gateway.Controllers;
using Sample.Gateway.Config;
using GraphQL.Server;
using GraphQL.Client.Transport;
using Sample.Service.Interface;
using System.Reflection;

namespace Sample.Gateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddMvc(options =>
            {
                options.OutputFormatters.Add(new HtmlOutputFormatter());
            });
            services.AddOptions();
            services.Configure<ServiceConfig>(Configuration.GetSection("ServiceConfig"));
            services.AddSingleton<Engine>(provider =>
            {
                var serviceConfig = provider.GetService<IOptions<ServiceConfig>>().Value;
                return Engine.New()
                    .AddProxy<ISampleSchema>(new HttpTransport(serviceConfig.ServiceUrls.SampleService))
                    .AddTypes(Assembly.GetExecutingAssembly())
                    .Compile();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
