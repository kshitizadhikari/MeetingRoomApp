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
        protected KeyValuePair<string, string> result;

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
            result = await _roomController.CreateRoom(obj);

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
            result = await _roomController.EditRoom(obj);
            TempData[result.Key] = result.Value;
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> DeleteRoom(int? id)
        {
            result = await _roomController.DeleteRoom(id);
            TempData[result.Key] = result.Value;
            return RedirectToAction("Index");
        }
    }
}
