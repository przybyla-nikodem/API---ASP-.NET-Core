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

        public User? GetByEmail(string email)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = "Select * from user where email = @Email";
                return connection.QueryFirstOrDefault<User>(sql, new { Email = email };
            }
        }

        public User? GetByUsername(string username)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = "Select * from user where username = @Username";
                return connection.QueryFirstOrDefault<User>(sql, new { Username = username };
            }
        }

        public void AddUser(User user)
        {
            using(var connection = new SqliteConnection(_connectionString))
            {
                string sql = @"INSERT INTO user (Name, Surname, Username, Email, Password, passwordSalt, Birthday) 
                               VALUES (@Name, @Surname, @Username, @Email, @Password, @passwordSalt, @Birthday);";

                connection.Execute(sql, user);
            }
        }

        public void updateUser(User user)
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

                connection.Execute(sql, user);
            }
        }
    }
}
