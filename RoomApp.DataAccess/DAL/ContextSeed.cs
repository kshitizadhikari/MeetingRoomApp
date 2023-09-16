using Microsoft.AspNetCore.Identity;
using RoomApp.Models;
using RoomApp.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomApp.DataAccess.DAL
{
    public static class ContextSeed
    {
        public static async Task SeedRolesAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole(Role.SuperAdmin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Role.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Role.BasicUser.ToString()));
        }

        public static async Task SeedDefaultAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var defaultUser = new ApplicationUser
            {
                FirstName = "Arc",
                LastName = "Warden",
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                EmailConfirmed = false,
                PhoneNumberConfirmed = false
            };

            if(userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Password@123");
                    await userManager.AddToRoleAsync(defaultUser, Role.Admin.ToString());
                }
            }
        }

        public static async Task SeedDefaultSuperAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var defaultUser = new ApplicationUser
            {
                FirstName = "Super",
                LastName = "BasicUser",
                UserName = "superadmin@gmail.com",
                Email = "superadmin@gmail.com",
                EmailConfirmed = false,
                PhoneNumberConfirmed = false
            };

            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Password@123");
                    await userManager.AddToRoleAsync(defaultUser, Role.SuperAdmin.ToString());
                }
            }
        }

    }

}
