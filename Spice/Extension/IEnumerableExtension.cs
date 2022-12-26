using Microsoft.AspNetCore.Mvc.Rendering;

namespace Spice.Extension
{
	public static class IEnumerableExtension
	{
		public static IEnumerable<SelectListItem> ToSelectListItem<T>(this IEnumerable<T> items,int selectedValue)
		{
			return from item in items
				   select new SelectListItem
				   {

				   };
		}
	}
}
