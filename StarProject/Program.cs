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
using StarProject.Attributes;
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

			// MVC 配置 - 加入全域過濾器
			builder.Services.AddControllersWithViews(options =>
			{
				// 全域註冊強制密碼修改過濾器
				options.Filters.Add<ForcePasswordChangeAttribute>();
			});
			builder.Services.AddRazorPages();

			// 郵件服務配置
			builder.Services.AddMailKit(config =>
			{
				config.UseMailKit(builder.Configuration.GetSection("Email").Get<MailKitOptions>());
			});

			// Cookie 驗證配置 - 修正路徑
			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					options.LoginPath = "/Login/Index";  // 修正：指向正確的控制器和動作
					options.LogoutPath = "/Login/Index";
					options.AccessDeniedPath = "/Home/AccessDenied";
					options.ExpireTimeSpan = TimeSpan.FromHours(8);
					options.SlidingExpiration = true; // 加入滑動過期時間
				});

			// 修正：移除全域驗證策略，改為在需要的控制器上加 [Authorize]
			builder.Services.AddAuthorization();

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
			app.UseStaticFiles();
			app.UseRouting();

			// 驗證和授權
			app.UseAuthentication();
			app.UseAuthorization();

			// 移到授權之前，避免干擾登入流程
			// app.UseStatusCodePagesWithReExecute("/Home/AccessDenied");

			// 路由配置 - 修正：預設導向登入頁面
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Login}/{action=Index}/{id?}");

			app.MapRazorPages();

			app.Run();
		}
	}
}