using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomApp.DataAccess.DAL;
using RoomApp.Models;
using RoomApp.Utility.ViewModels;

namespace MyRoomApp.Areas.BasicUser.Controllers
{
    [Authorize(Roles = "BasicUser")]
    [Area("BasicUser")]
    [Route("BasicUser/[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ViewRooms()
        {
            IEnumerable<Room> allRooms = await _db.Rooms.ToListAsync();
            return View(allRooms);
        }

        public async Task<IActionResult> BookRoom(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Invalid Id.";
                return RedirectToAction("Index");
            }

            Room? roomObj = await _db.Rooms.FindAsync(id);
            if (roomObj == null)
            {
                TempData["error"] = "Room Not Found.";
                return RedirectToAction("Index");
            }

            RoomBookingVM roomBookObj = new RoomBookingVM
            {
                Room = roomObj,
                BookingId = 0,
                BookingName = "",
                StartTime = null,
                EndTime = null
            };

            return View(roomBookObj);
        }
    }
}
