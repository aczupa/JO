using JO.Data;
using JO.Models;
using JO.Models.DTOs;
using JO.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace JO.Services
{


        public interface IOfferService
        {
         Task<GetOffersResponse> GetOffers();
        Task<BaseResponse> AddOffer(AddOfferForm form);
        Task<GetOfferResponse> GetOffer(int id);
        Task<BaseResponse> DeleteOffer(Offer offer);
        Task<BaseResponse> EditOffer(Offer offer);
    }

        public class OfferService : IOfferService
        {
            private readonly IDbContextFactory<DataContext> _factory;

            public OfferService(IDbContextFactory<DataContext> factory)
            {
                _factory = factory;
            }

        public async Task<BaseResponse> AddOffer(AddOfferForm form)
        {
             
            var response = new BaseResponse();
            try
            {
                using (var context = _factory.CreateDbContext())
                {
                    context.Add(new Offer
                    {
                        Name = form.Name,
                        TicketCount = form.TicketCount,
                        Price = form.Price,
                        Description = form.Description,
                        ImageUrl = form.ImageUrl
                    });

                    var result = await context.SaveChangesAsync();

                    if (result == 1)
                    {
                        response.StatusCode = 200;
                        response.Message = "Offer added successfully";
                    }
                    else
                    {
                        response.StatusCode = 400;
                        response.Message = "Error occurred while adding the offer.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = "Error adding employee: " + ex.Message;
            }

            return response;
        }

        public async Task<BaseResponse> DeleteOffer(Offer offer)
        {
            var response = new BaseResponse();
            try
            {
                using (var context = _factory.CreateDbContext())
                {
                    context.Remove(offer);

                    var result = await context.SaveChangesAsync();

                    if (result == 1)
                    {
                        response.StatusCode = 204;
                        response.Message = "Offer removed successfully";
                    }
                    else
                    {
                        response.StatusCode = 400;
                        response.Message = "Error occurred while removing the offer.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = "Error removing offer: " + ex.Message;
            }

            return response;
        }

        public async Task<BaseResponse> EditOffer(Offer offer)
        {
            var response = new BaseResponse();
            try
            {
                using (var context = _factory.CreateDbContext())
                {
                    context.Update(offer);

                    var result = await context.SaveChangesAsync();

                    if (result == 1)
                    {
                        response.StatusCode = 200;
                        response.Message = "Offer updated successfully";
                    }
                    else
                    {
                        response.StatusCode = 400;
                        response.Message = "Error occurred while updating the offer.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = "Error updating offer: " + ex.Message;
            }

            return response;
        }

        public async Task<GetOfferResponse> GetOffer(int id)
        {

            var response = new GetOfferResponse();
            try
            {
                using (var context = _factory.CreateDbContext())
                {
                    var offer = await context.Offers.FirstOrDefaultAsync(x => x.Id == id);
                    response.StatusCode = 200;
                    response.Message = "Success";
                    response.Offer = offer;
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = "Error retrieving offer: " + ex.Message;
            }

            return response;
        }

        public async Task<GetOffersResponse> GetOffers()
            {
                var response = new GetOffersResponse();
                try
                {
                    using (var context = _factory.CreateDbContext())
                    {
                        var offers = context.Offers.ToList();
                        response.StatusCode = 200;
                        response.Message = "Success";
                        response.Offers = offers;
                    }
                }
                catch (Exception ex)
                {
                    response.StatusCode = 500;
                    response.Message = "Error retrieving employees: " + ex.Message;
                    response.Offers = null;
                }

                return response;
            }
        }
    }

