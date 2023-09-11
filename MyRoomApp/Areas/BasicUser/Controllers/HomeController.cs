using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomApp.DataAccess.DAL;
using RoomApp.Models;
using RoomApp.Utility.ViewModels;
using System.Security.Claims;

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

        public async Task<IActionResult> Index()
        {
            var currentUser = HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            string UserId = currentUser?.ToString() ?? "DefaultUserId";
            ViewBag.UserId = UserId;
            List<RoomBookingVM> vmList = new List<RoomBookingVM>();
            List<Booking> allBookings = _db.Bookings.ToList();
            foreach (Booking booking in allBookings)
            {
                Room? room = await _db.Rooms.FindAsync(booking.RoomId);
                var bookUser = await _db.Users.FindAsync(booking.UserId);
                string? bookUserId = bookUser.Id;

                vmList.Add(new RoomBookingVM
                {
                    UserId = bookUserId,
                    Room = room,
                    BookingId = booking.Id,
                    BookingName = booking.Name,
                    StartTime = booking.StartTime,
                    EndTime = booking.EndTime,
                });
            }
            return View(vmList);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookRoom(RoomBookingVM? roomBook)
        {
            if (roomBook == null)
            {
                TempData["error"] = "Room Booking Unsuccessful.";
            }

            var currentUserId = HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            string currentUserIdString = currentUserId?.ToString() ?? "DefaultUserId";
            ApplicationUser? userObj = await _db.Users.FindAsync(currentUserIdString);

            List<Booking> allBookings = _db.Bookings.ToList();
            foreach (var booking in allBookings)
            {
                if (booking.UserId == currentUserIdString)
                {
                    if (roomBook.StartTime >= booking.StartTime && roomBook.StartTime < booking.EndTime)
                    {
                        TempData["error"] = "Starting Time Conflict. Choose a different starting time.";
                        return RedirectToAction("BookRoom", new { id = roomBook.Room.Id });
                    }
                    else if (roomBook.EndTime > booking.StartTime && roomBook.EndTime <= booking.EndTime)
                    {
                        TempData["error"] = "End Time Conflict. Choose a different ending time.";
                        return RedirectToAction("BookRoom", new { id = roomBook.Room.Id });
                    }
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

            _db.Bookings.Add(bookingObj);
            await _db.SaveChangesAsync();
            TempData["success"] = "Room Booked Successfully";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditBooking(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Invalid Id.";
                return RedirectToAction("Index");
            }

            Booking? bookObj = await _db.Bookings.FindAsync(id);
            if (bookObj == null)
            {
                TempData["error"] = "Booking Not Found.";
                return RedirectToAction("Index");
            }
            Room? roomObj = await _db.Rooms.FindAsync(bookObj.RoomId);
            if (roomObj == null)
            {
                TempData["error"] = "Room Not Found.";
                return RedirectToAction("Index");
            }

            RoomBookingVM roomBookObj = new RoomBookingVM
            {
                Room = roomObj,
                BookingId = bookObj.Id,
                BookingName = bookObj.Name,
                StartTime = bookObj.StartTime,
                EndTime = bookObj.EndTime,
            };
            return View(roomBookObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBooking(RoomBookingVM roomBook)
        {
            if (roomBook == null)
            {
                TempData["error"] = "Invalid object.";
                return RedirectToAction("Index");
            }

            var currentUser = HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            string currentUserId = currentUser?.ToString() ?? "DefaultUserId";
            ApplicationUser? userObj = await _db.Users.FindAsync(currentUserId);

            if (userObj == null)
            {
                TempData["error"] = "User Not Found";
                return RedirectToAction("Index");
            }
            Booking? bookingObj = await _db.Bookings.FindAsync(roomBook.BookingId);

            if (bookingObj == null)
            {
                TempData["error"] = "Booking Not Found.";
                return RedirectToAction("Index");
            }

            bookingObj.Name = roomBook.BookingName;
            bookingObj.RoomId = roomBook.Room?.Id ?? 0;
            bookingObj.StartTime = roomBook.StartTime;
            bookingObj.EndTime = roomBook.EndTime;
            bookingObj.UserId = currentUserId;
            bookingObj.User = userObj;

            _db.Bookings.Update(bookingObj);
            await _db.SaveChangesAsync();
            TempData["success"] = "Updated Booking Successfully.";
            return RedirectToAction("Index");
        }


    }
}
