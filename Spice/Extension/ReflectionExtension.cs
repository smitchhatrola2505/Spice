namespace Spice.Extension
{
	public static class ReflectionExtension
	{
		public static string GetPropertyValue<T>(this T item, string propertyName)
		{
			return typeof(T).Name;
		}
	}
}