using Microsoft.AspNetCore.Mvc;

namespace Portal.Controllers
{
    public class StatisticsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
