using BackendAdmin.Application.Features.Auths.DTOs;

namespace BackendAdmin.Application.Features.Auths.Commands.SignIn;

public record SignInCommand(SignInDTO Signin)
    : ICommand<SignInResult>;

public record SignInResult(string Token);

public class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(x => x.Signin)
            .NotNull()
            .WithMessage("Le mot de passe/l'adresse email est incorrect.");
    }
}
