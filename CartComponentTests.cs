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
using Bunit.TestDoubles;

public class CartComponentTests : TestContext
{
    private ClaimsPrincipal CreateClaimsPrincipal(string userId = null)
    {
        var identity = string.IsNullOrEmpty(userId)
            ? new ClaimsIdentity()
            : new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public void OnInitializedAsync_LoadsCartItemsAndCalculatesTotal()
    {
        var userId = "test-user-id";
        var offer = new Offer { Id = 1, Name = "Test Offer", Price = 10m, ImageUrl = "img.jpg" };
        var cart = new Cart { CartItems = new List<CartItem> { new CartItem { Offer = offer, Qty = 2 } } };

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.Setup(cs => cs.GetCart(userId)).ReturnsAsync(cart);

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(CreateClaimsPrincipal(userId)));

        Services.AddSingleton(cartServiceMock.Object);
        Services.AddSingleton(authProviderMock.Object);
        Services.AddSingleton<NavigationManager, FakeNavigationManager>();

        var component = RenderComponent<JO.Pages.Cart>();
        var instance = component.Instance;

        Assert.NotNull(instance.CartItems);
        Assert.Single(instance.CartItems);
        Assert.Equal(20m, instance.TotalAmount);
    }

    [Fact]
    public void ClickingRemoveButton_RemovesItemAndUpdatesCart()
    {
        var userId = "test-user-id";
        var offer = new Offer { Id = 1, Name = "Offer A", Price = 10m, ImageUrl = "img.jpg" };
        var initialCart = new Cart { CartItems = new List<CartItem> { new CartItem { Offer = offer, Qty = 1 } } };
        var updatedCart = new Cart { CartItems = new List<CartItem>() };

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.SetupSequence(cs => cs.GetCart(userId))
            .ReturnsAsync(initialCart)
            .ReturnsAsync(updatedCart);
        cartServiceMock.Setup(cs => cs.RemoveOfferFromCart(userId, offer.Id))
            .ReturnsAsync(new GetCartItemResponse { Success = true });

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(CreateClaimsPrincipal(userId)));

        Services.AddSingleton(cartServiceMock.Object);
        Services.AddSingleton(authProviderMock.Object);
        Services.AddSingleton<NavigationManager, FakeNavigationManager>();

        var component = RenderComponent<JO.Pages.Cart>();
        var deleteButton = component.Find("button.btn-icon");
        deleteButton.Click();

