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
            TempData["success"] = "Room created successfully.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditRoom(int? id)
        {
            if(id == null || id == 0)
            {
                TempData["error"] = "Invalid Room Id";
                return RedirectToAction("Index");
            }

            Room? roomObj = await _db.Rooms.FindAsync(id);
            if(roomObj == null)
            {
                TempData["error"] = "No such room exists in the database.";
                return RedirectToAction("Index");
            }
            return View(roomObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(Room obj)
        {
            Room? roomObj = await _db.Rooms.FindAsync(obj.Id);
            if(roomObj == null)
            {
                TempData["error"] = "No such room exists in the database.";
                return RedirectToAction("Index");
            }

            roomObj.Id = obj.Id;
            roomObj.Name = obj.Name;
            roomObj.Size = obj.Size;
            roomObj.Status = obj.Status;
            _db.Update(roomObj);
            await _db.SaveChangesAsync();
            TempData["success"] = "Room details updated successfully.";
            return RedirectToAction("Index");
        }
    }
}
