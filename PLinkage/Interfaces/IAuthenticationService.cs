using PLinkage.Models;

namespace PLinkage.Interfaces
{
    public interface IAuthenticationService
    {
        Task<IUser?> LoginAsync(string email, string password);
        Task LogoutAsync();
        bool IsUserLoggedIn();
    }


}
