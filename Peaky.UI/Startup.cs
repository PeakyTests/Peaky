using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Peaky.UI.Startup))]
namespace Peaky.UI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
