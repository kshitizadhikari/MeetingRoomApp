using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomApp.DataAccess.DAL;
using RoomApp.Models;

namespace MyRoomApp.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Room> allRooms = await _db.Rooms.ToListAsync();
            return View(allRooms);
        }

        public IActionResult CreateRoom()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom(Room obj)
        {
            if(!ModelState.IsValid)
            {
                TempData["error"] = "Model state invalid";
            }

            _db.Rooms.Add(obj);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
