using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Utility;
using System.Data;
using System.Security.Claims;

namespace Spice.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.ManagerUser)]

	public class UserController : Controller
	{
		private readonly ApplicationDbContext _db;
		public UserController(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IActionResult> Index()
		{
			var claimsIdentity = (ClaimsIdentity)this.User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			return View(await _db.ApplicationUser.Where(u => u.Id != claim.Value).ToListAsync());
		}

		//Lock
		public async Task<IActionResult>Lock(string id)
		{
			if (id == null)
			{
				return NotFound();
			}
			var applicationUser = await _db.ApplicationUser.FirstOrDefaultAsync(m => m.Id == id);
			if (applicationUser == null)
			{
				return NotFound();
			}
			applicationUser.LockoutEnd = DateTime.Now.AddDays(7);
			await _db.SaveChangesAsync();
			
			return RedirectToAction(nameof(Index));
		}

		//Unlock
		public async Task<IActionResult> Unlock(string id)
		{
			if (id == null)
			{
				return NotFound();
			}
			var applicationUser = await _db.ApplicationUser.FirstOrDefaultAsync(m => m.Id == id);
			if (applicationUser == null)
			{
				return NotFound();
			}
			applicationUser.LockoutEnd = DateTime.Now;
			await _db.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}
	}
}
