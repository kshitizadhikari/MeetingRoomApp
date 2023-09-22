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

        public async Task<KeyValuePair<string, string>> EditRoom(Room obj)
        {
            Room? roomObj = await _repository.Room.FindById(obj.Id);
            if (roomObj == null)
            {
                return new KeyValuePair<string, string>("error", "Room Not Found");

            }

            roomObj.Id = obj.Id;
            roomObj.Name = obj.Name;
            roomObj.Size = obj.Size;
            roomObj.Status = obj.Status;
            _repository.Room.Update(roomObj);
            await _repository.Save();
            return new KeyValuePair<string, string>("success", "Room Updated Successfully");
        }

        public async Task<KeyValuePair<string, string>> DeleteRoom(int? id)
        {
            if (id == null || id == 0)
            {
                return new KeyValuePair<string, string>("error", "Invalid Room Id");
            }

            Room? roomObj = await _repository.Room.FindById(id);
            if (roomObj == null)
            {
                return new KeyValuePair<string, string>("error", "Room Not Found");
            }

            List<Booking> bookingsInRoom = await _repository.Booking.FindByCondition(b => b.RoomId == roomObj.Id).ToListAsync();

            if (bookingsInRoom.Count > 0)
            {
                return new KeyValuePair<string, string>("error", "Cannot Delete Room That Has Bookings");
            }
            _repository.Room.Delete(roomObj);
            await _repository.Save();
            return new KeyValuePair<string, string>("success", "Room Deleted Successfully");

        }

    }
}
