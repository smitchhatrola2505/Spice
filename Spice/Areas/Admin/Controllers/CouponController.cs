using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;

namespace Spice.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class CouponController : Controller
	{
		private readonly ApplicationDbContext _db;
		public CouponController(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IActionResult> Index()
		{
			return View(await _db.Coupon.ToListAsync());
		}

		//GET-Create
		[HttpGet]

		public async Task <IActionResult> Create()
		{
			return View();
		}

		//Post-Create

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreatePost(Coupon coupons)
		{
			if(ModelState.IsValid)
			{
				byte[] p1 = null;

				using (var fs1 = files[0].OpenReadStrem())
				{

				}
			}

			return RedirectToAction(nameof(Index);
		}
	}
}
