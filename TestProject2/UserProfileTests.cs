using System.Threading.Tasks;
using JO.Data;
using JO.Models;
using JO.Pages;
using JO.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace JO.Tests.Services
{
    public class UserProfileServiceTests
    {
        [Fact]
        public async Task SaveUserProfile_Should_Add_UserProfile_And_SaveChanges()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using var context = new DataContext(options);
            var service = new UserProfileService(context);

            var userId = "testUserId";
            var checkoutInfo = new PersonalData.CheckoutInfo
            {
                FirstName = "John",
                LastName = "Doe",
                Street = "Main St",
                StreetNumber = "123",
                PostalCode = "1000",
                City = "Paris",
                Country = "France",
                PaymentMethod = "CreditCard"
            };

            // Act
            await service.SaveUserProfile(userId, checkoutInfo);

            // Assert
            var userProfile = await context.UserProfile.FirstOrDefaultAsync(u => u.UserId == userId);

            Assert.NotNull(userProfile);
            Assert.Equal(userId, userProfile.UserId);
            Assert.Equal(checkoutInfo.FirstName, userProfile.FirstName);
            Assert.Equal(checkoutInfo.LastName, userProfile.LastName);
            Assert.Equal(checkoutInfo.Street, userProfile.Street);
            Assert.Equal(checkoutInfo.StreetNumber, userProfile.StreetNumber);
            Assert.Equal(checkoutInfo.PostalCode, userProfile.PostalCode);
            Assert.Equal(checkoutInfo.City, userProfile.City);
            Assert.Equal(checkoutInfo.Country, userProfile.Country);
            Assert.Equal(checkoutInfo.PaymentMethod, userProfile.PaymentMethod);
        }

        [Fact]
        public async Task SaveUserProfile_Should_Save_Exactly_One_UserProfile()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_SaveOnce")
                .Options;

            using var context = new DataContext(options);
            var service = new UserProfileService(context);

            var userId = "anotherUser";
            var checkoutInfo = new PersonalData.CheckoutInfo
            {
                FirstName = "Jane",
                LastName = "Smith",
                Street = "Second St",
                StreetNumber = "456",
                PostalCode = "2000",
                City = "Lyon",
                Country = "France",
                PaymentMethod = "PayPal"
            };

            // Act
            await service.SaveUserProfile(userId, checkoutInfo);

            // Assert
            var count = await context.UserProfile.CountAsync();
            Assert.Equal(1, count);
        }
    }
}
