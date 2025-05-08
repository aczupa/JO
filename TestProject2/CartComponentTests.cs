using Bunit;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using JO.Models;
using JO.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using JO.Models.Responses;

public class CartComponentTests : TestContext
{
    [Fact]
    public async Task OnInitializedAsync_LoadsCartItemsAndCalculatesTotal()
    {
        // Arrange
        var userId = "test-user-id";

        var offer = new Offer { Id = 1, Name = "Test Offer", Price = 10m, ImageUrl = "img.jpg" };
        var cartItem = new CartItem { Offer = offer, Qty = 2 };
        var cart = new Cart { CartItems = new List<CartItem> { cartItem } };

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.Setup(cs => cs.GetCart(userId)).ReturnsAsync(cart);

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));

        Services.AddSingleton(cartServiceMock.Object);
        Services.AddSingleton(authProviderMock.Object);
        Services.AddSingleton<NavigationManager>(new Mock<NavigationManager>().Object);

        // Act
        var component = RenderComponent<JO.Pages.Cart>();

        // Assert
        var instance = component.Instance;
        Assert.NotNull(instance);
        Assert.NotNull(instance.CartItems);
        Assert.Single(instance.CartItems);
        Assert.Equal(20m, instance.TotalAmount); // 2 * 10

        component.Markup.Contains("Votre panier");
        component.Markup.Contains("Test Offer");
    }

    [Fact]
    public async Task ClickingRemoveButton_RemovesItemAndUpdatesCart()
    {
        // Arrange
        var userId = "test-user-id";

        var offer = new Offer { Id = 1, Name = "Offer A", Price = 10m, ImageUrl = "img.jpg" };
        var initialCartItem = new CartItem { Offer = offer, Qty = 1 };
        var initialCart = new Cart { CartItems = new List<CartItem> { initialCartItem } };

        var updatedCart = new Cart { CartItems = new List<CartItem>() };

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.SetupSequence(cs => cs.GetCart(userId))
            .ReturnsAsync(initialCart)   // initial
            .ReturnsAsync(updatedCart);  // after removal

        cartServiceMock.Setup(cs => cs.RemoveOfferFromCart(userId, offer.Id))
            .ReturnsAsync(new GetCartItemResponse { Success = true });

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId)
    }, "TestAuth"));

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));

        Services.AddSingleton(cartServiceMock.Object);
        Services.AddSingleton(authProviderMock.Object);
        Services.AddSingleton<NavigationManager>(new Mock<NavigationManager>().Object);

        var component = RenderComponent<JO.Pages.Cart>();

        // Act
        var deleteButton = component.Find("button.btn-icon"); // przycisk usuwania
        deleteButton.Click();

        // Assert
        var instance = component.Instance;
        Assert.Empty(instance.CartItems);
        Assert.Equal(0m, instance.TotalAmount);
    }


    [Fact]
    public void WhenCartItemsIsNull_ShowsLoadingMessage()
    {
        // Arrange
        var cartServiceMock = new Mock<ICartService>();

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, "test-user-id")
    }, "TestAuth"));

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));

        cartServiceMock.Setup(cs => cs.GetCart(It.IsAny<string>())).ReturnsAsync((Cart)null);

        Services.AddSingleton(cartServiceMock.Object);
        Services.AddSingleton(authProviderMock.Object);
        Services.AddSingleton<NavigationManager>(new Mock<NavigationManager>().Object);

        var component = RenderComponent<JO.Pages.Cart>();

        // Assert
        component.Markup.Contains("Chargement du panier");
    }


}
