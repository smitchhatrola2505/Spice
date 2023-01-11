using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModel;
using Spice.Utility;
using Stripe;
using System.Security.Claims;

namespace Spice.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class CartController : Controller
	{
		private readonly ApplicationDbContext _db;
		//private readonly IConfiguration _configuration;
		private readonly IEmailSender _emailSender;

		[BindProperty]
		public OrderDetailsCart detailsCart { get; set; }

		public CartController(ApplicationDbContext db, IEmailSender emailSender) 
		{
			_db = db;
			_emailSender = emailSender;
			
		}

		public async Task<IActionResult> Index()
		{
			detailsCart = new OrderDetailsCart()
			{
				OrderHeader = new Models.OrderHeader()
			};

			detailsCart.OrderHeader.OrderTotal = 0;
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			var cart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value);

			if (claim != null)
			{
				detailsCart.listCart = cart.ToList();
			}

			foreach (var list in detailsCart.listCart)
			{
				list.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == list.MenuItemId);
				detailsCart.OrderHeader.OrderTotal = detailsCart.OrderHeader.OrderTotal + Math.Round((list.MenuItem.Price * list.Count), 2);
				list.MenuItem.Description = SD.ConvertToRawHtml(list.MenuItem.Description);

				if (list.MenuItem.Description.Length > 105)
				{
					list.MenuItem.Description = list.MenuItem.Description.Substring(0, 105) + "....";
				}
			}

			detailsCart.OrderHeader.OrderTotalOriginal = detailsCart.OrderHeader.OrderTotal;

			if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
			{
				detailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
				var couponFromDb = await _db.Coupon.Where(c => c.Name.ToUpper() == detailsCart.OrderHeader.CouponCode.ToUpper()).FirstOrDefaultAsync();
				detailsCart.OrderHeader.OrderTotal = SD.DiscountPrice(couponFromDb, detailsCart.OrderHeader.OrderTotalOriginal);
			}
			return View(detailsCart);
		}

		//GET - Summary

		public async Task<IActionResult> Summary()
		{
			detailsCart = new OrderDetailsCart()
			{
				OrderHeader = new Models.OrderHeader()
			};

			detailsCart.OrderHeader.OrderTotal = 0;
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			ApplicationUser applicationUser = await _db.ApplicationUser.Where(c => c.Id == claim.Value).FirstOrDefaultAsync();
			var cart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value);
			if (claim != null)
			{
				detailsCart.listCart = cart.ToList();
			}

			foreach (var list in detailsCart.listCart)
			{
				list.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == list.MenuItemId);
				detailsCart.OrderHeader.OrderTotal = detailsCart.OrderHeader.OrderTotal + Math.Round((list.MenuItem.Price * list.Count), 2);

			}

			detailsCart.OrderHeader.OrderTotalOriginal = detailsCart.OrderHeader.OrderTotal;
			detailsCart.OrderHeader.PickupName = applicationUser.Name;
			detailsCart.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
			detailsCart.OrderHeader.PickUpTime = DateTime.Now;

			if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
			{
				detailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
				var couponFromDb = await _db.Coupon.Where(c => c.Name.ToUpper() == detailsCart.OrderHeader.CouponCode.ToUpper()).FirstOrDefaultAsync();
				detailsCart.OrderHeader.OrderTotal = SD.DiscountPrice(couponFromDb, detailsCart.OrderHeader.OrderTotalOriginal);
			}
			return View(detailsCart);
		}

		//POST - Summary

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("Summary")]

		public async Task<IActionResult> SummaryPOST(string stripeToken)
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			detailsCart.listCart = await _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToListAsync();

			detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
			detailsCart.OrderHeader.OrderDate = DateTime.Now;
			detailsCart.OrderHeader.UserId = claim.Value;
			detailsCart.OrderHeader.Status = SD.PaymentStatusPending;
			detailsCart.OrderHeader.PickUpTime = Convert.ToDateTime(detailsCart.OrderHeader.PickUpDate.ToShortDateString() + " " + detailsCart.OrderHeader.PickUpTime.ToShortTimeString());

			List<OrderDetails> orderDetailsList = new List<OrderDetails>();
			_db.OrderHeader.Add(detailsCart.OrderHeader);
			await _db.SaveChangesAsync();

			detailsCart.OrderHeader.OrderTotalOriginal = 0;


			foreach (var item in detailsCart.listCart)
			{
				item.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == item.MenuItemId);

				OrderDetails orderdetails = new OrderDetails
				{
					MenuItemId = item.MenuItemId,
					OrderId = detailsCart.OrderHeader.Id,
					Description = item.MenuItem.Description,
					Name = item.MenuItem.Name,
					Price = item.MenuItem.Price,
					Count = item.Count
				};

				detailsCart.OrderHeader.OrderTotalOriginal += orderdetails.Count * orderdetails.Price;
				_db.OrderDetails.Add(orderdetails);
			}

			if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
			{
				detailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
				var couponFromDb = await _db.Coupon.Where(c => c.Name.ToUpper() == detailsCart.OrderHeader.CouponCode.ToUpper()).FirstOrDefaultAsync();
				detailsCart.OrderHeader.OrderTotal = SD.DiscountPrice(couponFromDb, detailsCart.OrderHeader.OrderTotalOriginal);
			}
			else
			{
				detailsCart.OrderHeader.OrderTotal = detailsCart.OrderHeader.OrderTotalOriginal;
			}
			detailsCart.OrderHeader.CouponCodeDiscount = detailsCart.OrderHeader.OrderTotalOriginal - detailsCart.OrderHeader.OrderTotal;
			await _db.SaveChangesAsync();

			_db.ShoppingCart.RemoveRange(detailsCart.listCart);
			HttpContext.Session.SetInt32(SD.ssShoppingCartCount, 0);
			await _db.SaveChangesAsync();

			var options = new ChargeCreateOptions
			{
				Amount = Convert.ToInt32(detailsCart.OrderHeader.OrderTotal * 100),
				Currency = "USD",
				Description = "Order ID : " + detailsCart.OrderHeader.Id,
				Source = stripeToken
			};

			var service = new ChargeService();
			Charge charge = service.Create(options);

			if(charge.BalanceTransactionId == null)
			{
				detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
			}
			else
			{
				detailsCart.OrderHeader.TransactionId = charge.BalanceTransactionId;
			}
			if(charge.Status.ToLower() == "succeeded")
			{

				await _emailSender.SendEmailAsync
					(_db.Users.Where(u => u.Id == claim.Value)
					.FirstOrDefault()
					.Email,"Spice - Order Created" + 
					detailsCart.OrderHeader.Id.ToString(),"Order has been submitted successfully.");

				detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
				detailsCart.OrderHeader.Status = SD.StatusSubmited;

			}
			else
			{
				detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;

			}

			await _db.SaveChangesAsync();

			return RedirectToAction("Confirm", "Order", new { id = detailsCart.OrderHeader.Id });
		}

		public async Task<IActionResult> AddCoupon()
		{
			if (detailsCart.OrderHeader.CouponCode == null)
			{
				detailsCart.OrderHeader.CouponCode = " ";
			}
			HttpContext.Session.SetString(SD.ssCouponCode, detailsCart.OrderHeader.CouponCode);

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> RemoveCoupon()
		{

			HttpContext.Session.SetString(SD.ssCouponCode, string.Empty);

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> plus(int cardId)
		{
			var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cardId);
			cart.Count += 1;
			await _db.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> minus(int cardId)
		{
			var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cardId);
			if (cart.Count == 1)
			{
				_db.ShoppingCart.Remove(cart);
				await _db.SaveChangesAsync();

				var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
				HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);
			}
			else
			{
				cart.Count -= 1;
				await _db.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> remove(int cardId)
		{
			var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cardId);
			_db.ShoppingCart.Remove(cart);
			await _db.SaveChangesAsync();

			var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
			HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);

			return RedirectToAction(nameof(Index));
		}


	}
}
