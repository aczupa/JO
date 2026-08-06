namespace JO.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public Cart Cart { get; set; } = null!; // Relacja do Cart
        public int OfferId { get; set; }
        public Offer Offer { get; set; } = null!; // Relacja do Offer
        public int Qty { get; set; }
    }
}
