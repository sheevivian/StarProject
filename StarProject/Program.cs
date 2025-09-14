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

			// �O�d�G�Y�ɱH�e�u���W���\�H�v���\�ऴ�� MailService
			builder.Services.AddTransient<MailService>();

			// �����G���ʫe 7 �Ѵ������I���u�@
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

			// �Y���n�J/���U�y�{�A��ĳ�[�W�G
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
