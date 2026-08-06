namespace JO.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPaid { get; set; }

       

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
