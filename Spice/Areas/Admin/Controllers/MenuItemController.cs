using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models.ViewModel;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Spice.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class MenuItemController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly IWebHostEnvironment _hostingEnvironment;

		[BindProperty]
		public MenuitemViewModel MenuItemVM { get; set; }

		public MenuItemController(ApplicationDbContext db, IWebHostEnvironment hostingEnvironment)
		{
			_db = db;
			_hostingEnvironment = hostingEnvironment;

			MenuItemVM = new MenuitemViewModel() { 
				Category = _db.Category,
				MenuItem = new Models.MenuItem()
			};
		}

		public async Task<IActionResult> Index()
		{
			var menuItems = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync();
			return View(menuItems);
		}

		//GET - CREATE

		public IActionResult Create()
		{
			return View(MenuItemVM);
		}
	}
}
