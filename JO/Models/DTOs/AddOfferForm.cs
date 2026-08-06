using System.ComponentModel.DataAnnotations;

namespace JO.Models.DTOs
{
    public class AddOfferForm
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public int TicketCount { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public string ImageUrl { get; set; } = string.Empty;

    }
}
