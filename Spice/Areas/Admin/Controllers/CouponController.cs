using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModel;

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

		[BindProperty]
		public Coupon coupons { get; set; }

		public async Task<IActionResult> Index()
		{
			var couponOrder = await _db.Coupon.ToListAsync();
		    var coupon =  from a in couponOrder
							   orderby a.Discount
						   select a;
			return View(coupon);  
		}

		//GET-Create
		[HttpGet]

		public async Task<IActionResult> Create()
		{
			return View();
		}

		//Post-Create

		[HttpPost, ActionName("Create")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreatePost()
		{
			if (!ModelState.IsValid) //not work
			{
				var files = HttpContext.Request.Form.Files;
				if (files.Count > 0)
				{
					byte[] p1 = null;
					using (var fs1 = files[0].OpenReadStream())
					{
						using (var ms1 = new MemoryStream())
						{
							fs1.CopyTo(ms1);
							p1 = ms1.ToArray();
						}
					}
					coupons.Picture = p1;
				}
			}
			_db.Coupon.Add(coupons);
			await _db.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		//GET-Edit
		[HttpGet]

		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var coupon = await _db.Coupon.SingleOrDefaultAsync(M => M.Id == id);
			if (coupon == null)
			{
				return NotFound();
			}
			return View(coupon);
		}

		//Post-Edit

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit()
		{
			if (coupons.Id == 0)
			{
				return NotFound();
			}

			var couponFromDb = await _db.Coupon.Where(c => c.Id == coupons.Id).FirstOrDefaultAsync();

			if (!ModelState.IsValid)
			{
				var files = HttpContext.Request.Form.Files;
				if (files.Count > 0)
				{
					byte[] p1 = null;
					using (var fs1 = files[0].OpenReadStream())
					{
						using (var ms1 = new MemoryStream())
						{
							fs1.CopyTo(ms1);
							p1 = ms1.ToArray();
						}
					}
					couponFromDb.Picture = p1;
				}
				couponFromDb.MinimumAmount = coupons.MinimumAmount;
				couponFromDb.Name = coupons.Name;
				couponFromDb.Discount = coupons.Discount;
				couponFromDb.CouponType = coupons.CouponType;
				couponFromDb.IsActive = coupons.IsActive;

				await _db.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(coupons);
		}


		//[HttpPost, ActionName("Edit")]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> EditPost()
		//{
		//	if (coupons.Id == 0)
		//	{
		//		return NotFound();
		//	}

		//	var couponFromDb = await _db.Coupon.Where(c => c.Id == coupons.Id).FirstOrDefaultAsync();

		//	if (!ModelState.IsValid) //not work
		//	{
		//		var files = HttpContext.Request.Form.Files;
		//		if (files.Count > 0)
		//		{
		//			byte[] p1 = null;
		//			using (var fs1 = files[0].OpenReadStream())
		//			{
		//				using (var ms1 = new MemoryStream())
		//				{
		//					fs1.CopyTo(ms1);
		//					p1 = ms1.ToArray();
		//				}
		//			}
		//			couponFromDb.Picture = p1;
		//		}

		//		couponFromDb.MinimumAmount = coupons.MinimumAmount;
		//		couponFromDb.Name = coupons.Name;
		//		couponFromDb.Discount = coupons.Discount;
		//		couponFromDb.CouponType = coupons.CouponType;
		//		couponFromDb.IsActive = coupons.IsActive;

		//		await _db.SaveChangesAsync();
		//		return RedirectToAction(nameof(Index));
		//	}
		//	return View(coupons);
		//}

		//GET-Details
		[HttpGet]

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var coupon = await _db.Coupon.SingleOrDefaultAsync(M => M.Id == id);
			if (coupon == null)
			{
				return NotFound();
			}
			return View(coupon);
		}

		//Post-Details

		[HttpPost, ActionName("Details")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DetailsPost()
		{
			if (coupons.Id == 0)
			{
				return NotFound();
			}

			var couponFromDb = await _db.Coupon.Where(c => c.Id == coupons.Id).FirstOrDefaultAsync();

			if (!ModelState.IsValid) //not work
			{
				var files = HttpContext.Request.Form.Files;
				if (files.Count > 0)
				{
					byte[] p1 = null;
					using (var fs1 = files[0].OpenReadStream())
					{
						using (var ms1 = new MemoryStream())
						{
							fs1.CopyTo(ms1);
							p1 = ms1.ToArray();
						}
					}
					couponFromDb.Picture = p1;
				}

				couponFromDb.MinimumAmount = coupons.MinimumAmount;
				couponFromDb.Name = coupons.Name;
				couponFromDb.Discount = coupons.Discount;
				couponFromDb.CouponType = coupons.CouponType;
				couponFromDb.IsActive = coupons.IsActive;

				await _db.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(coupons);
		}


		//GET-Delete
		[HttpGet]

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var coupon = await _db.Coupon.SingleOrDefaultAsync(M => M.Id == id);
			if (coupon == null)
			{
				return NotFound();
			}
			return View(coupon);
		}

		//Post-Delete

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeletePost(int id)
		{
			var couponDelete = await _db.Coupon.SingleOrDefaultAsync(m=> m.Id == id);
			_db.Coupon.Remove(couponDelete);
			await _db.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
	}
}
