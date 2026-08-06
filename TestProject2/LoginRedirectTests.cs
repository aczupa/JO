using Bunit;
using Bunit.TestDoubles;
using FluentAssertions.Common;
using JO.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestProject2
{
    public class LoginRedirectTests : TestContext
    {
        [Fact]
        public void OnInitializedAsync_ShouldNavigateToLogin_Sync()
        {
            // Arrange
            var navMan = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;
            Assert.NotNull(navMan);

            // Act
            var cut = RenderComponent<RedirectToLogin>();

            // Assert
            Assert.Equal(navMan.BaseUri + "Identity/Account/Login", navMan.Uri);
        }

        [Fact]
        public async Task OnInitializedAsync_ShouldNavigateToLogin_Async()
        {
            // Arrange
            var navMan = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;
            Assert.NotNull(navMan);

            // Act
            var cut = RenderComponent<RedirectToLogin>();
            await cut.InvokeAsync(() => Task.CompletedTask);

            // Assert
            Assert.Equal(navMan.BaseUri + "Identity/Account/Login", navMan.Uri);
        }
    }
}
