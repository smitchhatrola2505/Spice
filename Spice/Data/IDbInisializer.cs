using Microsoft.AspNetCore.Mvc;

namespace Spice.Data
{
	public interface IDbInisializer
	{
		Task<bool> Initialize(); 
	}
}
