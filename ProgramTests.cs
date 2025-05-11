using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using JO;
using JO.Services;
using JO.Models;
using JO.Data;

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
    }
}
