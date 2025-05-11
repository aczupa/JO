using Bunit;
using Moq;
using Xunit;
using JO.Services;
using JO.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using JO.Models.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Bunit.TestDoubles;


namespace JO.Tests
{
    public class OffresPageTests : TestContext
    {
        private readonly Mock<IOfferService> _offerServiceMock = new();
        private readonly Mock<ICartService> _cartServiceMock = new();
        private readonly Mock<AuthenticationStateProvider> _authProviderMock = new();
        private readonly Mock<NavigationManager> _navigationManagerMock = new();

        private readonly List<Offer> _mockOffers = new()
        {
            new Offer { Id = 1, Name = "Solo", Description = "1 billet", Price = 100, ImageUrl = "/img/solo.jpg" },
            new Offer { Id = 2, Name = "Duo", Description = "2 billets", Price = 180, ImageUrl = "/img/duo.jpg" },
        };

        private AuthenticationState GetAuthState(bool isAuthenticated = true)
        {
            var identity = isAuthenticated
                ? new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "user1"),
                    new Claim(ClaimTypes.Name, "testuser")
                }, "TestAuth")
                : new ClaimsIdentity();

            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        [Fact]
        public void Displays_Offers_When_Available()
        {
            // Arrange
            _offerServiceMock.Setup(x => x.GetOffers())
                .ReturnsAsync(new GetOffersResponse { Offers = _mockOffers });

            _authProviderMock.Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(GetAuthState());

            _cartServiceMock.Setup(x => x.GetCartItemCount("user1"))
                .ReturnsAsync(2);

            Services.AddSingleton(_offerServiceMock.Object);
            Services.AddSingleton(_cartServiceMock.Object);
            Services.AddSingleton(_authProviderMock.Object);


            // Act
            var cut = RenderComponent<JO.Pages.Offres>(); // Upewnij się, że masz namespace zgodny z projektem

            // Assert
            Assert.Contains("Solo", cut.Markup);
            Assert.Contains("Duo", cut.Markup);
        }

        [Fact]
        public void Redirects_To_Login_When_AddToCart_Clicked_And_NotAuthenticated()
        {
            // Arrange
            _offerServiceMock.Setup(x => x.GetOffers())
                .ReturnsAsync(new GetOffersResponse { Offers = _mockOffers });

            _authProviderMock.Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(GetAuthState(false)); // user is NOT authenticated

            Services.AddSingleton(_offerServiceMock.Object);
            Services.AddSingleton(_cartServiceMock.Object);
            Services.AddSingleton(_authProviderMock.Object);

            var navMan = Services.GetRequiredService<FakeNavigationManager>();

            var cut = RenderComponent<JO.Pages.Offres>();

            // Act
            cut.Find("button").Click(); // Click first AddToCart

            // Assert
            Assert.Contains("/Identity/Account/Login", navMan.Uri);
        }

        [Fact]
        public void Shows_Cart_Button_When_LoggedIn()
        {
            // Arrange
            _offerServiceMock.Setup(x => x.GetOffers())
                .ReturnsAsync(new GetOffersResponse { Offers = _mockOffers });

            _authProviderMock.Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(GetAuthState(true));

            _cartServiceMock.Setup(x => x.GetCartItemCount("user1"))
                .ReturnsAsync(3);

            Services.AddSingleton(_offerServiceMock.Object);
            Services.AddSingleton(_cartServiceMock.Object);
            Services.AddSingleton(_authProviderMock.Object);


            // Act
            var cut = RenderComponent<JO.Pages.Offres>();

            // Assert
            Assert.Contains("Panier", cut.Markup);
            Assert.Contains("3", cut.Markup);
        }

        [Fact]
        public async Task Adds_Item_To_Cart_When_Clicked_And_Authenticated()
        {
            // Arrange
            _offerServiceMock.Setup(x => x.GetOffers())
                .ReturnsAsync(new GetOffersResponse { Offers = _mockOffers });

            _authProviderMock.Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(GetAuthState());

            _cartServiceMock.Setup(x => x.GetCartItemCount("user1"))
                .ReturnsAsync(0);

            _cartServiceMock.Setup(x => x.AddOfferToCart("user1", 1, 1))
                .ReturnsAsync(new GetCartItemResponse { Success = true, Message = "Added" });

            Services.AddSingleton(_offerServiceMock.Object);
            Services.AddSingleton(_cartServiceMock.Object);
            Services.AddSingleton(_authProviderMock.Object);


            var cut = RenderComponent<JO.Pages.Offres>();

            // Act
            var button = cut.Find("button");
            await button.ClickAsync(new());

            _cartServiceMock.Verify(x => x.AddOfferToCart("user1", 1, 1), Times.Once);
        }
    

    [Fact]
        public void Should_Display_Cart_Count_After_Add()
        {
            _offerServiceMock.Setup(x => x.GetOffers())
                .ReturnsAsync(new GetOffersResponse { Offers = _mockOffers });

            _authProviderMock.Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(GetAuthState());

            _cartServiceMock.SetupSequence(x => x.GetCartItemCount("user1"))
                .ReturnsAsync(0)  // before add
                .ReturnsAsync(1); // after add

            _cartServiceMock.Setup(x => x.AddOfferToCart("user1", 1, 1))
                .ReturnsAsync(new GetCartItemResponse { Success = true, Message = "Added" });

            Services.AddSingleton(_offerServiceMock.Object);
            Services.AddSingleton(_cartServiceMock.Object);
            Services.AddSingleton(_authProviderMock.Object);

            var cut = RenderComponent<JO.Pages.Offres>();

            var button = cut.Find("button");
            button.Click();

          
            _cartServiceMock.Verify(x => x.AddOfferToCart("user1", 1, 1), Times.Once);
        }

        [Fact]
        public void Should_Display_Login_If_NotAuthenticated()
        {
            _offerServiceMock.Setup(x => x.GetOffers())
                .ReturnsAsync(new GetOffersResponse { Offers = _mockOffers });

            _authProviderMock.Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(GetAuthState(false)); // user not authenticated

            Services.AddSingleton(_offerServiceMock.Object);
            Services.AddSingleton(_cartServiceMock.Object);
            Services.AddSingleton(_authProviderMock.Object);

            var navMan = Services.GetRequiredService<FakeNavigationManager>();

            var cut = RenderComponent<JO.Pages.Offres>();

            var button = cut.Find("button");
            button.Click();

            Assert.Contains("/Identity/Account/Login", navMan.Uri);
        }

    }
}