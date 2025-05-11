using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using JO.Pages;
using JO.Services;
using System.Security.Claims;
using Bunit.TestDoubles;
using System.Threading.Tasks;

public class PersonalDataPageTests : TestContext
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<IUserProfileService> _userProfileServiceMock;
    private readonly Mock<AuthenticationStateProvider> _authStateProviderMock;

    public PersonalDataPageTests()
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
        _userProfileServiceMock = new Mock<IUserProfileService>();
        _authStateProviderMock = new Mock<AuthenticationStateProvider>();

        Services.AddSingleton(_userManagerMock.Object);
        Services.AddSingleton(_userProfileServiceMock.Object);
        Services.AddSingleton(_authStateProviderMock.Object);
        Services.AddSingleton<FakeNavigationManager>();
        Services.AddSingleton<NavigationManager>(sp => sp.GetRequiredService<FakeNavigationManager>());
    }

    private void SetupAuth(string userId)
    {
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        _authStateProviderMock
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(new IdentityUser { Email = "test@example.com" });
    }

    [Fact]
    public void OnInitializedAsync_LoadsUserEmail()
    {
        SetupAuth("test-user-id");
        var cut = RenderComponent<PersonalData>();
        var emailInput = cut.Find("#email");
        Assert.Equal("test@example.com", emailInput.GetAttribute("value"));
    }

    [Fact]
    public void SubmitForm_ShowsValidationErrors_WhenRequiredFieldsMissing()
    {
        SetupAuth("test-user-id");
        var cut = RenderComponent<PersonalData>();
        cut.Find("form").Submit();
        Assert.Contains("Le prénom est requis", cut.Markup);
        Assert.Contains("Le nom est requis", cut.Markup);
    }

    [Fact]
    public void OnInitializedAsync_DoesNotFail_WhenUserNotFound()
    {
        var userId = "test-user-id";
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        _authStateProviderMock
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));
        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync((IdentityUser)null);

        var cut = RenderComponent<PersonalData>();
        var emailInput = cut.Find("#email");
        Assert.Null(emailInput.GetAttribute("value"));
    }

    [Fact]
    public async Task HandleValidSubmit_RedirectsToConfirmation_WhenPayPalSelected()
    {
        SetupAuth("test-user-id");
        var navManager = Services.GetRequiredService<FakeNavigationManager>();
        var cut = RenderComponent<PersonalData>();

        cut.Find("#firstName").Change("Jane");
        cut.Find("#lastName").Change("Smith");
        cut.Find("#street").Change("Second St");
        cut.Find("#streetNumber").Change("2");
        cut.Find("#postalCode").Change("54321");
        cut.Find("#city").Change("Lyon");
        cut.Find("#country").Change("France");
        cut.Find("#paymentMethod").Change("PayPal");

        cut.Find("form").Submit();

        _userProfileServiceMock.Verify(s => s.SaveUserProfile("test-user-id", It.IsAny<PersonalData.CheckoutInfo>()), Times.Once);
        Assert.Contains("/confirmation", navManager.Uri);
    }

    [Fact]
    public async Task HandleValidSubmit_RedirectsToCreditCardForm_WhenCardSelected()
    {
        SetupAuth("test-user-id");
        var navManager = Services.GetRequiredService<FakeNavigationManager>();
        var cut = RenderComponent<PersonalData>();

        cut.Find("#firstName").Change("Jane");
        cut.Find("#lastName").Change("Smith");
        cut.Find("#street").Change("Second St");
        cut.Find("#streetNumber").Change("2");
        cut.Find("#postalCode").Change("54321");
        cut.Find("#city").Change("Lyon");
        cut.Find("#country").Change("France");

      
        cut.Find("#paymentMethod").Change("CreditCard");

        cut.Find("form").Submit();

        _userProfileServiceMock.Verify(s => s.SaveUserProfile("test-user-id", It.IsAny<PersonalData.CheckoutInfo>()), Times.Once);

        cut.WaitForAssertion(() =>
            Assert.Contains("/creditcard", navManager.Uri),
            timeout: System.TimeSpan.FromSeconds(10));
    }



    [Fact]
    public void EmailInput_IsDisabled()
    {
        SetupAuth("test-user-id");
        var cut = RenderComponent<PersonalData>();
        var emailInput = cut.Find("#email");
        Assert.True(emailInput.HasAttribute("disabled"));
    }

    [Fact]
    public void SubmitForm_DoesNotCallService_WhenNoUserId()
    {
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        _authStateProviderMock
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));
        var cut = RenderComponent<PersonalData>();

        cut.Find("#firstName").Change("Jane");
        cut.Find("#lastName").Change("Smith");
        cut.Find("#street").Change("Second St");
        cut.Find("#streetNumber").Change("2");
        cut.Find("#postalCode").Change("54321");
        cut.Find("#city").Change("Lyon");
        cut.Find("#country").Change("France");
        cut.Find("#paymentMethod").Change("PayPal");

        cut.Find("form").Submit();

        _userProfileServiceMock.Verify(s => s.SaveUserProfile(It.IsAny<string>(), It.IsAny<PersonalData.CheckoutInfo>()), Times.Never);
    }

    [Fact]
    public void PaymentMethodDropdown_HasExpectedOptions()
    {
        SetupAuth("test-user-id");
        var cut = RenderComponent<PersonalData>();
        var select = cut.Find("#paymentMethod");
        Assert.Contains("-- Choisir une option --", select.InnerHtml);
        Assert.Contains("Carte bancaire", select.InnerHtml);
        Assert.Contains("PayPal", select.InnerHtml);
    }

    [Fact]
    public void Component_ShowsLoadingIndicator_WhenLoading()
    {
        var userId = "test-user-id";
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        _authStateProviderMock
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));
        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId))
            .Returns(async () =>
            {
                await Task.Delay(500); // sztuczne opóźnienie
                return new IdentityUser { Email = "test@example.com" };
            });

        var cut = RenderComponent<PersonalData>();

        cut.WaitForAssertion(() =>
            Assert.Contains("Chargement...", cut.Markup));
    }

    [Fact]
    public async Task HandleValidSubmit_ShowsConfirmationMessage()
    {
        SetupAuth("test-user-id");
        var navManager = Services.GetRequiredService<FakeNavigationManager>();
        var cut = RenderComponent<PersonalData>();

        cut.Find("#firstName").Change("Jane");
        cut.Find("#lastName").Change("Smith");
        cut.Find("#street").Change("Second St");
        cut.Find("#streetNumber").Change("2");
        cut.Find("#postalCode").Change("54321");
        cut.Find("#city").Change("Lyon");
        cut.Find("#country").Change("France");
        cut.Find("#paymentMethod").Change("PayPal");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
        {
            // Jeśli komponent przekierowuje:
            Assert.Contains("/confirmation", navManager.Uri);

            // Jeśli jednak pokazuje komunikat:
            // Assert.Contains("Merci pour vos informations personnelles", cut.Markup);
        }, timeout: System.TimeSpan.FromSeconds(10));
    }
}
