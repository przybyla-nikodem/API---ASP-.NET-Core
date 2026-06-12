using API___ASP_.NET_Core.Models;
namespace API___ASP_.NET_Core.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> addUserAsync(User user);
        Task<bool> updateUserAsync(User user);
    }
}
