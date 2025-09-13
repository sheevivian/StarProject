using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit;
using NETCore.MailKit.Core;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using StarProject.Data;
using StarProject.Models;
using MailKitOptions = NETCore.MailKit.Core.MailKitOptions;

namespace StarProject
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

			// ��Ʈw�t�m
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString));

			builder.Services.AddDbContext<StarProjectContext>(options =>
			{
				options.UseSqlServer(builder.Configuration.GetConnectionString("StarProject"));
			});

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// Identity �t�m
			builder.Services.AddDefaultIdentity<IdentityUser>(options =>
				options.SignIn.RequireConfirmedAccount = true)
				.AddEntityFrameworkStores<ApplicationDbContext>();

			// MVC �t�m
			builder.Services.AddControllersWithViews();
			builder.Services.AddRazorPages();

			// �l��A�Ȱt�m
			builder.Services.AddMailKit(config =>
			{
				config.UseMailKit(builder.Configuration.GetSection("Email").Get<MailKitOptions>());
			});

			// Cookie ���Ұt�m
			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					options.LoginPath = "/Login";
					options.LogoutPath = "/Login";
					options.AccessDeniedPath = "/Home/AccessDenied";
					options.ExpireTimeSpan = TimeSpan.FromHours(8);
				});

			// �������ҵ���
			builder.Services.AddAuthorization(options =>
			{
				options.FallbackPolicy = new AuthorizationPolicyBuilder()
					.RequireAuthenticatedUser()
					.Build();
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
				app.UseHsts();
			}

			app.UseHttpsRedirection();

			// �R�A���䴩 - �����b UseRouting ���e
			app.UseStaticFiles();

			app.UseRouting();

			// ���ҩM���v - �����b�o�Ӷ���
			app.UseAuthentication();
			app.UseAuthorization();

			// ���A�X�����B�z - �b���v����
			app.UseStatusCodePagesWithReExecute("/Home/AccessDenied");

			// ���Ѱt�m
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.MapRazorPages();

			app.Run();
		}
	}
}