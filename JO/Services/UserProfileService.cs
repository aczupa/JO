using JO.Data;
using JO.Models;
using System.Threading.Tasks;
using static JO.Pages.PersonalData;

namespace JO.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly DataContext _context;

        public UserProfileService(DataContext context)
        {
            _context = context;
        }

        public async Task SaveUserProfile(string userId, CheckoutInfo info)
        {
            var userProfile = new UserProfile
            {
                UserId = userId,
                FirstName = info.FirstName,
                LastName = info.LastName,
                Street = info.Street,
                StreetNumber = info.StreetNumber,
                PostalCode = info.PostalCode,
                City = info.City,
                Country = info.Country,
                PaymentMethod = info.PaymentMethod
            };

            _context.UserProfile.Add(userProfile);
            await _context.SaveChangesAsync();
        }
    }
}
