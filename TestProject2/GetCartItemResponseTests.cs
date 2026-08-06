using JO.Models.Responses;
using JO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestProject2
{
    public class GetCartItemResponseTests
    {
        [Fact]
        public void DefaultConstructor_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var response = new GetCartItemResponse();

            // Assert
            Assert.Equal(0, response.StatusCode);
            Assert.Null(response.Message);
            Assert.Null(response.CartItem);
            Assert.False(response.Success);
        }

        [Fact]
        public void Properties_ShouldBeSetAndGetCorrectly()
        {
            // Arrange
            var expectedCode = 200;
            var expectedMessage = "Operation successful";
            var dummyItem = new CartItem();

            // Act
            var response = new GetCartItemResponse
            {
                StatusCode = expectedCode,
                Message = expectedMessage,
                CartItem = dummyItem,
                Success = true
            };

            // Assert
            Assert.Equal(expectedCode, response.StatusCode);
            Assert.Equal(expectedMessage, response.Message);
            Assert.Same(dummyItem, response.CartItem);
            Assert.True(response.Success);
        }
    }
}
