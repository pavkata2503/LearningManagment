using AppData.Enums;
using AppData.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext
{
    public class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            var userManager = service.GetService<UserManager<ApplicationUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Teacher.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Student.ToString()));


            var user = new ApplicationUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                Name = "Павката",
                FirstName = "Павел",
                LastName = "Петров",
                PhoneNumber = "0884390393",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var user1 = new ApplicationUser
            {
                UserName = "vladislav@gmail.com",
                Email = "vladislav@gmail.com",
                Name = "Владо",
                FirstName = "Владислав",
                LastName = "Христов",
                PhoneNumber = "0884390393",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var userInDb = await userManager.FindByEmailAsync(user.Email);
            if (userInDb == null)
            {
                await userManager.CreateAsync(user, "Admin@123");
                await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
            }
            var userInDb1 = await userManager.FindByEmailAsync(user1.Email);
            if (userInDb1 == null)
            {
                await userManager.CreateAsync(user1, "Vladislav123!");
                await userManager.AddToRoleAsync(user1, Roles.Admin.ToString());
            }
        }
    }
}
