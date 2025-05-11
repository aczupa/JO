using Xunit;
using Bunit;
using Moq;
using JO.Models;
using JO.Models.Responses;
using JO.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Bunit.TestDoubles;
using System.Collections.Generic;
using System.Threading.Tasks;
using JO.Pages;
using JO.Components; 

public class OffersTableTests : TestContext
{
    [Fact]
    public void Should_Render_Table_With_Offers()
    {
        // Arrange
        var mockService = new Mock<IOfferService>();
        mockService.Setup(s => s.GetOffers())
            .ReturnsAsync(new GetOffersResponse
            {
                Offers = new List<Offer>
                {
                    new Offer { Id = 1, Name = "Offer1", Description = "Desc1", Price = 10, TicketCount = 100, ImageUrl = "img1.jpg" },
                    new Offer { Id = 2, Name = "Offer2", Description = "Desc2", Price = 20, TicketCount = 200, ImageUrl = "img2.jpg" }
                }
            });

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<OffersTable>();

        // Assert - check if table contains offers
        var markup = cut.Markup;
        Assert.Contains("Offer1", markup);
        Assert.Contains("Desc1", markup);
        Assert.Contains("$10", markup);
        Assert.Contains("100", markup);
        Assert.Contains("Offer2", markup);
        Assert.Contains("Desc2", markup);
        Assert.Contains("$20", markup);
        Assert.Contains("200", markup);
    }


    [Fact]
    public void Should_Navigate_To_OfferDetail_OnButtonClick()
    {
        // Arrange  
        var mockService = new Mock<IOfferService>();
        mockService.Setup(s => s.GetOffers())
            .ReturnsAsync(new GetOffersResponse
            {
                Offers = new List<Offer>
                {
                new Offer { Id = 1, Name = "Offer1", Description = "Desc1", Price = 10, TicketCount = 100, ImageUrl = "img1.jpg" }
                }
            });

        Services.AddSingleton(mockService.Object);
        var navMan = Services.GetRequiredService<FakeNavigationManager>();

        var cut = RenderComponent<OffersTable>();

        // Act - click on TableButton  
        cut.FindComponent<TableButton>().Find("button").Click();

        // Assert - check navigation  

        Assert.Equal("/offer/1", "/" + navMan.Uri.Replace(navMan.BaseUri, ""));

    }


    [Fact]
    public void Should_Not_Render_Table_When_NoOffers()
    {
        // Arrange
        var mockService = new Mock<IOfferService>();
        mockService.Setup(s => s.GetOffers())
            .ReturnsAsync(new GetOffersResponse
            {
                Offers = new List<Offer>()
            });

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<OffersTable>();

        // Assert - table is not rendered
        Assert.DoesNotContain("<table", cut.Markup);
    }

    [Fact]
    public void Should_Show_Error_When_Service_Fails()
    {
        var mockService = new Mock<IOfferService>();
        mockService.Setup(s => s.GetOffers())
            .ReturnsAsync(new GetOffersResponse
            {
                StatusCode = 500,
                Message = "Server error",
                Offers = null
            });

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<OffersTable>();

        // Assert – brak tabeli w przypadku błędu
        Assert.DoesNotContain("<table", cut.Markup);
    }

    [Fact]
    public void Should_Render_Correct_Number_Of_Offers()
    {
        var mockService = new Mock<IOfferService>();
        var offers = new List<Offer>
    {
        new Offer { Id = 1, Name = "Offer1", Description = "Desc1", Price = 10, TicketCount = 100, ImageUrl = "img1.jpg" },
        new Offer { Id = 2, Name = "Offer2", Description = "Desc2", Price = 20, TicketCount = 200, ImageUrl = "img2.jpg" },
        new Offer { Id = 3, Name = "Offer3", Description = "Desc3", Price = 30, TicketCount = 300, ImageUrl = "img3.jpg" }
    };

        mockService.Setup(s => s.GetOffers())
            .ReturnsAsync(new GetOffersResponse { Offers = offers });

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<OffersTable>();

        var rows = cut.FindAll("tr.offer-row");
        Assert.Equal(3, rows.Count);
    }

    [Fact]
    public void Should_Show_Loading_When_Offers_Null()
    {
        var mockService = new Mock<IOfferService>();
        mockService.Setup(s => s.GetOffers())
            .ReturnsAsync(new GetOffersResponse { Offers = null });

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<OffersTable>();
        Assert.Contains("Chargement des offres", cut.Markup);
    }


}
