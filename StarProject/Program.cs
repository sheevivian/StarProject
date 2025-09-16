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
using StarProject.Services;
using MailKitOptions = NETCore.MailKit.Core.MailKitOptions;


namespace StarProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ApplicationDbContext (Identity 用)
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure()
                ));

            // StarProjectContext (業務資料庫)
            builder.Services.AddDbContext<StarProjectContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("StarProject"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure()
                ));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Identity
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<IPromotionService, PromotionService>();


			builder.Services.AddMailKit(config =>
			{
				config.UseMailKit(builder.Configuration.GetSection("Email").Get<MailKitOptions>());
			});

			// Cookie 驗證
			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	        .AddCookie(options =>
	        {
		        options.LoginPath = "/Login"; // 未登入會導向此頁
	        });

			/// 全域都要經過驗證才能進入
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
            app.UseStaticFiles();
            app.UseRouting();
            //登入驗證的功能
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseAuthentication(); // 如果有 Identity 登入功能
			app.UseAuthorization();

			app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
