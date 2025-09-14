using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StarProject.Data;
using StarProject.Models;
using StarProject.Services;


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
			builder.Logging.AddConsole();
			builder.Logging.SetMinimumLevel(LogLevel.Information);

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString));
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			builder.Services.AddDbContext<StarProjectContext>(options =>
			{
				options.UseSqlServer(builder.Configuration.GetConnectionString("StarProject"));
			});

			builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
				.AddEntityFrameworkStores<ApplicationDbContext>();

			builder.Services.AddControllersWithViews();

			// 保留：即時寄送「報名成功信」等功能仍需 MailService
			builder.Services.AddTransient<MailService>();

			// 移除：活動前 7 天提醒的背景工作
			// builder.Services.AddHostedService<SevenDayReminderWorker>();

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

			// 若有登入/註冊流程，建議加上：
			// app.UseAuthentication();

			app.UseAuthorization();

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");
			app.MapRazorPages();

			app.Run();
		}
	}
}
