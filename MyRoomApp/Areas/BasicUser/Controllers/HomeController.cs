using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomApp.DataAccess.DAL;
using RoomApp.DataAccess.Infrastructure.Interfaces;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepositoryWrapper _repository;
        private readonly IEmailSender _emailSender;

        public HomeController(UserManager<ApplicationUser> userManager, IRepositoryWrapper repository, IEmailSender emailSender)
        {
            _userManager = userManager;
            _repository = repository;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            string? UserId = currentUser?.ToString() ?? "NoId";
            ViewBag.UserId = UserId;
            List<RoomBookingParticipantVM> vmList = new List<RoomBookingParticipantVM>();
            List<Booking> allBookings = await _repository.Booking.FindAll().ToListAsync();
            foreach (Booking booking in allBookings)
            {
                Room? room = await _repository.Room.FindById(booking.RoomId);
                var bookUser = await _userManager.FindByIdAsync(booking.UserId);

                string bookUserId = bookUser.Id;
                List<Participant> participantList = await _repository.Participants.FindAll().ToListAsync();
                List<Participant> bookParticipantList = new List<Participant>();

                foreach (var par in participantList)
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
            IEnumerable<Room> allRooms = await _repository.Room.FindAll().ToListAsync();
            return View(allRooms);
        }

        public async Task<IActionResult> BookRoom(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Invalid Id.";
                return RedirectToAction("Index");
            }

            Room? roomObj = await _repository.Room.FindById(id);
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
        public async Task<IActionResult> BookRoom(RoomBookingParticipantVM roomBook)
        {
            if (roomBook == null)
            {
                TempData["error"] = "Room Booking Unsuccessful.";
            }

            if (roomBook.StartTime < DateTime.Now)
            {
                TempData["error"] = "Starting Time cannot be before today's date.";
                return RedirectToAction("BookRoom", new { id = roomBook.Room.Id });
            }

            if (roomBook.EndTime < roomBook.StartTime)
            {
                TempData["error"] = "Ending Time cannot be before starting time.";
                return RedirectToAction("BookRoom", new { id = roomBook.Room.Id });
            }

            bool isBookingNameUnique = _repository.Booking.FindByCondition(b => b.Name == roomBook.BookingName).FirstOrDefault() == null;
            if (!isBookingNameUnique)
            {
                TempData["error"] = "Duplicate Booking Name. Choose a different Name.";
                return RedirectToAction("BookRoom", new { id = roomBook.Room.Id });
            }

            var currentUserId = HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            string currentUserIdString = currentUserId?.ToString() ?? "DefaultUserId";
            //ApplicationUser? userObj = await _db.Users.FindAsync(currentUserIdString);
            ApplicationUser? userObj = await _userManager.FindByIdAsync(currentUserIdString);

            List<Booking> allBookings = await _repository.Booking.FindAll().ToListAsync();
            foreach (var booking in allBookings)
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

            Booking? bookingObj = new Booking
            {
                Name = roomBook.BookingName,
                RoomId = roomBook.Room?.Id ?? 0,
                StartTime = roomBook.StartTime,
                EndTime = roomBook.EndTime,
                UserId = currentUserIdString,
                User = userObj
            };

            if (ModelState.IsValid)
            {
                _repository.Booking.Create(bookingObj);
                await _repository.Save();
                TempData["success"] = "Room Booked Successfully";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditBooking(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Invalid Id.";
                return RedirectToAction("Index");
            }

            Booking? bookObj = await _repository.Booking.FindById(id);
            if (bookObj == null)
            {
                TempData["error"] = "No such booking object found in the database.";
                return RedirectToAction("Index");
            }
            Room? roomObj = await _repository.Room.FindById(bookObj.RoomId);
            if (roomObj == null)
            {
                TempData["error"] = "No such Room object found in the database.";
                return RedirectToAction("Index");
            }
            List<Participant>? allParticipants = await _repository.Participants.FindAll().ToListAsync();
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
            //ApplicationUser? userObj = await _db.Users.FindAsync(currentUserId);
            ApplicationUser? userObj = await _userManager.FindByIdAsync(currentUserId);

            if (userObj == null)
            {
                TempData["error"] = "User Not Found";
                return RedirectToAction("Index");
            }
            Booking? bookingObj = await _repository.Booking.FindById(roomBook.BookingId);

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

            _repository.Booking.Update(bookingObj);
            await _repository.Save();
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

            Booking? bookObj = await _repository.Booking.FindById(id);
            if (bookObj == null)
            {
                TempData["error"] = "Booking Not Found.";
                return RedirectToAction("Index");
            }

            List<Participant> participantsInBooking = await _repository.Participants.FindByCondition(p => p.BookingId == bookObj.Id).ToListAsync();
            _repository.Participants.RemoveMultiple(participantsInBooking);
            _repository.Booking.Delete(bookObj);
            await _repository.Save();
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

            //var usersNotInParticipants = _db.Users
            //    .Where(u => !_db.Participants.Any(p => p.UserId == u.Id && p.BookingId == roomBookObj.BookingId))
            //    .ToList();


            var participantUserIds = _repository.Participants
            .FindByCondition(p => p.BookingId == roomBookObj.BookingId)
            .Select(p => p.UserId)
            .ToList();

            var usersNotInParticipants = _userManager.Users.Where(u => !participantUserIds.Contains(u.Id)).ToList();
            ViewBag.Participant = usersNotInParticipants;

            roomBookObj.UserStatus = ParticipantStatus.Pending;
            return View("AddParticipant", roomBookObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddParticipant(RoomBookingParticipantVM obj)
        {
            if (obj == null)
            {
                TempData["error"] = "Object null";
                return RedirectToAction("Index");
            }

            //var user = await _db.Users.FindAsync(obj.UserId);
            var user = await _userManager.FindByIdAsync(obj.UserId);
            if (user == null)
            {
                TempData["error"] = "User Not Found.";
                return RedirectToAction("Index");
            }

            int? roomSize = obj.Room.Size;
            int currentBookingParticipantCount = _repository.Participants.FindByCondition(p => p.BookingId == obj.BookingId).Count();

            if (currentBookingParticipantCount == (roomSize - 1))
            {
                TempData["error"] = "Room Size Full. Cannot add more Participants.";
                return RedirectToAction("EditBooking", new { id = obj.BookingId });
            }
            Participant participant = new Participant
            {
                BookingId = obj.BookingId,
                UserId = obj.UserId,
                FirstName = user?.FirstName,
                LastName = user?.LastName,
                Status = ParticipantStatus.Pending
            };

            _repository.Participants.Create(participant);
            await _repository.Save();

            var receiver = user.Email.ToString();
            var subject = "Meeting Invitation";
            var message = $"You have been invited to a meeting:\n Room:{obj.Room.Name}\n Start Time: {obj.StartTime}\n End Time: {obj.EndTime}";
            await _emailSender.SendEmailAsync(receiver, subject, message);
            TempData["success"] = "Participant Added Successfully.";
            return RedirectToAction("EditBooking", new { id = obj.BookingId });
        }

        public async Task<IActionResult> RemoveParticipant(int? id)
        {
            Participant? obj = await _repository.Participants.FindById(id);
            if (obj == null)
            {
                TempData["error"] = "Participant Object Null";
                return RedirectToAction("Index");
            }
            _repository.Participants.Delete(obj);
            await _repository.Save();
            TempData["success"] = "Participant Removed Successfully.";
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

            Booking? bookObj = await _repository.Booking.FindById(id);
            if (bookObj == null)
            {
                TempData["error"] = "Booking Not Found.";
                return RedirectToAction("EditBooking", new { id = id });
            }
            Room? roomObj = await _repository.Room.FindById(bookObj.RoomId);
            if (roomObj == null)
            {
                TempData["error"] = "Room Not Found.";
                return RedirectToAction("EditBooking", new { id = id });
            }
            List<Participant>? allParticipants = await _repository.Participants.FindAll().ToListAsync();
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

        public async Task<IActionResult> UpdateMeetingStatus(RoomBookingParticipantVM obj)
        {
            Participant? participant = await _repository.Participants.FindByCondition(x => x.UserId == obj.UserId && x.BookingId == obj.BookingId).FirstOrDefaultAsync(); ;
            string? userStatus = obj.UserStatus.ToString();
            ParticipantStatus enumValue = (ParticipantStatus)Enum.Parse(typeof(ParticipantStatus), userStatus);
            participant.Status = enumValue;
            _repository.Participants.Update(participant);
            await _repository.Save();
            TempData["sucess"] = "Participant Status Updated Successfully.";
            return RedirectToAction("Index");
        }

    }
}
