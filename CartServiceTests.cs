using Moq;
using Microsoft.EntityFrameworkCore;
using JO.Services;
using JO.Data;
using JO.Models;
using JO.Models.Responses;
using Xunit;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

public class CartServiceTests
{
    private readonly CartService _cartService;
private readonly DataContext _context;

public CartServiceTests()
    {


        var options = new DbContextOptionsBuilder<DataContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
    .Options;


        _context = new DataContext(options);
        _cartService = new CartService(_context);

       
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

     
        _context.Offers.AddRange(new List<Offer>
{
    new Offer { Name = "Offer 1", TicketCount = 10, Price = 100, Description = "Description 1", ImageUrl = "Image1" },
    new Offer { Name = "Offer 2", TicketCount = 20, Price = 150, Description = "Description 2", ImageUrl = "Image2" }
});

        _context.SaveChanges();  
    }



    [Fact]
    public async Task AddOfferToCart_Should_Add_Offer_To_Cart_When_Offer_Exists()
    {
        // Arrange
        var userId = "user1";
        var offerId = 1;
        var quantity = 2;

        // Act
        var result = await _cartService.AddOfferToCart(userId, offerId, quantity);

        // Assert
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        Assert.NotNull(cart);
        Assert.Single(cart.CartItems);
        Assert.Equal(quantity, cart.CartItems.First().Qty);
        Assert.True(result.Success);
        Assert.Equal("L'offre a été ajoutée au panier.", result.Message);
    }

    [Fact]
    public async Task AddOfferToCart_Should_Return_Failure_When_Offer_Not_Found()
    {
        // Arrange
        var userId = "user1";
        var offerId = 999;  // Non-existing offer
        var quantity = 2;

        // Act
        var result = await _cartService.AddOfferToCart(userId, offerId, quantity);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("L'offre n'a pas été trouvée.", result.Message);
    }

