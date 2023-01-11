
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Services;
using Spice.Utility;
using Stripe;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>() //options => options.SignIn.RequireConfirmedAccount = true
	.AddDefaultTokenProviders()
		 .AddDefaultUI()
	.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSession(option =>
{
	option.Cookie.IsEssential= true;
	option.IdleTimeout = TimeSpan.FromMinutes(35);
	option.Cookie.HttpOnly= true;
});


builder.Services.Configure<StripeSetting>(
	builder.Configuration.GetSection("Stripe"));

builder.Services.AddAuthentication().AddFacebook(FacebookOptions =>
{
	FacebookOptions.AppId = "729873621887318";
	FacebookOptions.AppSecret = "1aced6919bb47628d5fbf8d9521f6cd9";
	 
});


builder.Services.AddSingleton<IEmailSender, EmailSender>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe")["SecretKey"];

app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
	name: "areas",
	pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

//app.UseEndpoints(endpoints =>
//{
//	endpoints.MapControllerRoute(
//		name: "areas",
//		pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
//	endpoints.MapRazorPages();
//});

app.Run();
