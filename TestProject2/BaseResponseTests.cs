using JO.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestProject2
{
    public class BaseResponseTests
    {

        [Fact]
        public void DefaultConstructor_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var response = new BaseResponse();

            // Assert
            Assert.Equal(0, response.StatusCode);
            Assert.Null(response.Message);
        }

        [Fact]
        public void Properties_ShouldBeSetAndGetCorrectly()
        {
            // Arrange
            var expectedCode = 404;
            var expectedMessage = "Not Found";

            // Act
            var response = new BaseResponse
            {
                StatusCode = expectedCode,
                Message = expectedMessage
            };

            // Assert
            Assert.Equal(expectedCode, response.StatusCode);
            Assert.Equal(expectedMessage, response.Message);
        }
    }
}

