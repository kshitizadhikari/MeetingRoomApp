using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRoomApp.Controllers;
using RoomApp.DataAccess.DAL;
using RoomApp.DataAccess.Infrastructure.Interfaces;
using RoomApp.Models;

namespace MyRoomApp.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly IRepositoryWrapper _repository;
        private readonly RoomController _roomController;

        public HomeController(IRepositoryWrapper repository, RoomController roomController)
        {
            _repository = repository;
           _roomController = roomController;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Room> allRooms = await _repository.Room.FindAll().ToListAsync();
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
            KeyValuePair<string, string> result = await _roomController.CreateRoom(obj);

            TempData[result.Key] = result.Value;

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> EditRoom(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Invalid Room Id";
                return RedirectToAction("Index");
            }

            Room? roomObj = await _repository.Room.FindById(id);
            if (roomObj == null)
            {
                TempData["error"] = "Room Not Found.";
                return RedirectToAction("Index");
            }
            return View(roomObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(Room obj)
        {
            Room? roomObj = await _repository.Room.FindById(obj.Id);
            if (roomObj == null)
            {
                TempData["error"] = "Room Not Found.";
                return RedirectToAction("Index");
            }

            roomObj.Id = obj.Id;
            roomObj.Name = obj.Name;
            roomObj.Size = obj.Size;
            roomObj.Status = obj.Status;
            _repository.Room.Update(roomObj);
            await _repository.Save();
            TempData["success"] = "Room details updated successfully.";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> DeleteRoom(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Invalid Room Id";
                return RedirectToAction("Index");
            }

            Room? roomObj = await _repository.Room.FindById(id);
            if (roomObj == null)
            {
                TempData["error"] = "Room Not Found.";
                return RedirectToAction("Index");
            }

            List<Booking> bookingsInRoom = await _repository.Booking.FindByCondition(b => b.RoomId == roomObj.Id).ToListAsync();

            if (bookingsInRoom.Count > 0)
            {
                TempData["error"] = "Cannot delete room because of associated bookings.";
                return RedirectToAction("Index");
            }
            _repository.Room.Delete(roomObj);
            await _repository.Save();
            TempData["success"] = "Room deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
