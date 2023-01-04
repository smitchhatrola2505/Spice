using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModel;
using Spice.Utility;
using System.Diagnostics;
using System.Security.Claims;

namespace Spice.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _db;

		public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
		{
			_logger = logger;
			_db = db;
		}

		public async Task<IActionResult> Index()
		{
			IndexViewModel IndexVM = new IndexViewModel()
			{
				MenuItem = await _db.MenuItem.Include(c => c.Category).Include(s => s.SubCategory).ToListAsync(),
				Category = await _db.Category.ToListAsync(),
				Coupon = await _db.Coupon.Where(c => c.IsActive == true).ToListAsync(),
			};

			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);	 
		
			if(claim != null)
			{
				var cnt = _db.ShoppingCart.Where(u=>u.ApplicationUserId == claim.Value).ToList().Count;
				HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);
			}

			return View(IndexVM);
		}

		[Authorize]

		public async Task<IActionResult> Details(int id)
		{
			var MenuItemFromDb = await _db.MenuItem.Include(c => c.Category).Include(c => c.SubCategory).Where(m => m.Id == id).FirstOrDefaultAsync();
			ShoppingCart cartobj = new ShoppingCart()
			{
				MenuItem = MenuItemFromDb,
				MenuItemId = MenuItemFromDb.Id
			};
			return View(cartobj);

		}

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]

		public async Task<IActionResult> Details(ShoppingCart CartObject)
		{
			CartObject.Id = 0;

			if (!ModelState.IsValid) //Error : Model State Not Correct
			{
				var cliamsIdentity = (ClaimsIdentity)this.User.Identity;
				var claim = cliamsIdentity.FindFirst(ClaimTypes.NameIdentifier);
				CartObject.ApplicationUserId = claim.Value;

				ShoppingCart cartFromDb = await _db.ShoppingCart.Where(c => c.ApplicationUserId == CartObject.ApplicationUserId 
				&&c.MenuItemId == CartObject.MenuItemId).FirstOrDefaultAsync();

				if(cartFromDb == null)
				{
					await _db.ShoppingCart.AddAsync(CartObject);
				}
				else
				{
					cartFromDb.Count = cartFromDb.Count + CartObject.Count;
				}
				await _db.SaveChangesAsync();	

				var count = _db.ShoppingCart.Where(c => c.ApplicationUserId == CartObject.ApplicationUserId).ToList().Count();
				HttpContext.Session.SetInt32(SD.ssShoppingCartCount, count);

				return RedirectToAction("Index");
			}
			else
			{
				var MenuItemFromDb = await _db.MenuItem.Include(c => c.Category).Include(c => c.SubCategory).Where(m => m.Id == CartObject.MenuItemId).FirstOrDefaultAsync();
				 

				ShoppingCart cartobj = new ShoppingCart()
				{
					MenuItem = MenuItemFromDb,
					MenuItemId = MenuItemFromDb.Id
				};
				return View(cartobj);

			}
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