namespace JO.Models
{

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public int OfferId { get; set; }
        public Offer Offer { get; set; } = null!;
        public int Qty { get; set; }
        public decimal Price { get; set; }
    }
}
