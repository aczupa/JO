using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using JO.Areas.Identity.Pages.Account;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class RegisterTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;

    public RegisterTests()
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
    public async Task OnPostAsync_Should_AddModelError_When_UserAlreadyExists()
    {
        // Arrange
        var model = new RegisterModel(_signInManagerMock.Object, _userManagerMock.Object)
        {
            Input = new RegisterModel.InputModel { Email = "test@example.com", Password = "Password123!" }
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(new IdentityUser());

        // Act
        var result = await model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
        Assert.Contains(model.ModelState, m => m.Value.Errors.Any(e => e.ErrorMessage.Contains("Un compte avec cet e-mail existe déjà")));
    }

    [Fact]
    public async Task OnPostAsync_Should_AddModelError_When_PasswordInvalid()
    {
        // Arrange
        var model = new RegisterModel(_signInManagerMock.Object, _userManagerMock.Object)
        {
            Input = new RegisterModel.InputModel { Email = "test@example.com", Password = "short" }
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync((IdentityUser)null);

        // Act
        var result = await model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
        Assert.Contains(model.ModelState, m => m.Value.Errors.Any(e => e.ErrorMessage.Contains("Le mot de passe doit contenir")));
    }

    [Fact]
    public async Task OnPostAsync_Should_CreateUser_And_SignIn_When_Success()
    {
        // Arrange
        var model = new RegisterModel(_signInManagerMock.Object, _userManagerMock.Object)
        {
            Input = new RegisterModel.InputModel { Email = "test@example.com", Password = "Password123!" }
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync((IdentityUser)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Success);

        _signInManagerMock.Setup(x => x.SignInAsync(It.IsAny<IdentityUser>(), false, null))
            .Returns(Task.CompletedTask);

        // Act
        var result = await model.OnPostAsync();

        // Assert
        var redirectResult = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("~/", redirectResult.Url);
    }

    [Fact]
    public async Task OnPostAsync_Should_ReturnPage_When_ModelStateInvalid()
    {
        // Arrange
        var model = new RegisterModel(_signInManagerMock.Object, _userManagerMock.Object);
        model.ModelState.AddModelError("Input.Email", "Required");

        // Act
        var result = await model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
    }
}
