using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kontructo.Data;
using Kontructo.Logic;
using Kontructo.Models;

namespace Kontructo.Data
{
    public class DBInitializer : IDBInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DBInitializer(ApplicationDbContext db, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        public async void Initialize()    
        {
            //Exit if role already exists
            if (_db.Roles.Any(r => r.Name == Roles.Master)) return;

            if (_db.Roles.Any(r => r.Name == Roles.Admin)) return;

            if (_db.Roles.Any(r => r.Name == Roles.SuperAdmin)) return;

            if (_db.Roles.Any(r => r.Name == Roles.Member)) return;

            //Create Admin role
            _roleManager.CreateAsync(new IdentityRole(Roles.Master)).GetAwaiter().GetResult();

            _roleManager.CreateAsync(new IdentityRole(Roles.Admin)).GetAwaiter().GetResult();

            _roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin)).GetAwaiter().GetResult();

            _roleManager.CreateAsync(new IdentityRole(Roles.Member)).GetAwaiter().GetResult();


            //Create Admin user
            _userManager.CreateAsync(new AppUser
            {
                UserName = "master",
                Email = "master@mail.com",
                EmailConfirmed = true,
                Name = "Web Master",
            },"P@ssword1").GetAwaiter().GetResult();

            //Assign role to Admin user
            await _userManager.AddToRoleAsync(await _userManager.FindByNameAsync("master"),Roles.Master);
                       

        }
    }
}
