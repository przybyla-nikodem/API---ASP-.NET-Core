using API___ASP_.NET_Core.Models;
using API___ASP_.NET_Core.Repositories;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace API___ASP_.NET_Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] Register newUserData)
        {
            User dbUser;

            if (newUserData == null || string.IsNullOrEmpty(newUserData.Password) || string.IsNullOrEmpty(newUserData.Email))
            {
                return BadRequest(new { Message = "Dane wejściowe nie mogą być puste." });
            }

            try
            {
                var mail = new MailAddress(newUserData.Email);
            }
            catch
            {
                return BadRequest(new { Message = "Format adresu email jest niepoprawny." });
            }

            if (hasSpecial(newUserData.Name))

            {

                return BadRequest(new { Message = "Niepoprawne znaki w imieniu" });

            }
            else if (hasSpecial(newUserData.Surname))

            {

                return BadRequest(new { Message = "Niepoprawne znaki w nazwisku" });

            }

            if (!DateOnly.TryParse(newUserData.Birthday, out var parsedDate))
            {
                return BadRequest(new { Message = "Błędny format daty. Użyj formatu YYYY-MM-DD." });
            }

            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

            string hashed = HashPassword(newUserData.Password!, salt);

            try
            {
                dbUser = new User
                {
                    Username = newUserData.UserName,
                    Name = newUserData.Name,
                    Surname = newUserData.Surname,
                    Email = newUserData.Email,
                    Password = hashed,
                    passwordSalt = salt,
                    Birthday = newUserData.Birthday
                };
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Nie udało się zarejestrować" });
            }

            try
            {
                _userRepository.addUser(dbUser);
                return StatusCode(StatusCodes.Status201Created, new { Message = "Rejestracja przebiegła pomyślnie" });
            }
            catch
            {
                return BadRequest(new { Message = "Rejestracja nie udała się" });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Login loginData)
        {
            if (loginData == null || string.IsNullOrEmpty(loginData.Email) || string.IsNullOrEmpty(loginData.Password))
            {
                return BadRequest(new { Message = "Email i hasło są wymagane." });
            }

            User? user = _userRepository.GetByEmail(loginData.Email);

            if (user == null)
            {
                return BadRequest(new { Message = "Ten email nie jest używany przez żadnego użytkownika" });
            }

            string hashed = HashPassword(loginData.Password, user.passwordSalt);

            if (user.Password == hashed)
            {
                return Ok(new
                {
                    Message = "Zalogowano pomyślnie",
                    User = new { user.Id, user.Username, user.Email }
                });
            }
            else
            {
                return BadRequest(new { Message = "Nieprawidłowy email lub hasło." });
            }
        }

        [HttpPatch("edit")]
        public IActionResult Alter([FromBody] Edit editData)
        {
            if (editData == null || string.IsNullOrEmpty(editData.Password) || string.IsNullOrEmpty(editData.Username))
            {
                return BadRequest(new { Message = "Hasło i nazwa użytkownika nie mogą być puste" });
            }

            User? userDb = _userRepository.GetByUsername(editData.Username);

            if (userDb == null) return BadRequest(new { Message = "Brak uzytkownika" });

            string hashed = HashPassword(editData.Password, userDb.passwordSalt);
            if (hashed != userDb.Password)
            {
                return BadRequest(new { Message = "Nieprawidłowe hasło" });
            }

            if (string.IsNullOrEmpty(editData.newUsername) && string.IsNullOrEmpty(editData.Name) &&
                string.IsNullOrEmpty(editData.Surname) && string.IsNullOrEmpty(editData.Email) &&
                string.IsNullOrEmpty(editData.Birthday))
            {
                return BadRequest(new { Message = "Brak danych do zmiany." });
            }

            if ((string.IsNullOrEmpty(editData.newUsername) || editData.newUsername == userDb.Username) &&
                (string.IsNullOrEmpty(editData.Name) || editData.Name == userDb.Name) &&
                (string.IsNullOrEmpty(editData.Surname) || editData.Surname == userDb.Surname) &&
                (string.IsNullOrEmpty(editData.Email) || editData.Email == userDb.Email) &&
                (string.IsNullOrEmpty(editData.Birthday) || editData.Birthday == userDb.Birthday))
            {
                return BadRequest(new { Message = "Przesłane dane są identyczne z aktualnymi. Nic nie zmieniono." });
            }

            if (!string.IsNullOrEmpty(editData.Email) && !Regex.IsMatch(editData.Email, @"@.*\."))
            {
                return BadRequest(new { Message = "Format nowego adresu email jest niepoprawny" });
            }

            if (!string.IsNullOrEmpty(editData.Name) && hasSpecial(editData.Name))
            {
                return BadRequest(new { Message = "Niepoprawne znaki w nowym imieniu" });
            }

            if (!string.IsNullOrEmpty(editData.Surname) && hasSpecial(editData.Surname))
            {
                return BadRequest(new { Message = "Niepoprawne znaki w nowym nazwisku" });
            }

            var tempNewUsername = !string.IsNullOrEmpty(editData.newUsername) ? editData.newUsername : userDb.Username;
            var tempNewName = !string.IsNullOrEmpty(editData.Name) ? editData.Name : userDb.Name;
            var tempNewSurname = !string.IsNullOrEmpty(editData.Surname) ? editData.Surname : userDb.Surname;
            var tempNewEmail = !string.IsNullOrEmpty(editData.Email) ? editData.Email : userDb.Email;
            var tempNewBirthday = !string.IsNullOrEmpty(editData.Birthday) ? editData.Birthday : userDb.Birthday;

            userDb.Username = tempNewUsername;
            userDb.Name = tempNewName;
            userDb.Surname = tempNewSurname;
            userDb.Email = tempNewEmail;
            userDb.Birthday = tempNewBirthday;

            try
            {
                _userRepository.updateUser(userDb);
                return Ok(new { Message = "Dane zmienione pomyślnie" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Błąd zapisu danych do bazy");
            }
        }

        [HttpGet("verify")]
        public IActionResult Verify([FromHeader(Name = "Username")] string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { Message = "Brak użytkownika" });
            }

            try
            {
                User? dbUser = _userRepository.GetByUsername(username);

                if (dbUser == null)
                {
                    return NotFound("Użytkownik nie istnieje.");
                }

                var date = DateOnly.FromDateTime(DateTime.Today);
                var birthDay = DateOnly.Parse(dbUser.Birthday.ToString()!);
                int age = date.Year - birthDay.Year;

                if (birthDay > date.AddYears(-age)) age--;

                if (age >= 18)
                {
                    return Ok(new { Message = "Sukces, masz > 18 lat." });
                }
                else
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { Message = "Nie masz > 18 lat" });
                }
            }
            catch
            {
                return BadRequest(new { Message = "Nie udało się zweryfikować danych" });
            }
        }

        private bool hasSpecial(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;

            if (Regex.IsMatch(s, @"[^a-zA-ZąęćńóśłźżĄĘĆŃÓŚŁŹŻ]"))
            {
                return true;
            }
            return false;
        }

        private string HashPassword(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
        }
    }
}