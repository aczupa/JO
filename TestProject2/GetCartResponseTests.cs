using JO.Models;
using JO.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestProject2
{
    public class GetCartResponseTests
    {
        [Fact]
        public void DefaultConstructor_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var response = new GetCartResponse();

            // Assert
            Assert.Equal(0, response.StatusCode);
            Assert.Null(response.Message);
            Assert.Null(response.Cart);
        }

        [Fact]
        public void Properties_ShouldBeSetAndGetCorrectly()
        {
            // Arrange
            var expectedCode = 200;
            var expectedMessage = "Operation successful";
            var dummyCart = new Cart();

            // Act
            var response = new GetCartResponse
            {
                StatusCode = expectedCode,
                Message = expectedMessage,
                Cart = dummyCart
            };

            // Assert
            Assert.Equal(expectedCode, response.StatusCode);
            Assert.Equal(expectedMessage, response.Message);
            Assert.Same(dummyCart, response.Cart);
        }

        [Fact]
        public void GetCartResponse_IsDerivedFrom_BaseResponse()
        {
            // Arrange & Act
            var response = new GetCartResponse();

            // Assert inheritance
            Assert.IsAssignableFrom<BaseResponse>(response);
        }
    }
}
