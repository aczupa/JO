using Bunit;
using Moq;
using Xunit;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using JO.Services;
using JO.Data;
using Microsoft.EntityFrameworkCore;
using JO.Models;
using Microsoft.Extensions.DependencyInjection;

public class ConfirmationComponentTests : TestContext
{
    [Fact]
    public async Task OnInitializedAsync_DisplaysSuccessMessageAndSendsEmail()
    {
        // Arrange
        var userId = "test-user-id";
        var userEmail = "test@example.com";
        var firstName = "Jean";
        var lastName = "Dupont";

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail),
            new Claim(ClaimTypes.Name, $"{firstName} {lastName}")
        }, "testAuth"));

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.Setup(cs => cs.PlaceOrderAndClearCart(userId))
            .ReturnsAsync(new JO.Models.Responses.GetCartItemResponse { Success = true });

        var qrCodeServiceMock = new Mock<IQRCodeService>();
        qrCodeServiceMock.Setup(q => q.GenerateQRCodeWithBytes(It.IsAny<string>()))
            .Returns(("base64string", new byte[] { 1, 2, 3 }));

        var emailServiceMock = new Mock<IEmailService>();

        // DbContext in-memory
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        var dbContext = new DataContext(options);

       
        dbContext.UserProfile.Add(new UserProfile
        {
            Id = 1,
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            City = "Paris",
            Country = "France",
            PaymentMethod = "CreditCard",
            PostalCode = "75000",
            Street = "Champs-Élysées",
            StreetNumber = "1"
        });
        dbContext.SaveChanges();

        
        Services.AddSingleton(authProviderMock.Object);
        Services.AddSingleton(cartServiceMock.Object);
        Services.AddSingleton(qrCodeServiceMock.Object);
        Services.AddSingleton(emailServiceMock.Object);
        Services.AddSingleton(dbContext);

        // Act
        var component = RenderComponent<JO.Pages.Confirmation>();

       
        await Task.Delay(500);

        // Assert
        Assert.Contains($"Merci pour votre commande, {firstName} {lastName}!", component.Markup);

        cartServiceMock.Verify(cs => cs.PlaceOrderAndClearCart(userId), Times.Once);
        qrCodeServiceMock.Verify(q => q.GenerateQRCodeWithBytes(It.IsAny<string>()), Times.Once);
        emailServiceMock.Verify(e => e.SendEmailWithAttachmentAsync(
            userEmail,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }
}
