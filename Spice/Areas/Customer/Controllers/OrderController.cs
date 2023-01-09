﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModel;
using System.Security.Claims;

namespace Spice.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class OrderController : Controller
	{

		private readonly ApplicationDbContext _db;
		private int PageSize = 2;


		public OrderController(ApplicationDbContext db)
		{
			_db = db;
		}

		[Authorize]

		public async Task<IActionResult> Confirm(int id)
		{

			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
			{
				OrderHeader = await _db.OrderHeader.Include(o => o.ApplicationUser).FirstOrDefaultAsync(o => o.Id == id && o.UserId == claim.Value),
				OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == id).ToListAsync()	
			};
			return View(orderDetailsViewModel);
		}



		public IActionResult Index()
		{
			return View();
		}


		[Authorize]
		public async Task<IActionResult> OrderHistory(int ProductPage = 1)
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			OrderListViewModel orderListVM = new OrderListViewModel()
			{
				Orders = new List<OrderDetailsViewModel>()
			};



			List<OrderHeader> OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser).Where(u => u.UserId == claim.Value).ToListAsync();

			foreach (OrderHeader item in OrderHeaderList)
			{
				OrderDetailsViewModel individule = new OrderDetailsViewModel()
				{
					OrderHeader = item,
					OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
				};
				orderListVM.Orders.Add(individule);
			}

			var count = orderListVM.Orders.Count;
			orderListVM.Orders = orderListVM.Orders.OrderByDescending(p => p.OrderHeader.Id)
								.Skip((ProductPage - 1) * PageSize)
								.Take(PageSize).ToList();

			orderListVM.PageingInfo = new Pageinginfo
			{
				CurrentPage = ProductPage,
				ItemsPerPage = PageSize,
				TotalItem = count,
				urlParam = "/Customer/Order/OrderHistory?productPage=:"
			};

			return View(orderListVM);
		}



		public async Task<IActionResult> GetOrderDetails (int Id)
		{
			OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
			{
				OrderHeader = await _db.OrderHeader.FirstOrDefaultAsync(m=>m.Id == Id),

				OrderDetails = await _db.OrderDetails.Where(m => m.OrderId == Id).ToListAsync()
		};
			orderDetailsViewModel.OrderHeader.ApplicationUser = await _db.ApplicationUser.FirstOrDefaultAsync(u => u.Id == orderDetailsViewModel.OrderHeader.UserId);

			return PartialView("_IndividualOrderDetails", orderDetailsViewModel);
		}

	}
}
