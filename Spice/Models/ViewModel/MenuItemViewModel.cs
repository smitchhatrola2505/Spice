namespace Spice.Models.ViewModel
{
	public class MenuitemViewModel
	{
		public MenuItem MenuItem { get; set; }
		public IEnumerable<Category> Category { get; set; }
		public IEnumerable<SubCategory> SubCategory { get; set; }
	}
}
