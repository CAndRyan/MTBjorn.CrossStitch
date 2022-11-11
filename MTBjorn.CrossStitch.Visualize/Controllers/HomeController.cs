using Microsoft.AspNetCore.Mvc;
using MTBjorn.CrossStitch.Business.Helpers;
using MTBjorn.CrossStitch.Visualize.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MTBjorn.CrossStitch.Visualize.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var diagnosticsFilePath = @"D:\Chris\Projects\MTBjorn.CrossStitch\MTBjorn.CrossStitch.Business.Test\Resources\contrast-test\contrast-test-image-3colors-reduction-diagnostics.json"; // @"D:\Chris\Downloads\cross-stitch-test-diagnostics_1.json"; // @"D:\Chris\Downloads\cross-stitch-test-diagnostics.json";
            var diagnostics = System.IO.File.ReadAllText(diagnosticsFilePath);
            //RebalanceHistory = JsonConvert.DeserializeObject<RebalanceHistory>(Diagnostics);

            return View("index", diagnostics);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}