using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomApp.DataAccess.Infrastructure.Interfaces;
using RoomApp.Models;

namespace MyRoomApp.Controllers
{
    public class RoomController : Controller
    {
        private readonly IRepositoryWrapper _repository;

        public RoomController(IRepositoryWrapper repository)
        {
            _repository = repository;
        }

        public async Task<KeyValuePair<string, string>> CreateRoom(Room obj)
        {
            if (!ModelState.IsValid)
            {
                return new KeyValuePair<string, string>("error", "Model State Invalid");
            }

            bool isRoomNameUnique = !await _repository.Room
                .FindByCondition(r => r.Name == obj.Name)
                .AnyAsync();

            if (!isRoomNameUnique)
            {
                return new KeyValuePair<string, string>("error", "Duplicate Room Name");
            }

            _repository.Room.Create(obj);
            await _repository.Save();

            return new KeyValuePair<string, string>("success", "Room created successfully");
        }

    }
}
