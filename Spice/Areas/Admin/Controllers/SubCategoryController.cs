using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models.ViewModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Spice.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class SubCategoryController : Controller
	{
		private readonly ApplicationDbContext _db;

		public SubCategoryAndCategoryViewModel SubCategoryAndCategoryViewModel { get; private set; }

		public SubCategoryController(ApplicationDbContext db)
		{
			_db = db;
		}

		//GET - Index
		public async Task<IActionResult> Index()
		{
			var SubCategory = await _db.SubCategory.Include(s => s.Category).ToListAsync();
			return View(SubCategory);
		}

		//GET - CREATE
		public async Task<IActionResult> Create()
		{
			SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
			{
				CategoryList = await _db.Category.ToListAsync(),
				SubCategory = new Models.SubCategory(),
				SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
			};

			return View(model);
		}
	}
}
