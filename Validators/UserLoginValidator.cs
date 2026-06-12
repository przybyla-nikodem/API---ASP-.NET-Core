using API___ASP_.NET_Core.Models;
using FluentValidation;

namespace API___ASP_.NET_Core.Validators
{
    public class UserLoginValidator : AbstractValidator<Login>
    {
        public UserLoginValidator()
        {
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().WithMessage("Niepoprawny format email.");
            RuleFor(x => x.Password).NotNull().NotEmpty().WithMessage("Hasło nie może być puste.");
        }
    }
}
