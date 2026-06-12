namespace API___ASP_.NET_Core.Repositories
{
    public interface IUserRepository
    {
        User? GetByEmail(string email);
        User? GetByUsername(string username);
        void addUser(User user);
        void updateUser(User user);
    }
}
