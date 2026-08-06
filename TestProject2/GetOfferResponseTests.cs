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
   

        public class GetOfferResponseTests
        {
            [Fact]
            public void DefaultConstructor_ShouldHaveDefaultValues()
            {
                // Arrange & Act
                var response = new GetOfferResponse();

                // Assert inherited defaults
                Assert.Equal(0, response.StatusCode);
                Assert.Null(response.Message);
                // Assert specific property
                Assert.Null(response.Offer);
            }

            [Fact]
            public void Properties_ShouldBeSetAndGetCorrectly()
            {
                // Arrange
                var expectedCode = 200;
                var expectedMessage = "Offer retrieved successfully";
                var dummyOffer = new Offer { /* initialize properties if needed */ };

                // Act
                var response = new GetOfferResponse
                {
                    StatusCode = expectedCode,
                    Message = expectedMessage,
                    Offer = dummyOffer
                };

                // Assert
                Assert.Equal(expectedCode, response.StatusCode);
                Assert.Equal(expectedMessage, response.Message);
                Assert.Same(dummyOffer, response.Offer);
            }

            [Fact]
            public void GetOfferResponse_IsDerivedFrom_BaseResponse()
            {
                // Arrange & Act
                var response = new GetOfferResponse();

                // Assert inheritance
                Assert.IsAssignableFrom<BaseResponse>(response);
            }
        }
    }

