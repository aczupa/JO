using Xunit;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Bunit.TestDoubles;
using System.Threading.Tasks;
using JO.Pages;

namespace TestProject2
{
    public class CreditCarteFormTests : TestContext 
    {

        [Fact]
        public void Should_Render_All_Fields()
        {
            // Arrange
            var navMan = Services.GetRequiredService<FakeNavigationManager>();

            // Act
            var cut = RenderComponent<CreditCarteForm>();

            // Assert
            Assert.NotNull(cut.Find("#cardNumber"));
            Assert.NotNull(cut.Find("#expiryDate"));
            Assert.NotNull(cut.Find("#cvv"));
            Assert.NotNull(cut.Find("#cardHolder"));
            Assert.NotNull(cut.Find("button[type='submit']"));
        }

        [Fact]
        public async Task Should_NavigateToConfirmation_OnValidSubmit()
        {
            // Arrange
            var navMan = Services.GetRequiredService<FakeNavigationManager>();

            var cut = RenderComponent<CreditCarteForm>();

            // Fill in valid data
            cut.Find("#cardNumber").Change("4111111111111111");
            cut.Find("#expiryDate").Change("12/25");
            cut.Find("#cvv").Change("123");
            cut.Find("#cardHolder").Change("John Doe");

            // Act
            await cut.Find("form").SubmitAsync();

            // Assert
            Assert.Equal("confirmation", navMan.Uri.Replace(navMan.BaseUri, ""));
        }

        [Fact]
        public async Task Should_DisplayValidationErrors_WhenInvalidData()
        {
            // Arrange
            var cut = RenderComponent<CreditCarteForm>();

            // Leave form empty and submit
            await cut.Find("form").SubmitAsync();

            // Assert - check if validation messages appear
            var markup = cut.Markup;
            Assert.Contains("Le numéro de carte est requis.", markup);
            Assert.Contains("La date d'expiration est requise.", markup);
            Assert.Contains("Le CVV est requis.", markup);
            Assert.Contains("Le nom du titulaire est requis.", markup);
        }

        [Fact]
        public async Task Should_DisplayInvalidFormatErrors_WhenInvalidFormat()
        {
            // Arrange
            var cut = RenderComponent<CreditCarteForm>();

            // Fill in invalid data
            cut.Find("#cardNumber").Change("123");
            cut.Find("#expiryDate").Change("13/99");
            cut.Find("#cvv").Change("12");
            cut.Find("#cardHolder").Change("John Doe");

            // Act
            await cut.Find("form").SubmitAsync();

            // Assert - check if validation error messages appear
            var markup = cut.Markup;
            Assert.Contains("Numéro de carte invalide.", markup);
            Assert.Contains("Format invalide. Utilisez MM/AA.", markup);
            Assert.Contains("CVV invalide.", markup);
        }
    }
}
