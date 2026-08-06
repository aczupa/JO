using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using JO;
using JO.Models;
using JO.Data;
using JO.Services;
using System.Net.Http.Json;

namespace JO.Tests
{
    public class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProgramTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Application_Starts_Successfully()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/");
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Redirect);
        }

        [Fact]
        public void Services_Are_Registered()
        {
            using var scope = _factory.Services.CreateScope();
            var services = scope.ServiceProvider;

            Assert.NotNull(services.GetService<IOfferService>());
            Assert.NotNull(services.GetService<ICartService>());
            Assert.NotNull(services.GetService<IUserProfileService>());
            Assert.NotNull(services.GetService<IQRCodeService>());
            Assert.NotNull(services.GetService<IEmailService>());
        }

        [Fact]
        public void Identity_Is_Configured()
        {
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>();
            var signInManager = scope.ServiceProvider.GetService<SignInManager<IdentityUser>>();

            Assert.NotNull(userManager);
            Assert.NotNull(signInManager);
        }

        [Fact]
        public void Database_Is_Created()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>();
            using var dbContext = dbContextFactory.CreateDbContext();

            bool dbExists = dbContext.Database.CanConnect();
            Assert.True(dbExists, "Database should be created and reachable.");
        }

        [Fact]
        public void SmtpOptions_Are_Configured()
        {
            using var scope = _factory.Services.CreateScope();
            var options = scope.ServiceProvider.GetService<IOptions<SmtpOptions>>();

            Assert.NotNull(options);
            Assert.False(string.IsNullOrEmpty(options.Value.Host), "SMTP host should be configured.");
        }

        // ——— tests for API endpoints ———

        [Fact]
        public async Task Get_Offers_ReturnsHtmlWhenAnonymous()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/offers");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task Get_Cart_ReturnsHtmlWhenAnonymous()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/cart");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task Post_Cart_ReturnsBadRequestWhenAnonymous()
        {
            var client = _factory.CreateClient();
            var newItem = new { OfferId = 1, Quantity = 1 };
            var response = await client.PostAsJsonAsync("/api/cart", newItem);

            // Currently anonymous POST returns BadRequest; adjust when auth is enforced
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task QRCodeEndpoint_ReturnsHtmlWhenAnonymous()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/qrcode?text=hello");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task UserProfile_RequiresAuthentication()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/userprofile");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public void EmailService_SendAsync_DoesNotThrow()
        {
            using var scope = _factory.Services.CreateScope();
            var emailSvc = scope.ServiceProvider.GetService<IEmailService>();
            var sendTask = emailSvc.SendEmailAsync("test@example.com", "Subject", "Body");
            var ex = Record.Exception(() => sendTask.GetAwaiter().GetResult());
            Assert.Null(ex);
        }
    }
}
