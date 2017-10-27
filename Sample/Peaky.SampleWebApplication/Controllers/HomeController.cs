using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Peaky.SampleWebApplication.Controllers
{
    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Index() => Redirect("/tests");
    }
}
