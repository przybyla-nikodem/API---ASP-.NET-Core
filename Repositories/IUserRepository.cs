using API___ASP_.NET_Core.Models;
namespace API___ASP_.NET_Core.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> addUserAsync(User user);
        Task<bool> updateUserAsync(User user);
        Task<List<string>> GetFriendsAsync(int userId);
        Task<bool> AddFriendAsync(int recieverId, int senderId);
        Task<bool> AcceptFriendRequestAsync(int recieverId, int senderId);
        Task<List<int>> ListFriendRequestsAsync(int userId);
    }
}
