using JO.Data;
using JO.Models;
using JO.Models.DTOs;
using JO.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace JO.Tests.Services
{
    public class OfferServiceTests
    {
        private IDbContextFactory<DataContext> GetDbContextFactory(string dbNamePrefix)
        {
            var uniqueDbName = $"{dbNamePrefix}_{Guid.NewGuid()}";

           
            var initOptions = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(uniqueDbName)
                .Options;
            using (var initContext = new DataContext(initOptions))
            {
                initContext.Database.EnsureDeleted();
                initContext.Database.EnsureCreated();
                initContext.Offers.RemoveRange(initContext.Offers);
                initContext.SaveChanges();
            }

        
            return new DbContextFactoryStub(() =>
            {
                var options = new DbContextOptionsBuilder<DataContext>()
                    .UseInMemoryDatabase(uniqueDbName)
                    .Options;
                return new DataContext(options);
            });
        }





        [Fact]
        public async Task GetOffers_Should_Return_All_Offers()
        {
            var factory = GetDbContextFactory("GetOffersDb");
            using (var context = factory.CreateDbContext())
            {
                context.Offers.AddRange(
                    new Offer { Name = "Offer 1", Description = "Desc", TicketCount = 5, Price = 10 },
                    new Offer { Name = "Offer 2", Description = "Desc", TicketCount = 10, Price = 20 }
                );
                await context.SaveChangesAsync();
            }

            var service = new OfferService(factory);
            var response = await service.GetOffers();

            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response.Offers);
            Assert.Equal(2, response.Offers.Count);
        }


        [Fact]
        public async Task EditOffer_Should_Return_400_When_NoChangesSaved()
        {
            var factory = GetDbContextFactory("EditOfferNoChangesDb");
            int offerId;

            using (var context = factory.CreateDbContext())
            {
                var offer = new Offer
                {
                    Name = "Test",
                    Description = "Test",
                    TicketCount = 10,
                    Price = 100
                };
                context.Offers.Add(offer);
                await context.SaveChangesAsync();
                offerId = offer.Id;
            }

            var service = new OfferService(factory);

            using var editContext = factory.CreateDbContext();
            var existing = await editContext.Offers.FindAsync(offerId);
            var response = await service.EditOffer(existing); 

            Assert.Equal(400, response.StatusCode);
            Assert.Contains("updating the offer", response.Message);
        }

        [Fact]
        public async Task EditOffer_Should_Return_400_When_Offer_Is_Null()
        {
            var factory = GetDbContextFactory("NullEditDb");
            var service = new OfferService(factory);

            var response = await service.EditOffer(null);

            Assert.Equal(400, response.StatusCode);
            Assert.Contains("cannot be null", response.Message);
        }




        [Fact]
        public async Task DeleteOffer_Should_Handle_Null_Offer()
        {
            var factory = GetDbContextFactory("NullDeleteDb");
            var service = new OfferService(factory);

            var response = await service.DeleteOffer(null);

            Assert.Equal(500, response.StatusCode);
            Assert.Contains("Error removing offer", response.Message);
        }


        [Fact]
        public async Task AddOffer_Should_Add_Offer_And_Return_Success()
        {
            var factory = GetDbContextFactory("AddOfferDb");
            var service = new OfferService(factory);

            var form = new AddOfferForm
            {
                Name = "Test Offer",
                Description = "Test Description",
                TicketCount = 10,
                Price = 100,
                ImageUrl = "https://example.com/image.jpg"
            };

            var response = await service.AddOffer(form);

            Assert.Equal(200, response.StatusCode);
            Assert.Equal("Offer added successfully", response.Message);

            using var context = factory.CreateDbContext();
            var offerInDb = await context.Offers.FirstOrDefaultAsync(o => o.Name == "Test Offer");
            Assert.NotNull(offerInDb);
        }

        [Fact]
        public async Task GetOffer_Should_Return_Offer_When_Found()
        {
            var factory = GetDbContextFactory("GetOfferDb");
            int offerId;
            using (var context = factory.CreateDbContext())
            {
                var offer = new Offer { Name = "Offer1", Description = "Desc1", TicketCount = 1, Price = 10 };
                context.Offers.Add(offer);
                await context.SaveChangesAsync();
                offerId = offer.Id;
            }

            var service = new OfferService(factory);

            var response = await service.GetOffer(offerId);

            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response.Offer);
            Assert.Equal("Offer1", response.Offer.Name);
        }

        [Fact]
        public async Task EditOffer_Should_Update_Offer_And_Return_Success()
        {
            var factory = GetDbContextFactory("EditOfferDb");
            int offerId;
            using (var context = factory.CreateDbContext())
            {
                var offer = new Offer { Name = "Offer1", Description = "Desc1", TicketCount = 1, Price = 10 };
                context.Offers.Add(offer);
                await context.SaveChangesAsync();
                offerId = offer.Id;
            }

            var service = new OfferService(factory);

            var updatedOffer = new Offer { Id = offerId, Name = "Updated Offer", Description = "Updated Desc", TicketCount = 5, Price = 50 };

            var response = await service.EditOffer(updatedOffer);

            Assert.Equal(200, response.StatusCode);

            using var verifyContext = factory.CreateDbContext();
            var offerInDb = await verifyContext.Offers.FindAsync(offerId);
            Assert.Equal("Updated Offer", offerInDb.Name);
            Assert.Equal(50, offerInDb.Price);
        }

        [Fact]
        public async Task DeleteOffer_Should_Remove_Offer_And_Return_Success()
        {
            var factory = GetDbContextFactory("DeleteOfferDb");
            int offerId;
            using (var context = factory.CreateDbContext())
            {
                var offer = new Offer { Name = "Offer1", Description = "Desc1", TicketCount = 1, Price = 10 };
                context.Offers.Add(offer);
                await context.SaveChangesAsync();
                offerId = offer.Id;
            }

            var service = new OfferService(factory);

            using (var context = factory.CreateDbContext())
            {
                var offerToDelete = await context.Offers.FindAsync(offerId);
                var response = await service.DeleteOffer(offerToDelete);

                Assert.Equal(204, response.StatusCode);
            }

           
            using (var verifyContext = factory.CreateDbContext())
            {
                var offerInDb = await verifyContext.Offers.FindAsync(offerId);
                Assert.Null(offerInDb);
            }
        }

        [Fact]
        public async Task EditOffer_Should_Return_404_When_Offer_NotFound()
        {
            var factory = GetDbContextFactory("EditNonExistentDb");
            var service = new OfferService(factory);
            var fakeOffer = new Offer { Id = 999, Name = "X", Description = "Y", TicketCount = 0, Price = 0 };

            var response = await service.EditOffer(fakeOffer);

            Assert.Equal(404, response.StatusCode);
            Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task DeleteOffer_Should_Return_400_When_RemoveFails()
        {
            var factory = GetDbContextFactory("DeleteFailDb");
            var service = new OfferService(factory);
            var nonExistent = new Offer { Id = 999 };

            var response = await service.DeleteOffer(nonExistent);

            Assert.Equal(400, response.StatusCode);
            Assert.Contains("removing the offer", response.Message, StringComparison.OrdinalIgnoreCase);
        }


        [Fact]
        public async Task GetOffers_Should_Return_EmptyList_When_NoOffers()
        {
            var factory = GetDbContextFactory("EmptyOffersDb");
            var service = new OfferService(factory);

            var response = await service.GetOffers();

            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response.Offers);
            Assert.Empty(response.Offers);
        }

        [Fact]
        public async Task GetOffer_Should_Return_Null_When_NotFound()
        {
            var factory = GetDbContextFactory("GetOfferNotFoundDb");
            var service = new OfferService(factory);

            var response = await service.GetOffer(999); // dowolne nieistniejące Id

            Assert.Equal(200, response.StatusCode);
            Assert.Null(response.Offer);
        }


        private class DbContextFactoryStub : IDbContextFactory<DataContext>
        {
            private readonly Func<DataContext> _contextFactory;
            public DbContextFactoryStub(Func<DataContext> contextFactory)
            {
                _contextFactory = contextFactory;
            }

            public DataContext CreateDbContext()
            {
                return _contextFactory();
            }
        }
    }


}
