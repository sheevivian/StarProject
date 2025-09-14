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

			// MVC �t�m - �[�J����L�o��
			builder.Services.AddControllersWithViews(options =>
			{
				// ������U�j��K�X�ק�L�o��
				options.Filters.Add<ForcePasswordChangeAttribute>();
			});
			builder.Services.AddRazorPages();

			// �l��A�Ȱt�m
			builder.Services.AddMailKit(config =>
			{
				config.UseMailKit(builder.Configuration.GetSection("Email").Get<MailKitOptions>());
			});

			// Cookie ���Ұt�m - �ץ����|
			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					options.LoginPath = "/Login/Index";  // �ץ��G���V���T������M�ʧ@
					options.LogoutPath = "/Login/Index";
					options.AccessDeniedPath = "/Home/AccessDenied";
					options.ExpireTimeSpan = TimeSpan.FromHours(8);
					options.SlidingExpiration = true; // �[�J�ưʹL���ɶ�
				});

			// �ץ��G�����������ҵ����A�אּ�b�ݭn������W�[ [Authorize]
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

			// ���ҩM���v
			app.UseAuthentication();
			app.UseAuthorization();

			// ������v���e�A�קK�z�Z�n�J�y�{
			// app.UseStatusCodePagesWithReExecute("/Home/AccessDenied");

			// ���Ѱt�m - �ץ��G�w�]�ɦV�n�J����
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Login}/{action=Index}/{id?}");

			app.MapRazorPages();

			app.Run();
		}
	}
}