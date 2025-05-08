using Microsoft.AspNetCore.Identity;

namespace JO.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        public string UserId { get; set; }  // Klucz obcy do AspNetUsers

        // Powiązanie z IdentityUser (Relacja jeden-do-jednego)
        public IdentityUser User { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; set; }
        public string StreetNumber { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PaymentMethod { get; set; }
    }
}
