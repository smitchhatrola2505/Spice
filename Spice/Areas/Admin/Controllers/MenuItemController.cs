﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models.ViewModel;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
	[Area("Admin")]
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
			return View(menuItems);
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
        public async Task <IActionResult> Edit(int? id)
        {
			MenuItemVM.MenuItem = await _db.MenuItem.Include(x => x.Category).Include(x=>x.SubCategory).SingleOrDefaultAsync();
			MenuItemVM.SubCategory = await _db.SubCategory.Where(s=>s.CategoryId==MenuItemVM.MenuItem.CategoryId).ToListAsync();	
            
			if(MenuItemVM.MenuItem == null)
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


    }
}