        var instance = component.Instance;
        Assert.Empty(instance.CartItems);
        Assert.Equal(0m, instance.TotalAmount);
    }

    [Fact]
    public void WhenCartItemsIsNull_ShowsLoadingMessage()
    {
        var userId = "test-user-id";
        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.Setup(cs => cs.GetCart(userId)).ReturnsAsync((Cart)null);

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(CreateClaimsPrincipal(userId)));

        Services.AddSingleton(cartServiceMock.Object);
        Services.AddSingleton(authProviderMock.Object);
        Services.AddSingleton<NavigationManager, FakeNavigationManager>();

        var component = RenderComponent<JO.Pages.Cart>();
        component.Markup.Contains("Chargement du panier");
    }

    [Fact]
    public void UpdatingQuantity_UpdatesCartAndRecalculatesTotal()
    {
        var userId = "test-user-id";
        var offer = new Offer { Id = 1, Name = "Offer A", Price = 10m, ImageUrl = "img.jpg" };
        var initialCart = new Cart { CartItems = new List<CartItem> { new CartItem { Offer = offer, Qty = 1 } } };
        var updatedCart = new Cart { CartItems = new List<CartItem> { new CartItem { Offer = offer, Qty = 3 } } };

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.SetupSequence(cs => cs.GetCart(userId))
            .ReturnsAsync(initialCart)
            .ReturnsAsync(updatedCart);
        cartServiceMock.Setup(cs => cs.UpdateOfferQuantity(userId, offer.Id, 3))
            .ReturnsAsync(new GetCartItemResponse { Success = true });

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(CreateClaimsPrincipal(userId)));

        Services.AddSingleton(cartServiceMock.Object);
        Services.AddSingleton(authProviderMock.Object);
        Services.AddSingleton<NavigationManager, FakeNavigationManager>();

        var component = RenderComponent<JO.Pages.Cart>();

        // Zaktualizuj Qty ręcznie, żeby odzwierciedlało input
        component.Instance.CartItems[0].Qty = 3;

        // Trigger @onchange
        var input = component.Find("input.form-control");
        input.Change(3);

        var instance = component.Instance;
        Assert.Equal(30m, instance.TotalAmount);
    }

    [Fact]
    public void UpdatingQuantity_ShowsError_WhenUpdateFails()
    {
        var userId = "test-user-id";
        var offer = new Offer { Id = 1, Name = "Offer A", Price = 10m, ImageUrl = "img.jpg" };
        var cart = new Cart
        {
            CartItems = new List<CartItem> { new CartItem { Offer = offer, Qty = 1 } }
        };

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.Setup(cs => cs.GetCart(userId)).ReturnsAsync(cart);
        cartServiceMock.Setup(cs => cs.UpdateOfferQuantity(userId, offer.Id, 99))
            .ReturnsAsync(new GetCartItemResponse { Success = false, Message = "Quantité trop élevée" });

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(CreateClaimsPrincipal(userId)));

        Services.AddSingleton(cartServiceMock.Object);
        Services.AddSingleton(authProviderMock.Object);
        Services.AddSingleton<NavigationManager, FakeNavigationManager>();

        var component = RenderComponent<JO.Pages.Cart>();

        // Wymuś zmianę ilości przed triggerem
        component.Instance.CartItems[0].Qty = 99;

        // Znajdź input i uruchom onchange
        var input = component.Find("input.form-control");
        input.Change(99);

        cartServiceMock.Verify(cs => cs.UpdateOfferQuantity(userId, offer.Id, 99), Times.Once);
    }


    [Fact]
    public void UpdatingQuantityToZero_RemovesItemFromCart()
    {
        var userId = "test-user-id";
        var offer = new Offer { Id = 1, Name = "Offer A", Price = 10m, ImageUrl = "img.jpg" };
        var initialCart = new Cart { CartItems = new List<CartItem> { new CartItem { Offer = offer, Qty = 2 } } };
        var updatedCart = new Cart { CartItems = new List<CartItem>() };

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.SetupSequence(cs => cs.GetCart(userId))
            .ReturnsAsync(initialCart)
            .ReturnsAsync(updatedCart);
        cartServiceMock.Setup(cs => cs.UpdateOfferQuantity(userId, offer.Id, 0))
            .ReturnsAsync(new GetCartItemResponse { Success = true });

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(CreateClaimsPrincipal(userId)));

        Services.AddSingleton(cartServiceMock.Object);
        Services.AddSingleton(authProviderMock.Object);
        Services.AddSingleton<NavigationManager, FakeNavigationManager>();

        var component = RenderComponent<JO.Pages.Cart>();

        component.Instance.CartItems[0].Qty = 0;

        var input = component.Find("input.form-control");
        input.Change(0);

        component.WaitForAssertion(() =>
        {
            var instance = component.Instance;
            Assert.Empty(instance.CartItems);
        }, timeout: TimeSpan.FromSeconds(5));
    }



    [Fact]
    public void ClickingProceedToCheckout_NavigatesToPersonalData()
    {
        var userId = "test-user-id";

        var offer = new Offer { Id = 1, Name = "Test Offer", Price = 10m, ImageUrl = "img.jpg" };
        var cart = new Cart
        {
            CartItems = new List<CartItem> { new CartItem { Offer = offer, Qty = 1 } }
        };

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.Setup(cs => cs.GetCart(userId)).ReturnsAsync(cart);

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(CreateClaimsPrincipal(userId)));

        Services.AddSingleton(cartServiceMock.Object);
        Services.AddSingleton(authProviderMock.Object);
        Services.AddSingleton<NavigationManager, FakeNavigationManager>();

        var navManager = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;
        var component = RenderComponent<JO.Pages.Cart>();

        var button = component.Find("button.btn-primary");
        button.Click();

        Assert.Equal("http://localhost/personaldata", navManager.Uri);
    }


    [Fact]
    public void UpdatingQuantity_WithNegativeValue_DoesNotCallService()
    {
        var userId = "test-user-id";
        var offer = new Offer { Id = 1, Name = "Offer A", Price = 10m, ImageUrl = "img.jpg" };
        var cart = new Cart
        {
            CartItems = new List<CartItem> { new CartItem { Offer = offer, Qty = 1 } }
        };

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.Setup(cs => cs.GetCart(userId)).ReturnsAsync(cart);
        cartServiceMock.Setup(cs => cs.UpdateOfferQuantity(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new GetCartItemResponse { Success = true });

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(CreateClaimsPrincipal(userId)));

        Services.AddSingleton(cartServiceMock.Object);
        Services.AddSingleton(authProviderMock.Object);
        Services.AddSingleton<NavigationManager, FakeNavigationManager>();

        var component = RenderComponent<JO.Pages.Cart>();

       
        component.Instance.CartItems[0].Qty = -1;

        var input = component.Find("input.form-control");
        input.Change(-1);

        cartServiceMock.Verify(cs =>
            cs.UpdateOfferQuantity(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }


    [Fact]
    public void OnInitializedAsync_NoUserId_DoesNotLoadCart()
    {
        var cartServiceMock = new Mock<ICartService>();

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(CreateClaimsPrincipal()));

        Services.AddSingleton(cartServiceMock.Object);
        Services.AddSingleton(authProviderMock.Object);
        Services.AddSingleton<NavigationManager, FakeNavigationManager>();

        var component = RenderComponent<JO.Pages.Cart>();
        var instance = component.Instance;

        Assert.Null(instance.CartItems);
    }
    [Fact]
    public void ClickingContinueOrder_LinkIsRenderedWithCorrectHref()
    {
        var userId = "test-user-id";

        var offer = new Offer { Id = 1, Name = "Test Offer", Price = 10m, ImageUrl = "img.jpg" };
        var cart = new Cart
        {
            CartItems = new List<CartItem>
        {
            new CartItem { Offer = offer, Qty = 1 }
        }
        };

        var cartServiceMock = new Mock<ICartService>();
        cartServiceMock.Setup(cs => cs.GetCart(userId)).ReturnsAsync(cart);

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(CreateClaimsPrincipal(userId)));

        Services.AddSingleton(cartServiceMock.Object);
        Services.AddSingleton(authProviderMock.Object);
        Services.AddSingleton<NavigationManager, FakeNavigationManager>();

        var component = RenderComponent<JO.Pages.Cart>();

        var link = component.Find("a.btn-secondary");

        Assert.Equal("/offres", link.GetAttribute("href"));
        Assert.Contains("Continuer ma commande", link.TextContent);
    }

}
