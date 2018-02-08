using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Peaky.SampleWebApplication
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

            services.AddPeakyTests(
                targets =>
                {
                    targets.Add("production",
                                "bing",
                                new Uri("https://bing.com"))
                           .Add("test",
                                "bing",
                                new Uri("https://bing.com"))
                           .Add("production",
                                "microsoft",
                                new Uri("https://microsoft.com"));
                });

            services.AddPeakySensors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseMvc();
            app.UsePeaky();
        }
    }
}
