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


            var app = builder.Build();

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
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
