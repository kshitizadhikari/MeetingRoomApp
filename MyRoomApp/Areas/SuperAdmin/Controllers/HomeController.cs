using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomApp.DataAccess.DAL;
using RoomApp.Models;
using RoomApp.Utility.ViewModels;

namespace MyRoomApp.Areas.SuperAdmin.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    [Area("SuperAdmin")]
    [Route("SuperAdmin/[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<ApplicationUser> users = _db.Users.ToList();
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

            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                TempData["error"] = "User doesn't exist.";
                return RedirectToAction("Index");
            }

            ViewBag.Roles = await _db.Roles.ToListAsync();
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

            var user = await _db.Users.FindAsync(obj.UserId);
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
    }

}
