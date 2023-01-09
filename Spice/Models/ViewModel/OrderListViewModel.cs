namespace Spice.Models.ViewModel
{
	public class OrderListViewModel
	{
		public IList<OrderDetailsViewModel> Orders { get; set; }
		public Pageinginfo PageingInfo { get; set; }

	}	
}
