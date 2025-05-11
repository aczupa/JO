using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using JO.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LoginModelTests
{
    private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;

    public LoginModelTests()
    {
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
            _userManagerMock.Object,
            Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
            null, null, null, null);
    }

    [Fact]
    public void OnGet_Should_Set_InfoMessage_When_LoginRequired()
    {
        // Arrange
        var model = new LoginModel(_signInManagerMock.Object, _userManagerMock.Object);

        // Act
        model.OnGet("login-required");

        // Assert
        Assert.Equal("Pour ajouter une offre au panier, vous devez d'abord vous connecter.", model.InfoMessage);
    }

    [Fact]
    public async Task OnPostAsync_Should_AddModelError_When_UserNotFound()
    {
        // Arrange
        var model = new LoginModel(_signInManagerMock.Object, _userManagerMock.Object)
        {
            Input = new InputModel { Email = "test@example.com", Password = "Password123!" }
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((IdentityUser)null);

        // Act
        var result = await model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
        Assert.True(model.ModelState.ErrorCount > 0);
    }

    [Fact]
    public async Task OnPostAsync_Should_RedirectToOffers_When_UserNotAdmin()
    {
        // Arrange
        var user = new IdentityUser { Email = "test@example.com" };
        var model = new LoginModel(_signInManagerMock.Object, _userManagerMock.Object)
        {
            Input = new InputModel { Email = user.Email, Password = "Password123!" }
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.PasswordSignInAsync(user.Email, It.IsAny<string>(), false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);

        // Act
        var result = await model.OnPostAsync();

        // Assert
        var redirectResult = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("~/offres", redirectResult.Url);
    }

    [Fact]
    public async Task OnPostAsync_Should_RedirectToOffersTable_When_UserIsAdmin()
    {
        // Arrange
        var user = new IdentityUser { Email = "admin@example.com" };
        var model = new LoginModel(_signInManagerMock.Object, _userManagerMock.Object)
        {
            Input = new InputModel { Email = user.Email, Password = "AdminPass!" }
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.PasswordSignInAsync(user.Email, It.IsAny<string>(), false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);

        // Act
        var result = await model.OnPostAsync();

        // Assert
        var redirectResult = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("~/offers-table", redirectResult.Url);
    }

    [Fact]
    public async Task OnPostAsync_Should_AddModelError_When_LoginFails()
    {
        // Arrange
        var user = new IdentityUser { Email = "test@example.com" };
        var model = new LoginModel(_signInManagerMock.Object, _userManagerMock.Object)
        {
            Input = new InputModel { Email = user.Email, Password = "WrongPassword" }
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.PasswordSignInAsync(user.Email, It.IsAny<string>(), false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        var result = await model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
        Assert.True(model.ModelState.ErrorCount > 0);
    }
}
