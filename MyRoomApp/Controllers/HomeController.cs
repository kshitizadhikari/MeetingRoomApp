using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RoomApp.DataAccess.DAL;
using RoomApp.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace AuthSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;



        public HomeController(ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            this._signInManager = signInManager;
            this._db = db;
            this._userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            string currentUserId = currentUser?.ToString() ?? "DefaultUserId";
            var user = await _db.Users.FindAsync(currentUserId);
            if (await _userManager.IsInRoleAsync(user, "superadmin"))
            {
                return RedirectToAction("Index", "Home", new { area = "SuperAdmin" });
            }
            if (await _userManager.IsInRoleAsync(user, "admin"))
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            if (await _userManager.IsInRoleAsync(user, "basicuser"))
            {
                return RedirectToAction("Index", "Home", new { area = "BasicUser" });
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


