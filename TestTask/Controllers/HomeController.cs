using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TestTask.Models;

namespace TestTask.Controllers
{
    [Controller]
    [Route("")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public IActionResult Index()
        {
            //var filePath = Path.Combine(_env.WebRootPath, "index.html");
            //if (System.IO.File.Exists(filePath))
            //{
            //    return PhysicalFile(filePath, "text/html");
            //}
            return View("index");
        }
        [Route("/error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
