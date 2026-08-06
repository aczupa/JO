using JO.Models;
using JO.Models.Responses;
using System.Threading.Tasks;

namespace JO.Services
{
    public interface ICartService
    {
        Task<GetCartItemResponse> AddOfferToCart(string userId, int offerId, int quantity);
        Task<Cart> GetCart(string userId);
        Task<int> GetCartItemCount(string userId);
        Task<GetCartItemResponse> RemoveOfferFromCart(string userId, int offerId);
        Task<GetCartItemResponse> UpdateOfferQuantity(string userId, int offerId, int newQty);

       
        Task<GetCartItemResponse> PlaceOrderAndClearCart(string userId);
    }
}
