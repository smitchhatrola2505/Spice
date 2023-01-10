using Spice.Models;

namespace Spice.Utility
{
	public static class SD
	{
		public const string DefaultFoodImage = "default_food.png";

		public const string ManagerUser = "Manager";
		public const string KitchenUser = "Kitchen";
		public const string FrontDeskUser = "FrontDesk";
		public const string CustomerEndUser = "CustomerEnd";

		public const string ssShoppingCartCount = "ssCartCount";
		public const string ssCouponCode = "ssCouponCode";

		public const string StatusSubmited = "Submited";
		public const string StatusInProcess = "Being Prepared";
		public const string StatusReady = "Ready For PickUp";
		public const string StatusCompleted = "Completed";
		public const string StatusCancelled = "Cancelled";

		public const string PaymentStatusPending = "Pending";
		public const string PaymentStatusApproved = "Approved";
		public const string PaymentStatusRejected = "Rejected";




		public static string ConvertToRawHtml(string source)
		{
			char[] array = new char[source.Length];
			int arrayIndex = 0;
			bool inside = false;

			for (int i = 0; i < source.Length; i++)
			{
				char let = source[i];
				if (let == '<')
				{
					inside = true;
					continue;
				}
				if (let == '>')
				{
					inside = false;
					continue;
				}
				if (!inside)
				{
					array[arrayIndex] = let;
					arrayIndex++;
				}
			}
			return new string(array, 0, arrayIndex);
		}

		public static double DiscountPrice(Coupon couponFromDb, double OrignalOrderTotal)
		{
			if (couponFromDb == null)
			{
				return Math.Round(OrignalOrderTotal, 2);
			}
			else
			{
				if (couponFromDb.MinimumAmount > OrignalOrderTotal)
				{
					return Math.Round(OrignalOrderTotal, 2);
				}
				else
				{
					//everything is valid
					if (Convert.ToInt32(couponFromDb.CouponType) == (int)Coupon.ECoupanType.Dollar)
					{
						//$10 off $1000
						return Math.Round(OrignalOrderTotal - couponFromDb.Discount, 2);
					}
					if (Convert.ToInt32(couponFromDb.CouponType) == (int)Coupon.ECoupanType.Percent)
					{
						//10% off $1000
						return Math.Round(OrignalOrderTotal - (OrignalOrderTotal * couponFromDb.Discount / 100), 2);
					}

				}
				return Math.Round(OrignalOrderTotal, 2);
			}
		}

	}

}
