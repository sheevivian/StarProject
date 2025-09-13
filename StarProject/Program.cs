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

			// 資料庫配置
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString));

			builder.Services.AddDbContext<StarProjectContext>(options =>
			{
				options.UseSqlServer(builder.Configuration.GetConnectionString("StarProject"));
			});

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// Identity 配置
			builder.Services.AddDefaultIdentity<IdentityUser>(options =>
				options.SignIn.RequireConfirmedAccount = true)
				.AddEntityFrameworkStores<ApplicationDbContext>();

			// MVC 配置
			builder.Services.AddControllersWithViews();
			builder.Services.AddRazorPages();

			// 郵件服務配置
			builder.Services.AddMailKit(config =>
			{
				config.UseMailKit(builder.Configuration.GetSection("Email").Get<MailKitOptions>());
			});

			// Cookie 驗證配置
			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					options.LoginPath = "/Login";
					options.LogoutPath = "/Login";
					options.AccessDeniedPath = "/Home/AccessDenied";
					options.ExpireTimeSpan = TimeSpan.FromHours(8);
				});

			// 全域驗證策略
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

			// 靜態文件支援 - 必須在 UseRouting 之前
			app.UseStaticFiles();

			app.UseRouting();

			// 驗證和授權 - 必須在這個順序
			app.UseAuthentication();
			app.UseAuthorization();

			// 狀態碼頁面處理 - 在授權之後
			app.UseStatusCodePagesWithReExecute("/Home/AccessDenied");

			// 路由配置
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.MapRazorPages();

			app.Run();
		}
	}
}