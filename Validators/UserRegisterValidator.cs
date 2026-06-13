using API___ASP_.NET_Core.Models;
using FluentValidation;

namespace API___ASP_.NET_Core.Validators
{
    public class UserRegisterValidator : AbstractValidator<Register>
    {
        public UserRegisterValidator()
        {
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().WithMessage("Niepoprawny format email.");
            RuleFor(x => x.Password).NotNull().NotEmpty().MinimumLength(6).WithMessage("Hasło musi mieć min. 6 znaków.");
            RuleFor(x => x.Name).NotNull().NotEmpty().Matches(@"^[a-zA-ZąęćńóśłźżĄĘĆŃÓŚŁŹŻ]+$").WithMessage("Imię zawiera błędne znaki.");
            RuleFor(x => x.Username).NotNull().NotEmpty().MinimumLength(6).WithMessage("Nazwa użytkownika musi mieć conajmniej 6 znaków");
            RuleFor(x => x.Surname).NotNull().NotEmpty().Matches(@"^[a-zA-ZąęćńóśłźżĄĘĆŃÓŚŁŹŻ]+$").WithMessage("Nazwisko zawiera błędne znaki.");
            RuleFor(x => x.Birthday)
                .NotNull().WithMessage("Data urodzenia jest wymagana.")
                .NotEmpty().WithMessage("Data urodzenia nie może być pusta.")
                .Custom((birthdayString, context) =>
                {
                    if (!DateOnly.TryParseExact(birthdayString, "yyyy-MM-dd", out var birthDate))
                    {
                        context.AddFailure("Błędny format daty. Użyj formatu YYYY-MM-DD.");
                        return;
                    }
                });
        }
    }
}
