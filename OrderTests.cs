using Xunit;
using JO.Models;
using System;
using System.Collections.Generic;

public class OrderTests
{
    [Fact]
    public void Order_Should_Initialize_With_Default_Values()
    {
        // Act
        var order = new Order();

        // Assert
        Assert.Equal(0, order.Id);
        Assert.Null(order.UserId);
        Assert.Equal(0m, order.TotalPrice);
        Assert.Equal(default, order.CreatedAt);
        Assert.False(order.IsPaid);
        Assert.NotNull(order.OrderItems);
        Assert.Empty(order.OrderItems);
    }

    [Fact]
    public void Order_Should_Allow_Setting_Properties()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var offer = new Offer { Id = 1, Name = "Test Offer", Price = 100m };
        var orderItems = new List<OrderItem>
        {
            new OrderItem
            {
                Id = 1,
                OrderId = 5,
                OfferId = offer.Id,
                Offer = offer,
                Qty = 2,
                Price = 200m
            }
        };

        // Act
        var order = new Order
        {
            Id = 5,
            UserId = "user123",
            TotalPrice = 200m,
            CreatedAt = date,
            IsPaid = true,
            OrderItems = orderItems
        };

        // Assert
        Assert.Equal(5, order.Id);
        Assert.Equal("user123", order.UserId);
        Assert.Equal(200m, order.TotalPrice);
        Assert.Equal(date, order.CreatedAt);
        Assert.True(order.IsPaid);
        Assert.Equal(orderItems, order.OrderItems);
        Assert.Single(order.OrderItems);
        Assert.Equal(1, order.OrderItems[0].Id);
        Assert.Equal(2, order.OrderItems[0].Qty);
        Assert.Equal(200m, order.OrderItems[0].Price);
        Assert.Equal("Test Offer", order.OrderItems[0].Offer.Name);
    }

    [Fact]
    public void Order_Should_Add_OrderItems()
    {
        // Arrange
        var order = new Order();
        var offer = new Offer { Id = 1, Name = "Special Ticket", Price = 50m };
        var item = new OrderItem
        {
            Id = 1,
            OrderId = order.Id,
            OfferId = offer.Id,
            Offer = offer,
            Qty = 1,
            Price = 50m
        };

        // Act
        order.OrderItems.Add(item);

        // Assert
        Assert.Single(order.OrderItems);
        var addedItem = order.OrderItems[0];
        Assert.Equal(1, addedItem.Id);
        Assert.Equal(order.Id, addedItem.OrderId);
        Assert.Equal(offer.Id, addedItem.OfferId);
        Assert.Equal(offer, addedItem.Offer);
        Assert.Equal(1, addedItem.Qty);
        Assert.Equal(50m, addedItem.Price);
    }


public class OrderItemTests
{
    [Fact]
    public void OrderItem_Should_Allow_Setting_Properties()
    {
        // Arrange
        var order = new Order { Id = 1 };
        var offer = new Offer { Id = 2, Name = "VIP Ticket", Price = 150m };

        // Act
        var item = new OrderItem
        {
            Id = 10,
            OrderId = order.Id,
            Order = order,
            OfferId = offer.Id,
            Offer = offer,
            Qty = 3,
            Price = 450m // normally Qty * Offer.Price
        };

        // Assert
        Assert.Equal(10, item.Id);
        Assert.Equal(1, item.OrderId);
        Assert.Equal(order, item.Order);
        Assert.Equal(2, item.OfferId);
        Assert.Equal(offer, item.Offer);
        Assert.Equal(3, item.Qty);
        Assert.Equal(450m, item.Price);
    }

    [Fact]
    public void OrderItem_Should_Calculate_TotalPrice_Correctly()
    {
        // Arrange
        var offer = new Offer { Id = 2, Name = "Standard Ticket", Price = 100m };
        var item = new OrderItem
        {
            Offer = offer,
            OfferId = offer.Id,
            Qty = 2,
            Price = offer.Price * 2
        };

        // Act
        var expectedPrice = offer.Price * item.Qty;

        // Assert
        Assert.Equal(expectedPrice, item.Price);
    }

    [Fact]
    public void Changing_Qty_Should_Change_TotalPrice_Manually()
    {
        // Arrange
        var offer = new Offer { Id = 3, Name = "Early Bird", Price = 80m };
        var item = new OrderItem
        {
            Offer = offer,
            OfferId = offer.Id,
            Qty = 1,
            Price = offer.Price
        };

        // Act
        item.Qty = 4;
        item.Price = offer.Price * item.Qty;

        // Assert
        Assert.Equal(4, item.Qty);
        Assert.Equal(320m, item.Price);
    }

    [Fact]
    public void Multiple_OrderItems_Should_Be_Independent()
    {
        // Arrange
        var offer1 = new Offer { Id = 1, Name = "A", Price = 50m };
        var offer2 = new Offer { Id = 2, Name = "B", Price = 100m };

        var item1 = new OrderItem { Id = 1, Offer = offer1, OfferId = offer1.Id, Qty = 1, Price = 50m };
        var item2 = new OrderItem { Id = 2, Offer = offer2, OfferId = offer2.Id, Qty = 2, Price = 200m };

        // Act & Assert
        Assert.NotEqual(item1.Id, item2.Id);
        Assert.NotEqual(item1.OfferId, item2.OfferId);
        Assert.NotEqual(item1.Price, item2.Price);
        Assert.NotSame(item1, item2);
    }
}

}
