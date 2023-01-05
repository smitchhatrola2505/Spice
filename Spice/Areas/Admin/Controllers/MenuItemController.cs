using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModel;
using Spice.Utility;
using System.Data;

namespace Spice.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.ManagerUser)]

	public class MenuItemController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly IWebHostEnvironment _hostingEnvironment;

		[BindProperty]
		public MenuItemViewModel MenuItemVM { get; set; }

		public MenuItemController(ApplicationDbContext db, IWebHostEnvironment hostingEnvironment)
		{
			_db = db;
			_hostingEnvironment = hostingEnvironment;

			MenuItemVM = new MenuItemViewModel()
			{
				Category = _db.Category,
				MenuItem = new Models.MenuItem()
			};
		}

		public async Task<IActionResult> Index()
		{
			var menuItems = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync();
			if (menuItems == null)
			{
				return NotFound();
			}
			var menuItemsOrder =  from a in menuItems 
								  orderby a.CategoryId
								  select a;

			return View(menuItemsOrder);
		}

		//GET - CREATE

		[HttpGet]
		public IActionResult Create()
		{
			return View(MenuItemVM);
		}

		//POST - CREATE

		[HttpPost, ActionName("Create")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreatePOST()
		{
			MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

			_db.MenuItem.Add(MenuItemVM.MenuItem);
			await _db.SaveChangesAsync();

			//Work on the image saving section

			string webRootPath = _hostingEnvironment.WebRootPath;
			var files = HttpContext.Request.Form.Files;

			var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);

			if (files.Count > 0)
			{
				//files has been uploaded
				var uploads = Path.Combine(webRootPath, "images");
				var extension = Path.GetExtension(files[0].FileName);

				using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension), FileMode.Create))
				{
					files[0].CopyTo(filesStream);
				}
				menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension;

			}
			else
			{
				//no file was uploaded, so use default
				var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
				System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + ".png");
				menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + ".png";
			}
			await _db.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		//GET - EDIT

		[HttpGet]
		public async Task<IActionResult> Edit(int? id)
		{
			MenuItemVM.MenuItem = await _db.MenuItem.Include(x => x.Category).Include(x => x.SubCategory).SingleOrDefaultAsync(x => x.Id == id);
			MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();

			if (MenuItemVM.MenuItem == null)
			{
				return NotFound();
			}
			return View(MenuItemVM);
		}

		//POST - EDIT

		[HttpPost, ActionName("Edit")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPOST(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

			//Work on the image saving section

			string webRootPath = _hostingEnvironment.WebRootPath;
			var files = HttpContext.Request.Form.Files;

			var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);

			if (files.Count > 0)
			{
				//New files has been uploaded
				var uploads = Path.Combine(webRootPath, "images");
				var extension_new = Path.GetExtension(files[0].FileName);

				//Delete the original file
				var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));

				if (System.IO.File.Exists(imagePath))
				{
					System.IO.File.Delete(imagePath);
				}

				//we will upload the new file

				using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension_new), FileMode.Create))
				{
					files[0].CopyTo(filesStream);
				}
				menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension_new;
			}

			menuItemFromDb.Name = MenuItemVM.MenuItem.Name;
			menuItemFromDb.Price = MenuItemVM.MenuItem.Price;
			menuItemFromDb.Description = MenuItemVM.MenuItem.Description;
			menuItemFromDb.Spicyness = MenuItemVM.MenuItem.Spicyness;
			menuItemFromDb.CategoryId = MenuItemVM.MenuItem.CategoryId;
			menuItemFromDb.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;

			await _db.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		//GET - Details

		[HttpGet]
		public async Task<IActionResult> Details(int? id)
		{
			MenuItemVM.MenuItem = await _db.MenuItem.Include(x => x.Category).Include(x => x.SubCategory).SingleOrDefaultAsync(x => x.Id == id);
			MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();

			if (MenuItemVM.MenuItem == null)
			{
				return NotFound();
			}
			return View(MenuItemVM);
		}

		//GET - DELETE

		[HttpGet]
		public async Task<IActionResult> Delete(int? id)
		{
			MenuItemVM.MenuItem = await _db.MenuItem.Include(x => x.Category).Include(x => x.SubCategory).SingleOrDefaultAsync(x => x.Id == id);
			MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();

			if (MenuItemVM.MenuItem == null)
			{
				return NotFound();
			}
			return View(MenuItemVM);
		}

		//POST - DELETE

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			MenuItem menuItem = await _db.MenuItem.FindAsync(id);
			string webRootPath = _hostingEnvironment.WebRootPath;

			if (menuItem == null)
			{
				return NotFound();
			}
			var imagePath = Path.Combine(webRootPath, menuItem.Image.TrimStart('\\'));

			if (System.IO.File.Exists(imagePath))
			{
				System.IO.File.Delete(imagePath);
			}
			_db.MenuItem.Remove(menuItem);
			await _db.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}
	}
}