    [Fact]
    public async Task RemoveOfferFromCart_Should_Remove_Offer_When_Exists()
    {
        // Arrange
        var userId = "user1";
        var offerId = 1;
        var quantity = 3;

        
        var cart = new Cart { UserId = userId };
        var cartItem = new CartItem { CartId = cart.Id, OfferId = offerId, Qty = quantity };
        cart.CartItems.Add(cartItem);

        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.RemoveOfferFromCart(userId, offerId);

        // Assert
        var updatedCart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        Assert.True(result.Success);
        Assert.Equal("L'offre a été réduite de 1 ou supprimée du panier.", result.Message);
        Assert.Single(updatedCart.CartItems);  
    }
    [Fact]
    public async Task UpdateOfferQuantity_Should_Return_Failure_When_Offer_Quantity_Exceeds_Available()
    {
        // Arrange
        var userId = "user1";
        var offerId = 1;
        var newQty = 20;  

        
        var offer = await _context.Offers.FindAsync(offerId);
        offer.TicketCount = 10;
        await _context.SaveChangesAsync();

       
        var cart = new Cart { UserId = userId };
        var cartItem = new CartItem { CartId = cart.Id, OfferId = offerId, Qty = 1 };
        cart.CartItems.Add(cartItem);

        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.UpdateOfferQuantity(userId, offerId, newQty);

        var updatedOffer = await _context.Offers.FindAsync(offerId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal($"Vous ne pouvez pas définir cette quantité. Quantité maximale disponible : {updatedOffer.TicketCount}", result.Message);
    }

    [Fact]
    public async Task UpdateOfferQuantity_Should_Update_Quantity_When_Valid()
    {
        // Arrange
        var userId = "user1";
        var offerId = 1;
        var newQty = 5;

        var offer = await _context.Offers.FindAsync(offerId);
        offer.TicketCount = 10;
        await _context.SaveChangesAsync();

        var cart = new Cart { UserId = userId };
        var cartItem = new CartItem { CartId = cart.Id, OfferId = offerId, Qty = 1 };
        cart.CartItems.Add(cartItem);

        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.UpdateOfferQuantity(userId, offerId, newQty);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("La quantité a été mise à jour.", result.Message);
        var updatedCart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        Assert.Equal(newQty, updatedCart.CartItems.First().Qty);
    }

    [Fact]
    public async Task UpdateOfferQuantity_Should_Remove_Item_When_Quantity_Is_Zero()
    {
        // Arrange
        var userId = "user1";
        var offerId = 1;
        var newQty = 0;

        var cart = new Cart { UserId = userId };
        var cartItem = new CartItem { CartId = cart.Id, OfferId = offerId, Qty = 2 };
        cart.CartItems.Add(cartItem);

        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.UpdateOfferQuantity(userId, offerId, newQty);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("L'article a été supprimé du panier.", result.Message);
        var updatedCart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        Assert.DoesNotContain(updatedCart.CartItems, ci => ci.OfferId == offerId);
    }

    [Fact]
    public async Task UpdateOfferQuantity_Should_Return_Failure_When_Cart_Not_Found()
    {
        // Arrange
        var userId = "nonexistent_user";
        var offerId = 1;
        var newQty = 2;

        // Act
        var result = await _cartService.UpdateOfferQuantity(userId, offerId, newQty);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Le panier n'a pas été trouvé.", result.Message);
    }

    [Fact]
    public async Task UpdateOfferQuantity_Should_Return_Failure_When_CartItem_Not_Found()
    {
        // Arrange
        var userId = "user1";
        var offerId = 1;
        var newQty = 2;

        var cart = new Cart { UserId = userId };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.UpdateOfferQuantity(userId, offerId, newQty);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("L'offre n'a pas été trouvée dans le panier.", result.Message);
    }

    [Fact]
    public async Task PlaceOrderAndClearCart_Should_Create_Order_And_Clear_Cart()
    {
        // Arrange
        var userId = "user1";
        var offerId = 1;

        var cart = new Cart { UserId = userId };
        cart.CartItems.Add(new CartItem { OfferId = offerId, Qty = 2 });

        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.PlaceOrderAndClearCart(userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("La commande a été passée et le panier a été vidé.", result.Message);

        var order = await _context.Orders.Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.UserId == userId);
        Assert.NotNull(order);
        Assert.Single(order.OrderItems);

        var clearedCart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        Assert.Null(clearedCart);

        var offer = await _context.Offers.FirstOrDefaultAsync(o => o.Id == offerId);
        Assert.Equal(8, offer.TicketCount); 
    }


    [Fact]
    public async Task AddOfferToCart_Should_Return_Failure_When_Quantity_Exceeds_Available()
    {
        // Arrange
        var userId = "user1";
        var offerId = 1;

        var offer = await _context.Offers.FindAsync(offerId);
        offer.TicketCount = 2;
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.AddOfferToCart(userId, offerId, 5);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Quantité disponible", result.Message);
    }

    [Fact]
    public async Task GetCartItemCount_Should_Return_Correct_Total()
    {
        // Arrange
        var userId = "user1";
        var cart = new Cart { UserId = userId };
        cart.CartItems.Add(new CartItem { OfferId = 1, Qty = 2 });
        cart.CartItems.Add(new CartItem { OfferId = 2, Qty = 3 });

        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        // Act
        var count = await _cartService.GetCartItemCount(userId);

        // Assert
        Assert.Equal(5, count);
    }

    [Fact]
    public async Task GetCartItemCount_Should_Return_Zero_When_Cart_Does_Not_Exist()
    {
        // Arrange
        var userId = "nonexistent";

        // Act
        var count = await _cartService.GetCartItemCount(userId);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task RemoveOfferFromCart_Should_Return_Failure_When_Cart_Not_Found()
    {
        // Arrange
        var userId = "nonexistent";
        var offerId = 1;

        // Act
        var result = await _cartService.RemoveOfferFromCart(userId, offerId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Le panier n'a pas été trouvé.", result.Message);
    }

    [Fact]
    public async Task RemoveOfferFromCart_Should_Return_Failure_When_Item_Not_Found()
    {
        // Arrange
        var userId = "user1";
        var cart = new Cart { UserId = userId };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.RemoveOfferFromCart(userId, 999);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("L'offre n'a pas été trouvée dans le panier.", result.Message);
    }

    [Fact]
    public async Task PlaceOrderAndClearCart_Should_Return_Failure_When_Cart_Empty()
    {
        // Arrange
        var userId = "user1";
        var cart = new Cart { UserId = userId };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.PlaceOrderAndClearCart(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Le panier est vide, il est impossible de passer une commande.", result.Message);
    }



}


