using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StarProject.Attributes;
using StarProject.Data;
using StarProject.Models;
using StarProject.Services;
using OfficeOpenXml;



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

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString));

			builder.Services.AddDbContext<StarProjectContext>(options =>
			{
				options.UseSqlServer(builder.Configuration.GetConnectionString("StarProject"));
			});

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Identity
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<IPromotionService, PromotionService>();


			// MVC �t�m - �[�J����L�o��
			builder.Services.AddControllersWithViews(options =>
			{
				// ������U�j��K�X�ק�L�o��
				options.Filters.Add<ForcePasswordChangeAttribute>();
			});

			builder.Services.AddRazorPages();

			// �l��A�Ȱt�m
			builder.Services.Configure<EmailSettings>(
				builder.Configuration.GetSection("EmailSettings"));
			builder.Services.AddScoped<IEmailService, EmailService>();

			// Cookie ���Ұt�m
			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					options.LoginPath = "/Login/Index";
					options.LogoutPath = "/Login/Index";
					options.AccessDeniedPath = "/Home/AccessDenied";
					options.ExpireTimeSpan = TimeSpan.FromHours(8);
					options.SlidingExpiration = true;
				});

			builder.Services.AddAuthorization();

      //除錯幫助
      builder.Logging.AddConsole();
			builder.Logging.SetMinimumLevel(LogLevel.Information);
          
			//Excel需要
      ExcelPackage.License.SetNonCommercialOrganization("StarProject Dev Team");
          
			//MailService
			//builder.Services.AddTransient<MailService>();

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


			// ���ҩM���v
			app.UseAuthentication();
			app.UseAuthorization();

			// ���Ѱt�m
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Login}/{action=Index}/{id?}");

			app.MapRazorPages();

			app.Run();
		}
	}
}