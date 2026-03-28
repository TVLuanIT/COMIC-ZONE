using COMICZONE.Data;
using COMICZONE.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            // --- add session ---
            builder.Services.AddDistributedMemoryCache(); // save session temporary in memory
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // timeout session
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Authentication/Login";
                    options.AccessDeniedPath = "/Authentication/Login";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                });

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ComiczoneContext>(options =>
                options.UseSqlServer(connectionString, o => o.UseCompatibilityLevel(120))
                       .LogTo(Console.WriteLine));
            //builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ComiczoneContext>();
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddControllersWithViews();

            builder.Services.AddSingleton<IVnPayService, VnPayService>();

            builder.Services.AddScoped<IProductSearchService, ProductSearchService>();
            builder.Services.AddScoped<IChatbotService, ChatbotService>();
            builder.Services.AddHttpClient<IGeminiService, GeminiService>();

            builder.Services.AddScoped<IRecommendationService, RecommendationService>();

            builder.Services.AddSingleton(x => new PaypalClient
            (
                builder.Configuration["Paypal:AppId"],
                builder.Configuration["Paypal:AppSecret"],
                builder.Configuration["Paypal:Mode"]
            ));

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

            // --- active session ---
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            //app.MapRazorPages();

            app.Run();
        }
    }
}
