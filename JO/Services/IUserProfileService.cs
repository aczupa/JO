using static JO.Pages.PersonalData;

namespace JO.Services
{
    public interface IUserProfileService
    {
        Task SaveUserProfile(string userId, CheckoutInfo info);
    }
}
