using Bunit;
using JO.Components;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestProject2
{
    public class ModalComponentTests : TestContext
    {
        [Fact]
        public void RendersChildContentInsideModalBody()
        {
            // Arrange
            var childMarkup = "<p data-test-id=\"child\">Hello, World!</p>";

            // Act
            var cut = RenderComponent<DeleteConfirmationModal>(parameters => parameters
                .AddChildContent(childMarkup)
            );

            // Assert
            var bodyDiv = cut.Find(".modal-body");
            Assert.Contains("Hello, World!", bodyDiv.InnerHtml);
            Assert.NotNull(cut.Find("[data-test-id=\"child\"]"));
        }

        [Fact]
        public void ModalHasCorrectAttributesAndClasses()
        {
            // Act
            var cut = RenderComponent<DeleteConfirmationModal>(parameters => parameters.AddChildContent("X"));

            var root = cut.Find(".modal");
            Assert.Contains("d-block", root.ClassList);
            Assert.Equal("-1", root.GetAttribute("tabindex"));
            Assert.Equal("dialog", root.GetAttribute("role"));

            var dialogDiv = cut.Find(".modal-dialog");
            Assert.Equal("document", dialogDiv.GetAttribute("role"));
        }

        [Fact]
        public void DoesNotRenderFooterWhenNotProvided()
        {
            // Act
            var cut = RenderComponent<DeleteConfirmationModal>(parameters => parameters.AddChildContent("No footer"));

            // Assert: There should be no element with a .modal-footer class
            var footers = cut.FindAll(".modal-footer");
            Assert.Empty(footers);
        }

        [Fact]
        public void RendersFooterContentWhenProvided()
        {
            // Arrange
            var footerMarkup = "<button data-test-id=\"footer-btn\">OK</button>";

            // Act
            var cut = RenderComponent<DeleteConfirmationModal>(parameters => parameters
                .AddChildContent("Content")
                .Add<RenderFragment>(p => p.FooterContent, builder => builder.AddMarkupContent(0, footerMarkup))
            );

            // Assert
            var footer = cut.Find(".modal-footer");
            Assert.Contains("button", footer.InnerHtml);
            Assert.NotNull(cut.Find("[data-test-id=\"footer-btn\"]"));
        }
    }
}
