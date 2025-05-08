using JO.Data;
using JO.Models;
using JO.Models.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace JO.Services
{
    public class CartService : ICartService
    {
        private readonly DataContext _context;

        public CartService(DataContext context)
        {
            _context = context;
        }

        public async Task<GetCartItemResponse> AddOfferToCart(string userId, int offerId, int quantity)
        {
            var response = new GetCartItemResponse();

            var offer = await _context.Offers.FindAsync(offerId);
            if (offer == null)
            {
                response.Success = false;
                response.Message = "L'offre n'a pas été trouvée.";
                return response;
            }

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.OfferId == offerId);

            int alreadyInCartQty = cartItem?.Qty ?? 0;
            int totalRequested = alreadyInCartQty + quantity;

            if (totalRequested > offer.TicketCount)
            {
                response.Success = false;
                response.Message = $"Vous ne pouvez pas ajouter plus de billets. Quantité disponible : {offer.TicketCount - alreadyInCartQty}";
                return response;
            }

            if (cartItem != null)
            {
                cartItem.Qty += quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    OfferId = offerId,
                    Qty = quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            response.Success = true;
            response.Message = "L'offre a été ajoutée au panier.";
            return response;
        }

        public async Task<Cart> GetCart(string userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Offer)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<int> GetCartItemCount(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.CartItems.Sum(ci => ci.Qty) ?? 0;
        }

        public async Task<GetCartItemResponse> RemoveOfferFromCart(string userId, int offerId)
        {
            var response = new GetCartItemResponse();

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                response.Success = false;
                response.Message = "Le panier n'a pas été trouvé.";
                return response;
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.OfferId == offerId);

            if (cartItem != null)
            {
                if (cartItem.Qty > 1)
                {
                    cartItem.Qty -= 1;
                }
                else
                {
                    _context.CartItems.Remove(cartItem);
                }

                await _context.SaveChangesAsync();
                response.Success = true;
                response.Message = "L'offre a été réduite de 1 ou supprimée du panier.";
            }
            else
            {
                response.Success = false;
                response.Message = "L'offre n'a pas été trouvée dans le panier.";
            }

            return response;
        }

        public async Task<GetCartItemResponse> UpdateOfferQuantity(string userId, int offerId, int newQty)
        {
            var response = new GetCartItemResponse();

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                response.Success = false;
                response.Message = "Le panier n'a pas été trouvé.";
                return response;
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.OfferId == offerId);

            var offer = await _context.Offers.FirstOrDefaultAsync(o => o.Id == offerId);
            if (offer == null)
            {
                response.Success = false;
                response.Message = "L'offre n'a pas été trouvée.";
                return response;
            }

            if (newQty > offer.TicketCount)
            {
                response.Success = false;
                response.Message = $"Vous ne pouvez pas définir cette quantité. Quantité maximale disponible : {offer.TicketCount}";
                return response;
            }

            if (cartItem != null)
            {
                cartItem.Qty = newQty;
                await _context.SaveChangesAsync();
                response.Success = true;
                response.Message = "La quantité a été mise à jour.";
            }
            else
            {
                response.Success = false;
                response.Message = "L'offre n'a pas été trouvée dans le panier.";
            }

            return response;
        }

        public async Task<GetCartItemResponse> PlaceOrderAndClearCart(string userId)
        {
            var response = new GetCartItemResponse();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Offer)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                response.Success = false;
                response.Message = "Le panier est vide, il est impossible de passer une commande.";
                return response;
            }

            var order = new Order
            {
                UserId = userId,
                TotalPrice = cart.TotalPrice,
                CreatedAt = DateTime.Now,
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    OfferId = ci.OfferId,
                    Offer = ci.Offer,
                    Qty = ci.Qty,
                    Price = ci.Offer.Price
                }).ToList()
            };

            _context.Orders.Add(order);

            foreach (var item in order.OrderItems)
            {
                var offer = await _context.Offers.FirstOrDefaultAsync(o => o.Id == item.OfferId);
                if (offer != null)
                {
                    offer.TicketCount -= item.Qty;
                    if (offer.TicketCount < 0)
                        offer.TicketCount = 0;

                    _context.Offers.Update(offer);
                }
            }

            await _context.SaveChangesAsync();

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            response.Success = true;
            response.Message = "La commande a été passée et le panier a été vidé.";
            return response;
        }
    }
}
