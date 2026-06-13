namespace API___ASP_.NET_Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public byte[] passwordSalt { get; set; }
        public string Birthday { get; set; }
        public List<string> Friends { get; set; }
        public List<int> FriendRequests { get; set; }
    }

    public class AddFriend
    {
        public required Login Auth { get; set; }
        public required string FriendEmail { get; set; }
    }

    public class Register
    {
        public required string Username { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Birthday { get; set; }

    }

    public class Login
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class Edit
    {
        public string Username { get; set; }
        public string newUsername { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Birthday { get; set; }
    }
}
