using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyRoomApp.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        public IActionResult Index()
        {
            return View("../");
        }
    }
}
