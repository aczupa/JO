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
        private IDbContextFactory<DataContext> GetDbContextFactory(string dbName)
        {
            return new DbContextFactoryStub(() =>
            {
                var options = new DbContextOptionsBuilder<DataContext>()
                    .UseInMemoryDatabase(dbName)
                    .Options;
                return new DataContext(options);
            });
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
            using (var context = factory.CreateDbContext())
            {
                var offer = new Offer { Id = 1, Name = "Offer1", Description = "Desc1", TicketCount = 1, Price = 10 };
                context.Offers.Add(offer);
                await context.SaveChangesAsync();
            }

            var service = new OfferService(factory);

            var response = await service.GetOffer(1);

            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response.Offer);
            Assert.Equal("Offer1", response.Offer.Name);
        }

        [Fact]
        public async Task EditOffer_Should_Update_Offer_And_Return_Success()
        {
            var factory = GetDbContextFactory("EditOfferDb");
            using (var context = factory.CreateDbContext())
            {
                var offer = new Offer { Id = 1, Name = "Offer1", Description = "Desc1", TicketCount = 1, Price = 10 };
                context.Offers.Add(offer);
                await context.SaveChangesAsync();
            }

            var service = new OfferService(factory);

            var updatedOffer = new Offer { Id = 1, Name = "Updated Offer", Description = "Updated Desc", TicketCount = 5, Price = 50 };

            var response = await service.EditOffer(updatedOffer);

            Assert.Equal(200, response.StatusCode);

            using var verifyContext = factory.CreateDbContext();
            var offerInDb = await verifyContext.Offers.FindAsync(1);
            Assert.Equal("Updated Offer", offerInDb.Name);
            Assert.Equal(50, offerInDb.Price);
        }

        [Fact]
        public async Task DeleteOffer_Should_Remove_Offer_And_Return_Success()
        {
            var factory = GetDbContextFactory("DeleteOfferDb");
            using (var context = factory.CreateDbContext())
            {
                var offer = new Offer { Id = 1, Name = "Offer1", Description = "Desc1", TicketCount = 1, Price = 10 };
                context.Offers.Add(offer);
                await context.SaveChangesAsync();
            }

            var service = new OfferService(factory);

            // Fetch the offer object by ID before passing it to DeleteOffer
            using (var context = factory.CreateDbContext())
            {
                var offerToDelete = await context.Offers.FindAsync(1);
                var response = await service.DeleteOffer(offerToDelete);

                Assert.Equal(204, response.StatusCode);

                var offerInDb = await context.Offers.FindAsync(1);
                Assert.Null(offerInDb);
            }
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
