using API___ASP_.NET_Core.Models;
using API___ASP_.NET_Core.Repositories;
using API___ASP_.NET_Core.Validators;
using FluentValidation;
using FluentValidation.Results;
using Mapster;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public async Task <IActionResult> Register([FromBody] Register newUserData)
        {
            UserRegisterValidator validator = new UserRegisterValidator();

            ValidationResult results = validator.Validate(newUserData);
            if (!results.IsValid)
            {
                return BadRequest(new { Message = results.Errors[0].ErrorMessage });
            }

            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

            string hashed = HashPassword(newUserData.Password!, salt);


            User dbUser = newUserData.Adapt<User>();
            dbUser.Password = hashed;
            dbUser.passwordSalt = salt;

            try
            {
                bool success = await _userRepository.addUserAsync(dbUser);
                if(success)
                {
                    return StatusCode(StatusCodes.Status201Created, new { Message = "Rejestracja przebiegła pomyślnie" });
                }
            }
            catch(Exception ex)
            {
                return BadRequest(new { Message = "Rejestracja nie udała się" });
            }

            return BadRequest(new { Message = "Rejestracja nie udała się" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login loginData)
        {
            UserLoginValidator validator = new UserLoginValidator();
            ValidationResult results = validator.Validate(loginData);

            if (!results.IsValid)
            {
                return BadRequest(new { Message = results.Errors[0].ErrorMessage });
            }

            User? user = await _userRepository.GetByEmailAsync(loginData.Email);

            if (user == null)
            {
                return BadRequest(new { Message = "Ten email nie jest używany przez żadnego użytkownika" });
            }

            string hashed = HashPassword(loginData.Password, user.passwordSalt);

            if (user.Password == hashed)
            {
                var userResponse = user.Adapt<User>();
                userResponse.Password = null!;
                userResponse.passwordSalt = null!;

                return Ok(new
                {
                    Message = "Zalogowano pomyślnie",
                    User = userResponse
            });
            }
            else
            {
                return BadRequest(new { Message = "Nieprawidłowy email lub hasło." });
            }
        }

        [HttpPatch("edit")]
        public async Task<IActionResult> Alter([FromBody] Edit editData)
        {
            if (editData == null)
            {
                return BadRequest(new { Message = "Dane wejściowe nie mogą być puste." });
            }

            User? userDb = await _userRepository.GetByUsernameAsync(editData.Username);

            if (userDb == null) return BadRequest(new { Message = "Brak uzytkownika" });

            string hashed = HashPassword(editData.Password, userDb.passwordSalt);
            if (hashed != userDb.Password)
            {
                return BadRequest(new { Message = "Nieprawidłowe hasło" });
            }

            UserEditValidator validator = new UserEditValidator();
            ValidationResult results = validator.Validate(editData);

            if(!results.IsValid)
            {
                return BadRequest(new { Message = results.Errors[0].ErrorMessage });
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
                bool success = await _userRepository.updateUserAsync(userDb);
                if (success)
                {
                    var userResponse = userDb.Adapt<User>();
                    userResponse.Password = null!;
                    userResponse.passwordSalt = null!;

                    return Ok(new { 
                        Message = "Dane zmienione pomyślnie",
                        User = userResponse
                    });
                }
                else
                {
                    return BadRequest($"Błąd zapisu danych do bazy");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Błąd zapisu danych do bazy");
            }
        }

        [HttpGet("verify")]
        public async Task <IActionResult> Verify([FromHeader(Name = "Username")] string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { Message = "Brak użytkownika" });
            }

            try
            {
                User? dbUser = await _userRepository.GetByUsernameAsync(username);

                if (dbUser == null)
                {
                    return NotFound("Użytkownik nie istnieje.");
                }

                if (!DateOnly.TryParseExact(dbUser.Birthday, "yyyy-MM-dd", out var birthDay))
                {
                    return BadRequest(new { Message = "Niepoprawny format daty w bazie danych" });
                }

                var date = DateOnly.FromDateTime(DateTime.Today);
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

        [HttpPost("addfriend")]
        public async Task<IActionResult> AddFriendAsync([FromBody] AddFriend requestData)
        {
            User? user = await _userRepository.GetByEmailAsync(requestData.Auth.Email);

            if (user == null) 
            {
                return BadRequest(new { Message = "Niepoprawne dane użytkownika " });
            }

            if(HashPassword(requestData.Auth.Password, user.passwordSalt) == user.Password)
            {
                user.Friends ??= new List<string>();
                requestData.FriendEmail = requestData.FriendEmail.ToLower();

                if(user.Email.Equals(requestData.FriendEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { Message = "Nie możesz dodać samego siebie do znajomych." });
                }

                if (user.Friends.Contains(requestData.FriendEmail))
                {
                    return BadRequest(new { Message = "Znajomy jest już dodany" });
                }

                var reciever = await _userRepository.GetByEmailAsync(requestData.FriendEmail);
                if (reciever == null) return NotFound(new { Message = "Użytkownik o podanym adresie e-mail nie istnieje." });

                int recieverId = reciever.Id;

                bool success = await _userRepository.AddFriendAsync(recieverId, user.Id);

                if (success)
                {
                    return Ok(new { Message = "Wysłano prośbę o dodanie znajomego" });
                }
                else
                {
                    return BadRequest(new { Message = "Nie udało się dodać znajomego" });
                }

            } else
            {
                return BadRequest(new { Message = "Błąd autoryzacji" });
            }
        }

        [HttpPatch("acceptfriend")]
        public async Task<IActionResult> AcceptFriendRequestAsync([FromBody] AddFriend data)
        {
            User? user = await _userRepository.GetByEmailAsync(data.Auth.Email);

            if (user == null)
            {
                return BadRequest(new { Message = "Niepoprawne dane użytkownika " });
            }

            if (HashPassword(data.Auth.Password, user.passwordSalt) == user.Password)
            {
                var friend = await _userRepository.GetByEmailAsync(data.FriendEmail);
                var friendId = friend.Id;

                if (user.FriendRequests.Contains(friendId))
                {
                    bool success = await _userRepository.AcceptFriendRequestAsync(user.Id, friendId);

                    if (success) return Ok(new { Message = $"Zaproszenie do znajomych od " + friend.Email + " zaakceptowane" });
                }
                else
                {
                    return BadRequest(new { Message = "To zaproszenie nie istnieje" });
                }
            } else
            {
                return BadRequest(new { Message = "Błąd autoryzacji"});
            }

            return BadRequest(new { Message = "Nie udało się zaakceptować zaproszenia" });
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile([FromHeader(Name = "Auth-Email")] string email, [FromHeader(Name = "Auth-Password")] string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return Unauthorized(new { Message = "Brak wymaganych nagłówków autoryzacyjnych." });
            }

            User? user = await _userRepository.GetByEmailAsync(email);

            if (user == null) return BadRequest(new { Message = "Użytkownik nie istnieje" });

            if (HashPassword(password, user.passwordSalt) == user.Password)
            {
                var userProfile = user.Adapt<User>();
                userProfile.passwordSalt = null!;
                userProfile.Password = null!;

                return Ok(new
                {
                    Message = "Pomyślnie pobrano profil.",
                    Profile = userProfile
                });
            }
            else
            {
                return Unauthorized(new { Message = "Niepoprawny e-mail lub hasło." });
            }
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