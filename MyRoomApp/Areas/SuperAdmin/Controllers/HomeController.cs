using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomApp.DataAccess.Infrastructure.Interfaces;
using RoomApp.Models;
using RoomApp.Utility.ViewModels;

namespace MyRoomApp.Areas.SuperAdmin.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    [Area("SuperAdmin")]
    [Route("SuperAdmin/[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly IRepositoryWrapper _repository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(IRepositoryWrapper repository, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _repository = repository;
            _userManager = userManager;
            this._roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<ApplicationUser> users = await _userManager.Users.ToListAsync();
            List<UserRoleVM> userWithRoles = new List<UserRoleVM>();

            foreach (var user in users)
            {
                var role = await _userManager.GetRolesAsync(user);
                string roleString = string.Join(", ", role);

                userWithRoles.Add(new UserRoleVM
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = roleString
                });

            }
            return View(userWithRoles);
        }

        public async Task<IActionResult> EditUser(string? id)
        {
            if (id == null)
            {
                TempData["error"] = "Invalid Id";
                return RedirectToAction("Index");
            }

            //var user = await _db.Users.FindAsync(id);
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["error"] = "User doesn't exist.";
                return RedirectToAction("Index");
            }

            //ViewBag.Roles = await _db.Roles.ToListAsync();
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            var role = await _userManager.GetRolesAsync(user);
            string roleString = string.Join(", ", role);

            UserRoleVM userWithRole = new UserRoleVM
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = roleString
            };

            return View(userWithRole);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserRole(UserRoleVM obj)
        {
            if (obj == null)
            {
                TempData["error"] = "Invalid object passed";
                return RedirectToAction("Index");
            }

            var user = await _repository.AppUsers.FindById(obj.UserId);

            if (user == null)
            {
                TempData["error"] = "No such user exists.";
                return RedirectToAction("Index");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in currentRoles)
            {
                var result = await _userManager.RemoveFromRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    TempData["error"] = "Couldn't remove the user's roles.";
                    return RedirectToAction("Index");

                }
            }

            string newRole = obj?.Role?.ToString() ?? "BasicUser";
            var addToRoleResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addToRoleResult.Succeeded)
            {
                TempData["error"] = "Couldn't add the user's roles.";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteUser(string? id)
        {
            if (id == null)
            {
                TempData["error"] = "Invalid Id";
                return RedirectToAction("Index");
            }

            var user = await _repository.AppUsers.FindById(id);
            if (user == null)
            {
                TempData["error"] = "User doesn't exist.";
                return RedirectToAction("Index");
            }
            bool userBookingExists = _repository.Booking.FindByCondition(b => b.UserId == id).Any();
            if(userBookingExists == true)
            {
                TempData["error"] = "User Has Bookings";
                return RedirectToAction("Index");
            }
            _repository.AppUsers.Delete(user);
            await _repository.Save();
            return RedirectToAction("Index");
        }
    }

}
