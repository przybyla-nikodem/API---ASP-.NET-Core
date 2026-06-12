using API___ASP_.NET_Core.Models;
using FluentValidation;
using System.Text.RegularExpressions;

namespace API___ASP_.NET_Core
{
    public class UserEditValidator : AbstractValidator<Edit>
    {
        public UserEditValidator()
        {
            RuleFor(x => x.Username)
                .NotNull().NotEmpty().WithMessage("Nazwa użytkownika nie może być pusta.");

            RuleFor(x => x.Password)
                .NotNull().NotEmpty().WithMessage("Hasło nie może być puste.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Format nowego adresu email jest niepoprawny.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Name)
                .Matches(@"^[a-zA-ZąęćńóśłźżĄĘĆŃÓŚŁŹŻ]+$").WithMessage("Niepoprawne znaki w nowym imieniu.")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Surname)
                .Matches(@"^[a-zA-ZąęćńóśłźżĄĘĆŃÓŚŁŹŻ]+$").WithMessage("Niepoprawne znaki w nowym nazwisku.")
                .When(x => !string.IsNullOrEmpty(x.Surname));

            RuleFor(x => x.Birthday)
                .Custom((birthdayString, context) =>
                {
                    if (!DateOnly.TryParseExact(birthdayString, "yyyy-MM-dd", out _))
                    {
                        context.AddFailure("Błędny format daty. Użyj formatu YYYY-MM-DD.");
                    }
                })
                .When(x => !string.IsNullOrEmpty(x.Birthday));
        }
    }
}