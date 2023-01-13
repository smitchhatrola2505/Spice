
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Spice.Data;
using Spice.Services;
using Spice.Utility;
using Stripe;
using static System.Formats.Asn1.AsnWriter;


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

builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.Configure<EmailOptions>(builder.Configuration);

builder.Services.AddScoped<IDbInisializer,DbInitializer>();


builder.Services.AddAuthentication().AddFacebook(FacebookOptions =>
{
	FacebookOptions.AppId = "729873621887318";
	FacebookOptions.AppSecret = "1aced6919bb47628d5fbf8d9521f6cd9";
});

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


using (var scope = app.Services.CreateAsyncScope())
{
	var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInisializer>();
	// use dbInitializer
	dbInitializer.Initialize();
}

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
