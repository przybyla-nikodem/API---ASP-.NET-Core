using API___ASP_.NET_Core.Models;
using Dapper;
using Microsoft.Data.Sqlite;

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
                string sql = "SELECT Id, Name, Surname, Email, Password, PasswordSalt, Birthday FROM user WHERE Email = @Email";
                return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
            }
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = "SELECT Id, Name, Surname, Email, Password, PasswordSalt, Birthday FROM user WHERE username = @Username";
                return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
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
