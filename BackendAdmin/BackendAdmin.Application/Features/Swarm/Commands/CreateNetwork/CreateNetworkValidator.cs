using FluentValidation;

namespace BackendAdmin.Application.Features.Swarm.Commands.CreateNetwork;

public class CreateNetworkValidator : AbstractValidator<CreateNetworkCommand>
{
    private static readonly string[] AllowedDrivers = { "bridge", "overlay", "host", "none", "macvlan" };

    public CreateNetworkValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Le nom du reseau est requis")
            .Matches(@"^[a-zA-Z0-9][a-zA-Z0-9_.-]*$")
            .WithMessage("Le nom du reseau doit commencer par une lettre ou un chiffre et ne contenir que des lettres, chiffres, tirets, points ou underscores");

        RuleFor(x => x.Driver)
            .Must(driver => AllowedDrivers.Contains(driver.ToLowerInvariant()))
            .WithMessage($"Le driver doit etre l'un des suivants: {string.Join(", ", AllowedDrivers)}");

        RuleFor(x => x.Subnet)
            .Matches(@"^(\d{1,3}\.){3}\d{1,3}/\d{1,2}$")
            .When(x => !string.IsNullOrEmpty(x.Subnet))
            .WithMessage("Le subnet doit etre au format CIDR (ex: 172.20.0.0/16)");

        RuleFor(x => x.Gateway)
            .Matches(@"^(\d{1,3}\.){3}\d{1,3}$")
            .When(x => !string.IsNullOrEmpty(x.Gateway))
            .WithMessage("Le gateway doit etre une adresse IP valide (ex: 172.20.0.1)");

        RuleFor(x => x.IpRange)
            .Matches(@"^(\d{1,3}\.){3}\d{1,3}/\d{1,2}$")
            .When(x => !string.IsNullOrEmpty(x.IpRange))
            .WithMessage("L'IP range doit etre au format CIDR (ex: 172.20.0.0/24)");
    }
}
