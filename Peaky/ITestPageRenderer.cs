using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Peaky
{
    public interface ITestPageRenderer
    {
        Task Render(HttpContext httpContext);
    }
}
