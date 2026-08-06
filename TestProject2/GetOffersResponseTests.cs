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
    public class GetOffersResponseTests
    {
        [Fact]
        public void DefaultConstructor_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var response = new GetOffersResponse();

            // Assert inherited defaults
            Assert.Equal(0, response.StatusCode);
            Assert.Null(response.Message);
            // Assert specific property
            Assert.Null(response.Offers);
        }

        [Fact]
        public void Properties_ShouldBeSetAndGetCorrectly()
        {
            // Arrange
            var expectedCode = 200;
            var expectedMessage = "Offers retrieved successfully";
            var dummyOffers = new List<Offer>
            {
                new Offer(),
                new Offer()
            };

            // Act
            var response = new GetOffersResponse
            {
                StatusCode = expectedCode,
                Message = expectedMessage,
                Offers = dummyOffers
            };

            // Assert
            Assert.Equal(expectedCode, response.StatusCode);
            Assert.Equal(expectedMessage, response.Message);
            Assert.Same(dummyOffers, response.Offers);
            Assert.Equal(2, response.Offers.Count);
        }

        [Fact]
        public void GetOffersResponse_IsDerivedFrom_BaseResponse()
        {
            // Arrange & Act
            var response = new GetOffersResponse();

            // Assert inheritance
            Assert.IsAssignableFrom<BaseResponse>(response);
        }
    }
}
