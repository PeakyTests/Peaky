using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Peaky
{
    internal class TestPageRouter : PeakyRouter
    {
        private readonly string html;

        public TestPageRouter(
            ITestPageFormatter testPageFormatter,
            string pathBase = "/tests") : base(pathBase)
        {
            if (testPageFormatter == null)
            {
                throw new ArgumentNullException(nameof(testPageFormatter));
            }

            html = testPageFormatter.Render();
        }

        public override async Task RouteAsync(RouteContext context)
        {
            if (RouteMatches(context))
            {
                context.Handler = async httpContext =>
                {
                    await context.HttpContext.Response.WriteAsync(html);
                };
            }
        }
    }
}
