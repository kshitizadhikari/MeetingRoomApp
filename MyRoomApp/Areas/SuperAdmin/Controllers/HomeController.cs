using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

            foreach(var user in users)
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
    }
}
