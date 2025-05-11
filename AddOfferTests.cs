using Xunit;
using Bunit;
using Moq;
using JO.Models.DTOs;
using JO.Models.Responses;
using JO.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using JO.Pages;
using Bunit.TestDoubles;

public class AddOfferTests : TestContext
{
    [Fact]
    public void Should_Render_AddOffer_Form_Inputs()
    {
        // Arrange
        var mockService = new Mock<IOfferService>();
        Services.AddSingleton(mockService.Object);
        Services.AddSingleton<FakeNavigationManager>();
        Services.AddSingleton<NavigationManager>(sp => sp.GetRequiredService<FakeNavigationManager>());

        // Act
        var cut = RenderComponent<AddOffer>();

        // Assert - check presence of fields
        Assert.NotNull(cut.Find("input[placeholder='Name here']"));
        Assert.NotNull(cut.Find("textarea[placeholder='Enter description']"));
        Assert.NotNull(cut.Find("input[placeholder='Enter price']"));
        Assert.NotNull(cut.Find("input[placeholder='Ticket count here']"));
        Assert.NotNull(cut.Find("input[placeholder='Img here']"));
        Assert.NotNull(cut.Find("button[type='submit']"));
    }

    [Fact]
    public async Task Should_CallAddOffer_AndRedirect_WhenFormSubmitted()
    {
        // Arrange
        var mockService = new Mock<IOfferService>();
        mockService.Setup(s => s.AddOffer(It.IsAny<AddOfferForm>()))
            .ReturnsAsync(new BaseResponse { StatusCode = 200, Message = "OK" });

        Services.AddSingleton(mockService.Object);
        Services.AddSingleton<FakeNavigationManager>();
        Services.AddSingleton<NavigationManager>(sp => sp.GetRequiredService<FakeNavigationManager>());

        var navMan = Services.GetRequiredService<FakeNavigationManager>();
        var cut = RenderComponent<AddOffer>();

        // Fill in form fields
        cut.Find("input[placeholder='Name here']").Change("Test Name");
        cut.Find("textarea[placeholder='Enter description']").Change("Test Description");
        cut.Find("input[placeholder='Enter price']").Change("123");
        cut.Find("input[placeholder='Ticket count here']").Change("10");
        cut.Find("input[placeholder='Img here']").Change("http://img.com/pic.png");

        // Act - submit form
        await cut.Find("form").SubmitAsync();

        // Assert
        mockService.Verify(s => s.AddOffer(It.Is<AddOfferForm>(f =>
            f.Name == "Test Name" &&
            f.Description == "Test Description" &&
            f.Price == 123 &&
            f.TicketCount == 10 &&
            f.ImageUrl == "http://img.com/pic.png"
        )), Times.Once);

        var relativeUri = navMan.Uri.Replace(navMan.BaseUri, "");
        if (!relativeUri.StartsWith("/"))
            relativeUri = "/" + relativeUri;

        Assert.Equal("/offers-table", relativeUri);
    }

    [Fact]
    public async Task Should_NotCallAddOffer_WhenFormInvalid()
    {
        // Arrange
        var mockService = new Mock<IOfferService>();
        Services.AddSingleton(mockService.Object);
        Services.AddSingleton<FakeNavigationManager>();
        Services.AddSingleton<NavigationManager>(sp => sp.GetRequiredService<FakeNavigationManager>());

        var cut = RenderComponent<AddOffer>();

        // Act - submit form without filling fields
        await cut.Find("form").SubmitAsync();

        // Assert - AddOffer should not be called
        mockService.Verify(s => s.AddOffer(It.IsAny<AddOfferForm>()), Times.Never);
    }

    [Fact]
    public async Task Should_HandleError_WhenAddOfferFails()
    {
        // Arrange
        var mockService = new Mock<IOfferService>();
        mockService.Setup(s => s.AddOffer(It.IsAny<AddOfferForm>()))
            .ReturnsAsync(new BaseResponse { StatusCode = 500, Message = "Error" });

        Services.AddSingleton(mockService.Object);
        Services.AddSingleton<FakeNavigationManager>();
        Services.AddSingleton<NavigationManager>(sp => sp.GetRequiredService<FakeNavigationManager>());

        var cut = RenderComponent<AddOffer>();

        // Fill in form fields
        cut.Find("input[placeholder='Name here']").Change("Test Name");
        cut.Find("textarea[placeholder='Enter description']").Change("Test Description");
        cut.Find("input[placeholder='Enter price']").Change("123");
        cut.Find("input[placeholder='Ticket count here']").Change("10");
        cut.Find("input[placeholder='Img here']").Change("http://img.com/pic.png");

        // Act
        await cut.Find("form").SubmitAsync();

        // Assert - you can extend the component to show a warning or error message
        // Example: Assert.Contains("Error", cut.Markup);
        mockService.Verify(s => s.AddOffer(It.IsAny<AddOfferForm>()), Times.Once);
    }
}
