using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace Peaky
{
    internal class TestPageRouter : PeakyRouter
    {
        private readonly ITestPageRenderer testPageRenderer;

        public TestPageRouter(
            ITestPageRenderer testPageRenderer,
            string pathBase = "/tests") : base(pathBase)
        {
            this.testPageRenderer = testPageRenderer ?? throw new ArgumentNullException(nameof(testPageRenderer));
        }

        public override async Task RouteAsync(RouteContext context)
        {
            if (RouteMatches(context))
            {
                context.Handler = async httpContext =>
                {
                    await testPageRenderer.Render(context.HttpContext);
                };
            }
        }
    }
}
