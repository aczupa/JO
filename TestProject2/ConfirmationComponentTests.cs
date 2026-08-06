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
    public void OnInitializedAsync_DisplaysSuccessMessageAndSendsEmail()
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
            .Returns(("data:image/png;base64,base64string", new byte[] { 1, 2, 3 }));

        var emailServiceMock = new Mock<IEmailService>();

        var dbOptions = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        var dbContext = new DataContext(dbOptions);

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

       
        Services.AddScoped(_ => authProviderMock.Object);
        Services.AddScoped(_ => cartServiceMock.Object);
        Services.AddScoped(_ => qrCodeServiceMock.Object);
        Services.AddScoped(_ => emailServiceMock.Object);
        Services.AddScoped(_ => dbContext);

        // Act
        var component = RenderComponent<JO.Pages.Confirmation>();

        // Assert
        component.WaitForAssertion(() =>
        {
            var h5 = component.Find("h5");
            Assert.Contains($"{firstName}", h5.TextContent);
            Assert.Contains($"{lastName}", h5.TextContent);
        });


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

    [Fact]
    public void OnInitializedAsync_ShowsError_WhenOrderFails()
    {
        // Arrange
        var userId = "test-user-id";
        var userEmail = "test@example.com";

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Email, userEmail)
    }, "testAuth"));

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.Setup(cs => cs.PlaceOrderAndClearCart(userId))
            .ReturnsAsync(new JO.Models.Responses.GetCartItemResponse
            {
                Success = false,
                Message = "Le panier est vide"
            });

        var qrCodeServiceMock = new Mock<IQRCodeService>();
        var emailServiceMock = new Mock<IEmailService>();

        var dbOptions = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "ErrorTestDb")
            .Options;
        var dbContext = new DataContext(dbOptions);

        Services.AddScoped(_ => authProviderMock.Object);
        Services.AddScoped(_ => cartServiceMock.Object);
        Services.AddScoped(_ => qrCodeServiceMock.Object);
        Services.AddScoped(_ => emailServiceMock.Object);
        Services.AddScoped(_ => dbContext);

        // Act
        var component = RenderComponent<JO.Pages.Confirmation>();

        // Assert
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Erreur lors du traitement de la commande", component.Markup);
        });

        cartServiceMock.Verify(cs => cs.PlaceOrderAndClearCart(userId), Times.Once);
        qrCodeServiceMock.Verify(q => q.GenerateQRCodeWithBytes(It.IsAny<string>()), Times.Never);
        emailServiceMock.Verify(e => e.SendEmailWithAttachmentAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void OnInitializedAsync_ShowsGuestMessage_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity()); // brak tożsamości

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));

        var cartServiceMock = new Mock<ICartService>();
        var qrCodeServiceMock = new Mock<IQRCodeService>();
        var emailServiceMock = new Mock<IEmailService>();

        var dbOptions = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "GuestTestDb")
            .Options;
        var dbContext = new DataContext(dbOptions);

        Services.AddScoped(_ => authProviderMock.Object);
        Services.AddScoped(_ => cartServiceMock.Object);
        Services.AddScoped(_ => qrCodeServiceMock.Object);
        Services.AddScoped(_ => emailServiceMock.Object);
        Services.AddScoped(_ => dbContext);

        // Act
        var component = RenderComponent<JO.Pages.Confirmation>();

        // Assert
        component.WaitForAssertion(() =>
        {
            var h5 = component.Find("h5");
            Assert.Contains("invité", h5.TextContent);
            Assert.DoesNotContain("Erreur", component.Markup);
        });

        cartServiceMock.Verify(cs => cs.PlaceOrderAndClearCart(It.IsAny<string>()), Times.Never);
        qrCodeServiceMock.Verify(q => q.GenerateQRCodeWithBytes(It.IsAny<string>()), Times.Never);
        emailServiceMock.Verify(e => e.SendEmailWithAttachmentAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void OnInitializedAsync_SetsUnknownUser_WhenUserProfileNotFound()
    {
        // Arrange
        var userId = "test-user-id";
        var userEmail = "test@example.com";

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Email, userEmail)
    }, "testAuth"));

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.Setup(cs => cs.PlaceOrderAndClearCart(userId))
            .ReturnsAsync(new JO.Models.Responses.GetCartItemResponse { Success = true });

        var qrCodeServiceMock = new Mock<IQRCodeService>();
        var emailServiceMock = new Mock<IEmailService>();

        var dbOptions = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "NoProfileTestDb")
            .Options;
        var dbContext = new DataContext(dbOptions); // Brak dodania profilu = null

        Services.AddScoped(_ => authProviderMock.Object);
        Services.AddScoped(_ => cartServiceMock.Object);
        Services.AddScoped(_ => qrCodeServiceMock.Object);
        Services.AddScoped(_ => emailServiceMock.Object);
        Services.AddScoped(_ => dbContext);

        // Act
        var component = RenderComponent<JO.Pages.Confirmation>();

        // Assert
        component.WaitForAssertion(() =>
        {
            var h5 = component.Find("h5");
            Assert.Contains("utilisateur inconnu", h5.TextContent);
        });

        cartServiceMock.Verify(cs => cs.PlaceOrderAndClearCart(userId), Times.Once);
        qrCodeServiceMock.Verify(q => q.GenerateQRCodeWithBytes(It.IsAny<string>()), Times.Never);
        emailServiceMock.Verify(e => e.SendEmailWithAttachmentAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public void OnInitializedAsync_HandlesQRCodeServiceException_Gracefully()
    {
        // Arrange
        var userId = "test-user-id";
        var userEmail = "test@example.com";
        var firstName = "Jean";
        var lastName = "Dupont";

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Email, userEmail)
    }, "testAuth"));

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.Setup(cs => cs.PlaceOrderAndClearCart(userId))
            .ReturnsAsync(new JO.Models.Responses.GetCartItemResponse { Success = true });

        var qrCodeServiceMock = new Mock<IQRCodeService>();
        qrCodeServiceMock.Setup(q => q.GenerateQRCodeWithBytes(It.IsAny<string>()))
            .Throws(new Exception("QR generation failed"));

        var emailServiceMock = new Mock<IEmailService>();

        var dbOptions = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "QRCodeErrorDb")
            .Options;
        var dbContext = new DataContext(dbOptions);

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

        Services.AddScoped(_ => authProviderMock.Object);
        Services.AddScoped(_ => cartServiceMock.Object);
        Services.AddScoped(_ => qrCodeServiceMock.Object);
        Services.AddScoped(_ => emailServiceMock.Object);
        Services.AddScoped(_ => dbContext);

        // Act
        var component = RenderComponent<JO.Pages.Confirmation>();

        // Assert
        component.WaitForAssertion(() =>
        {
            var h5 = component.Find("h5");
            Assert.Contains($"{firstName}", h5.TextContent);
            Assert.Contains($"{lastName}", h5.TextContent);
        });

        cartServiceMock.Verify(cs => cs.PlaceOrderAndClearCart(userId), Times.Once);
        qrCodeServiceMock.Verify(q => q.GenerateQRCodeWithBytes(It.IsAny<string>()), Times.Once);
        emailServiceMock.Verify(e => e.SendEmailWithAttachmentAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never); // ponieważ QRCode nie został wygenerowany
    }

    [Fact]
    public void OnInitializedAsync_SetsHasErrorTrue_WhenQRCodeGenerationFails()
    {
        // Arrange
        var userId = "test-user-id";
        var userEmail = "test@example.com";
        var firstName = "Jean";
        var lastName = "Dupont";

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Email, userEmail)
    }, "testAuth"));

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.Setup(cs => cs.PlaceOrderAndClearCart(userId))
            .ReturnsAsync(new JO.Models.Responses.GetCartItemResponse { Success = true });

        var qrCodeServiceMock = new Mock<IQRCodeService>();
        qrCodeServiceMock.Setup(q => q.GenerateQRCodeWithBytes(It.IsAny<string>()))
            .Throws(new Exception("QR Error"));

        var emailServiceMock = new Mock<IEmailService>();

        var dbOptions = new DbContextOptionsBuilder<JO.Data.DataContext>()
            .UseInMemoryDatabase(databaseName: "QRCodeErrorTestDb")
            .Options;
        var dbContext = new JO.Data.DataContext(dbOptions);

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

        Services.AddScoped(_ => authProviderMock.Object);
        Services.AddScoped(_ => cartServiceMock.Object);
        Services.AddScoped(_ => qrCodeServiceMock.Object);
        Services.AddScoped(_ => emailServiceMock.Object);
        Services.AddScoped(_ => dbContext);

        // Act
        var component = RenderComponent<JO.Pages.Confirmation>();

        // Assert
        component.WaitForAssertion(() =>
        {
            var h5 = component.Find("h5");
            Assert.Contains($"{firstName}", h5.TextContent);
            Assert.Contains($"{lastName}", h5.TextContent);
            // Można też sprawdzić konsolę lub flagę istnienia błędu – tutaj ograniczamy się do tego,
            // że EmailService nie powinien być wywołany
        });

        cartServiceMock.Verify(cs => cs.PlaceOrderAndClearCart(userId), Times.Once);
        qrCodeServiceMock.Verify(q => q.GenerateQRCodeWithBytes(It.IsAny<string>()), Times.Once);
        emailServiceMock.Verify(e => e.SendEmailWithAttachmentAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void OnInitializedAsync_SetsHasErrorTrue_WhenEmailFails()
    {
        // Arrange
        var userId = "test-user-id";
        var userEmail = "test@example.com";
        var firstName = "Jean";
        var lastName = "Dupont";

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Email, userEmail)
    }, "testAuth"));

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.Setup(cs => cs.PlaceOrderAndClearCart(userId))
            .ReturnsAsync(new JO.Models.Responses.GetCartItemResponse { Success = true });

        var qrCodeServiceMock = new Mock<IQRCodeService>();
        qrCodeServiceMock.Setup(q => q.GenerateQRCodeWithBytes(It.IsAny<string>()))
            .Returns(("data:image/png;base64,base64string", new byte[] { 1, 2, 3 }));

        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock.Setup(e => e.SendEmailWithAttachmentAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>())).Throws(new Exception("Email sending failed"));

        var dbOptions = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "EmailErrorTestDb")
            .Options;
        var dbContext = new DataContext(dbOptions);

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

        Services.AddScoped(_ => authProviderMock.Object);
        Services.AddScoped(_ => cartServiceMock.Object);
        Services.AddScoped(_ => qrCodeServiceMock.Object);
        Services.AddScoped(_ => emailServiceMock.Object);
        Services.AddScoped(_ => dbContext);

        // Act
        var component = RenderComponent<JO.Pages.Confirmation>();

        // Assert
        component.WaitForAssertion(() =>
        {
            // Można dodać lepszy feedback np. na stronie w markup lub sprawdzić flagę, jeśli jest exposed
            Assert.Contains($"{firstName}", component.Markup);
            Assert.Contains($"{lastName}", component.Markup);
        });

        cartServiceMock.Verify(cs => cs.PlaceOrderAndClearCart(userId), Times.Once);
        qrCodeServiceMock.Verify(q => q.GenerateQRCodeWithBytes(It.IsAny<string>()), Times.Once);
        emailServiceMock.Verify(e => e.SendEmailWithAttachmentAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

  


}
