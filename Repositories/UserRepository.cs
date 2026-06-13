using API___ASP_.NET_Core.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Collections.Immutable;

namespace API___ASP_.NET_Core.Repositories
{
    public class UserRepository :IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = "SELECT Id, Username, Name, Surname, Email, Password, PasswordSalt, Birthday FROM user WHERE Email = @Email";
                User user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });

                user.Friends = await GetFriendsAsync(user.Id);

                user.FriendRequests = await ListFriendRequestsAsync(user.Id);

                return user;
            }
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = "SELECT Id, Username, Name, Surname, Email, Password, PasswordSalt, Birthday FROM user WHERE username = @Username";
                User? user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });

                user.Friends = await GetFriendsAsync(user.Id);

                user.FriendRequests = await ListFriendRequestsAsync(user.Id);

                return user;
            }
        }

        public async Task<List<string>> GetFriendsAsync(int userId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = @"
                            SELECT u.Email 
                            FROM friendRequests fr
                            JOIN user u ON u.Id = (CASE WHEN fr.Sender_Id = @UserId THEN fr.Reciever_Id ELSE fr.Sender_Id END)
                            WHERE (fr.Sender_Id = @UserId OR fr.Reciever_Id = @UserId) AND fr.Is_Accepted = 1;";

                var friendEmails = await connection.QueryAsync<string>(sql, new { UserId = userId });

                return friendEmails.ToList();
            }
        }

        public async Task<List<int>> ListFriendRequestsAsync(int userId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = "SELECT Sender_Id FROM friendRequests WHERE Reciever_Id = @UserId AND Is_Accepted = 0;";
                var friendRequests = await connection.QueryAsync<int>(sql, new { UserId = userId });

                return friendRequests.ToList();
            }
        }

        public async Task<bool> AddFriendAsync(int recieverId, int senderId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = "INSERT INTO friendRequests (Sender_Id, Reciever_Id) VALUES (@Sender_Id, @Reciever_Id);";
                int rowsAffected = await connection.ExecuteAsync(sql, new { Sender_Id = senderId, Reciever_Id = recieverId });

                return rowsAffected > 0;
            }
        }

        public async Task<bool> AcceptFriendRequestAsync(int userId, int senderId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = "UPDATE friendRequests SET Is_Accepted = 1 WHERE Sender_Id = @Sender_Id AND Reciever_Id = @Reciever_Id;";
                int rowsAffected = await connection.ExecuteAsync(sql, new { Sender_Id = senderId, Reciever_Id = userId });

                return rowsAffected > 0;
            }
        }

        public async Task<bool> addUserAsync(User user)
        {
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    string sql = @"INSERT INTO user (Name, Surname, Username, Email, Password, passwordSalt, Birthday) 
                               VALUES (@Name, @Surname, @Username, @Email, @Password, @passwordSalt, @Birthday);";

                    int rowsAdded = await connection.ExecuteAsync(sql, user);

                    return rowsAdded > 0;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> updateUserAsync(User user)
        {
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    string sql = @"UPDATE user SET
                               Name = @Name,
                               Surname = @Surname,
                               Email = @Email,
                               Username = @Username,   
                               Birthday = @Birthday
                               WHERE Id = @Id";

                    int rowsAffected = await connection.ExecuteAsync(sql, user);

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
