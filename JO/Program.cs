using JO.Data;
using JO.Services;
using JO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Dodanie Razor Pages i Blazora
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            // Rejestracja usług aplikacji
            builder.Services.AddScoped<IOfferService, OfferService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IQRCodeService, QRCodeService>();
            builder.Services.AddScoped<IEmailService, EmailService>();

            // Połączenie do lokalnej bazy danych SQL Server
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") + ";TrustServerCertificate=True";

            builder.Services.AddDbContextFactory<DataContext>(options =>
                options.UseSqlServer(connectionString));

            // Konfiguracja tożsamości
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 10;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<DataContext>();

            // Konfiguracja SMTP z appsettings.json
            builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("SmtpOptions"));

            var app = builder.Build();

            // Upewniamy się, że baza danych istnieje
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                dbContext.Database.EnsureCreated();
            }

            // Obsługa błędów i HSTS w środowisku produkcyjnym
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}
