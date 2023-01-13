using EllipticCurve;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using Spice.Models;
using Spice.Utility;
using System.Security.Cryptography;

namespace Spice.Data
{
	public class DbInitializer : IDbInisializer
	{

		private readonly ApplicationDbContext _db;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;


		public DbInitializer(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_db = db;
			_userManager = userManager;
			_roleManager = roleManager;
		}
		public async Task<bool> Initialize()
		{
			try
			{
				if (_db.Database.GetPendingMigrations().Count() > 0)
				{
					_db.Database.Migrate();
				}
			}
			catch (Exception ex)
			{

			}

			if (_db.Roles.Any(r => r.Name == SD.ManagerUser))
			{
				return false;
			}
			_roleManager.CreateAsync(new IdentityRole(SD.ManagerUser)).GetAwaiter().GetResult();
			_roleManager.CreateAsync(new IdentityRole(SD.FrontDeskUser)).GetAwaiter().GetResult();
			_roleManager.CreateAsync(new IdentityRole(SD.KitchenUser)).GetAwaiter().GetResult();
			_roleManager.CreateAsync(new IdentityRole(SD.CustomerEndUser)).GetAwaiter().GetResult();

			_userManager.CreateAsync(new ApplicationUser
			{
				UserName = "admin@gmail.com",
				Email = "admin@gmail.com",
				Name = "Smit Patel",
				EmailConfirmed = true,
				PhoneNumber = "1234567890"
			}, "Admin@123").GetAwaiter().GetResult();


			var user = await _db.Users.Where(u => u.Email == "admin@gmail.com").FirstOrDefaultAsync();

			await _userManager.AddToRoleAsync(user, SD.ManagerUser);

			return true;
		}
	}
}
