using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomApp.DataAccess.Infrastructure.Interfaces;
using RoomApp.Models;
using RoomApp.Utility.ViewModels;
using System.Security.Claims;

namespace MyRoomApp.Controllers
{
    public class RoomController : Controller
    {
        private readonly IRepositoryWrapper _repository;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoomController(IRepositoryWrapper repository, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
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
        public async Task<KeyValuePair<string, string>> EditRoomPost(Room obj)
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

        public async Task<KeyValuePair<string, string>> BookRoom(RoomBookingParticipantVM roomBook, string currentUserIdString)
        {
            if (roomBook == null)
            {
                return new KeyValuePair<string, string>("error", "Room Booking Unsuccessfull");
            }

            if (roomBook.StartTime < DateTime.Now)
            {

                return new KeyValuePair<string, string>("error", "Starting Time Cannot Be Before Current Time");
            }

            if (roomBook.EndTime < roomBook.StartTime)
            {
                return new KeyValuePair<string, string>("error", "Ending Time Cannot Be Before Starting Time");
            }

            bool isBookingNameUnique = _repository.Booking.FindByCondition(b => b.Name == roomBook.BookingName).FirstOrDefault() == null;
            if (!isBookingNameUnique)
            {
                return new KeyValuePair<string, string>("error", "Duplicate Booking Name. Choose a Different Name");
            }

            
            ApplicationUser? userObj = await _userManager.FindByIdAsync(currentUserIdString);

            List<Booking> allBookings = await _repository.Booking.FindAll().ToListAsync();
            foreach (var booking in allBookings)
            {
                if (roomBook.StartTime >= booking.StartTime && roomBook.StartTime < booking.EndTime)
                {
                    return new KeyValuePair<string, string>("error", "Starting Time Conflict. Choose a Different Starting Time");
                }
                else if (roomBook.EndTime > booking.StartTime && roomBook.EndTime <= booking.EndTime)
                {
                    return new KeyValuePair<string, string>("error", "Ending Time Conflict. Choose a Different Ending Time");
                }
            }

            Booking? bookingObj = new Booking
            {
                Name = roomBook.BookingName,
                RoomId = roomBook.Room?.Id ?? 0,
                StartTime = roomBook.StartTime,
                EndTime = roomBook.EndTime,
                UserId = currentUserIdString,
                User = userObj
            };

            if (!ModelState.IsValid)
            {
                return new KeyValuePair<string, string>("error", "Room Booking Unsuccessful");
            }
            _repository.Booking.Create(bookingObj);
            await _repository.Save();
            return new KeyValuePair<string, string>("success", "Room Booked Successfully");

        }

    }
}