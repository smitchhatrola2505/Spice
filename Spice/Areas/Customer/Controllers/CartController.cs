using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models.ViewModel;
using Spice.Utility;
using System.Security.Claims;

namespace Spice.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class CartController : Controller
	{
		private readonly ApplicationDbContext _db;


		[BindProperty]
		public OrderDetailsCart detailsCart { get; set; }

		public CartController(ApplicationDbContext db)
		{
			_db = db;
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
				detailsCart.OrderHeader.OrderTotal = detailsCart.OrderHeader.OrderTotal + (list.MenuItem.Price * list.Count);
				list.MenuItem.Description = SD.ConvertToRawHtml(list.MenuItem.Description);

				if (list.MenuItem.Description.Length > 105)
				{
					list.MenuItem.Description = list.MenuItem.Description.Substring(0, 105) + "....";
				}
			}

			detailsCart.OrderHeader.OrderTotalOriginal = detailsCart.OrderHeader.OrderTotal;

			return View(detailsCart);
		}
	}
}
