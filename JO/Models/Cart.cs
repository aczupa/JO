namespace JO.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; 
        public List<CartItem> CartItems { get; set; } = new();

        public decimal TotalPrice => CartItems.Sum(item => item.Offer.Price * item.Qty);
    }
}
