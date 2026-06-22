using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartComply.Controllers
{
    public class AuditorController : Controller
    {
        [Authorize(Roles = "Auditor")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
