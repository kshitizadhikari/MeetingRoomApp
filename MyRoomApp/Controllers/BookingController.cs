using Microsoft.AspNetCore.Mvc;

namespace MyRoomApp.Controllers
{
    public class BookingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
