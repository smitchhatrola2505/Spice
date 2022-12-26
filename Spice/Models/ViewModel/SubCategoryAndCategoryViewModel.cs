﻿using System.Net.NetworkInformation;

namespace Spice.Models.ViewModel
{

	public class SubCategoryAndCategoryViewModel
	{
		public IEnumerable<Category> CategoryList { get; set; }
		public SubCategory SubCategory { get; set; }
		public List<string> SubCategoryList { get; set; }
		public string StatusMessage { get; set; }
	}
}
