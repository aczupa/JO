namespace JO.Models
{
  
        public class Offer
        {
            public int Id { get; set; } 
            public string Name { get; set; } = string.Empty; 
            public int TicketCount { get; set; } 
            public decimal Price { get; set; } 
            public string Description { get; set; } = string.Empty; 
            public string ImageUrl { get; set; } = string.Empty;

       
        }
}
