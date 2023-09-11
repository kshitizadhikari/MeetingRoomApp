using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomApp.DataAccess.DAL;
using RoomApp.Models;
using RoomApp.Models.Enum;
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
            List<RoomBookingParticipantVM> vmList = new List<RoomBookingParticipantVM>();
            List<Booking> allBookings = _db.Bookings.ToList();
            foreach (Booking booking in allBookings)
            {
                Room? room = await _db.Rooms.FindAsync(booking.RoomId);
                var bookUser = await _db.Users.FindAsync(booking.UserId);
                string? bookUserId = bookUser.Id;
                List<Participant> particpantList = await _db.Participants.ToListAsync();
                List<Participant> bookParticipantList = new List<Participant>();

                foreach (var par in particpantList)
                {
                    if (par.UserId == UserId && par.BookingId == booking.Id)
                    {
                        bookParticipantList.Add(par);
                    }
                }

                vmList.Add(new RoomBookingParticipantVM
                {
                    UserId = bookUserId,
                    Room = room,
                    BookingId = booking.Id,
                    BookingName = booking.Name,
                    StartTime = booking.StartTime,
                    EndTime = booking.EndTime,
                    Participants = bookParticipantList
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

                RoomBookingParticipantVM roomBookObj = new RoomBookingParticipantVM
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
        public async Task<IActionResult> BookRoom(RoomBookingParticipantVM? roomBook)
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
                TempData["error"] = "No such booking object found in the database.";
                return RedirectToAction("Index");
            }
            Room? roomObj = await _db.Rooms.FindAsync(bookObj.RoomId);
            if (roomObj == null)
            {
                TempData["error"] = "No such Room object found in the database.";
                return RedirectToAction("Index");
            }
            List<Participant>? allParticipants = await _db.Participants.ToListAsync();
            List<Participant> bookingParticipantList = new List<Participant>();
            foreach (var item in allParticipants)
            {
                if (item.BookingId == bookObj.Id)
                {
                    bookingParticipantList.Add(item);
                }
            }
            RoomBookingParticipantVM roomBookObj = new RoomBookingParticipantVM
            {
                Room = roomObj,
                BookingId = bookObj.Id,
                BookingName = bookObj.Name,
                StartTime = bookObj.StartTime,
                EndTime = bookObj.EndTime,
                Participants = bookingParticipantList
            };
            return View(roomBookObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBooking(RoomBookingParticipantVM roomBook)
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

        public async Task<IActionResult> DeleteBooking(int? id)
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

            _db.Bookings.Remove(bookObj);
            await _db.SaveChangesAsync();
            TempData["success"] = "Booking Removed Successfully.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AddParticipantView(RoomBookingParticipantVM roomBookObj)
        {
            if (roomBookObj == null)
            {
                TempData["error"] = "Invalid Object.";
                return RedirectToAction("Index");
            }

            ViewBag.Participant = await _db.Users.ToListAsync();

            BookingParticipantsVM obj = new BookingParticipantsVM
            {
                BookingId = roomBookObj.BookingId,
                UserId = "",
                ParticipantStatus = ParticipantStatus.Pending
            };
            return View("AddParticipant", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddParticipant(BookingParticipantsVM obj)
        {
            if (obj == null)
            {
                TempData["error"] = "Object null";
                return RedirectToAction("Index");
            }

            var user = await _db.Users.FindAsync(obj.UserId);
            if (user == null)
            {
                TempData["error"] = "User Not Found.";
                return RedirectToAction("Index");
            }

            Participant participant = new Participant
            {
                BookingId = obj.BookingId,
                UserId = obj.UserId,
                FirstName = user?.FirstName,
                LastName = user?.LastName,
                Status = obj.ParticipantStatus
            };

            _db.Participants.Add(participant);
            await _db.SaveChangesAsync();
            TempData["success"] = "Participant Added Successfully.";
            return RedirectToAction("EditBooking", new { id = obj.BookingId });
        }

        public async Task<IActionResult> RemoveParticipant(int? id)
        {
            Participant? obj = await _db.Participants.FindAsync(id);
            if (obj == null)
            {
                TempData["error"] = "Participant Object Null";
                return RedirectToAction("Index");
            }
            _db.Participants.Remove(obj);
            await _db.SaveChangesAsync();
            TempData["success"] = "Participant object removed successfully.";
            return RedirectToAction("EditBooking", new { id = obj.BookingId });
        }

        public async Task<IActionResult> ViewMeetingDetails(int? id)
        {
            var currentUser = HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            string UserId = currentUser?.ToString() ?? "DefaultUserId";
            ViewBag.UserId = UserId;


            if (id == null || id == 0)
            {
                TempData["error"] = "Invalid Id.";
                return RedirectToAction("Index");
            }

            Booking? bookObj = await _db.Bookings.FindAsync(id);
            if (bookObj == null)
            {
                TempData["error"] = "Booking object not found.";
                return RedirectToAction("EditBooking", new { id = id });
            }
            Room? roomObj = await _db.Rooms.FindAsync(bookObj.RoomId);
            if (roomObj == null)
            {
                TempData["error"] = "Room object not found.";
                return RedirectToAction("EditBooking", new { id = id });
            }
            List<Participant>? allParticipants = await _db.Participants.ToListAsync();
            List<Participant> bookingParticipantList = new List<Participant>();
            foreach (var item in allParticipants)
            {
                if (item.BookingId == bookObj.Id)
                {
                    bookingParticipantList.Add(item);
                }
            }

            RoomBookingParticipantVM roomBookObj = new RoomBookingParticipantVM
            {
                Room = roomObj,
                BookingId = bookObj.Id,
                BookingName = bookObj.Name,
                StartTime = bookObj.StartTime,
                EndTime = bookObj.EndTime,
                Participants = bookingParticipantList
            };

            return View(roomBookObj);
        }

    }
}
