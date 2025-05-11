using Xunit;
using Bunit;
using Moq;
using JO.Models;
using JO.Models.DTOs;
using JO.Models.Responses;
using JO.Services;
using JO.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

public class OfferEditPageTests : TestContext
{
    [Fact]
    public void Should_DisplayOfferData_OnLoad()
    {
        // Arrange
        var mockService = new Mock<IOfferService>();
        mockService.Setup(s => s.GetOffer(1))
            .ReturnsAsync(new GetOfferResponse
            {
                Offer = new Offer { Id = 1, Name = "Test", Description = "Test Desc", Price = 100, TicketCount = 10, ImageUrl = "url" }
            });

        Services.AddSingleton(mockService.Object);
        var navManager = Services.GetRequiredService<NavigationManager>();

        // Act
        var cut = RenderComponent<OfferDetails>(parameters => parameters.Add(p => p.OfferId, 1));

        // Assert
        cut.Markup.Contains("Test");
        cut.Markup.Contains("Test Desc");
    }

    [Fact]
    public async Task Should_CallEditOffer_OnSubmit()
    {
        // Arrange
        var mockService = new Mock<IOfferService>();
        mockService.Setup(s => s.GetOffer(1))
            .ReturnsAsync(new GetOfferResponse
            {
                Offer = new Offer { Id = 1, Name = "Test", Description = "Test Desc", Price = 100, TicketCount = 10, ImageUrl = "url" }
            });
        mockService.Setup(s => s.EditOffer(It.IsAny<Offer>()))
            .ReturnsAsync(new BaseResponse { StatusCode = 200, Message = "Updated" });

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<OfferDetails>(parameters => parameters.Add(p => p.OfferId, 1));

        // Act
        await cut.Find("form").SubmitAsync();

        // Assert
        mockService.Verify(s => s.EditOffer(It.IsAny<Offer>()), Times.Once);
    }

    [Fact]
    public void Should_ShowDeleteModal_WhenDeleteButtonClicked()
    {
        // Arrange
        var mockService = new Mock<IOfferService>();
        mockService.Setup(s => s.GetOffer(1))
            .ReturnsAsync(new GetOfferResponse
            {
                Offer = new Offer { Id = 1, Name = "Test", Description = "Test Desc", Price = 100, TicketCount = 10, ImageUrl = "url" }
            });

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<OfferDetails>(parameters => parameters.Add(p => p.OfferId, 1));

        // Act
        cut.Find("div.btn-danger").Click();

        // Assert
        Assert.Contains("Are you sure you want to delete this offer?", cut.Markup);
    }
}
