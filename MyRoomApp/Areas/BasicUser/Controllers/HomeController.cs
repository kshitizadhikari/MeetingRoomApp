using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyRoomApp.Areas.BasicUser.Controllers
{
    [Authorize(Roles = "BasicUser")]
    [Area("BasicUser")]
    [Route("BasicUser/[controller]/[action]")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
