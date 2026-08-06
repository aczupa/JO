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
    .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unikalna baza
    .Options;


        _context = new DataContext(options);
        _cartService = new CartService(_context);

        // Zawsze upewniamy się, że baza jest czyszczona przed każdym testem
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Dodanie przykładowych ofert do bazy danych przed testami
        _context.Offers.AddRange(new List<Offer>
    {
        new Offer { Name = "Offer 1", TicketCount = 10, Price = 100, Description = "Description 1", ImageUrl = "Image1" },
        new Offer { Name = "Offer 2", TicketCount = 20, Price = 150, Description = "Description 2", ImageUrl = "Image2" }
    });

        _context.SaveChanges();  // Zapisanie danych do bazy
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

        // Dodanie oferty do koszyka
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
        Assert.Single(updatedCart.CartItems);  // Sprawdzenie, że oferta została usunięta lub ilość została zaktualizowana
    }
    [Fact]
    public async Task UpdateOfferQuantity_Should_Return_Failure_When_Offer_Quantity_Exceeds_Available()
    {
        // Arrange
        var userId = "user1";
        var offerId = 1;
        var newQty = 20;  // Większe niż dostępne bilety

        // Upewniamy się, że oferta ma TicketCount = 10 (przepisanie wartości na wszelki wypadek)
        var offer = await _context.Offers.FindAsync(offerId);
        offer.TicketCount = 10;
        await _context.SaveChangesAsync();

        // Dodanie oferty do koszyka
        var cart = new Cart { UserId = userId };
        var cartItem = new CartItem { CartId = cart.Id, OfferId = offerId, Qty = 1 };
        cart.CartItems.Add(cartItem);

        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.UpdateOfferQuantity(userId, offerId, newQty);

        // Pobranie aktualnej wartości TicketCount z bazy
        var updatedOffer = await _context.Offers.FindAsync(offerId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal($"Vous ne pouvez pas définir cette quantité. Quantité maximale disponible : {updatedOffer.TicketCount}", result.Message);
    }

}
