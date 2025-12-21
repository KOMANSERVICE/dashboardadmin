namespace BackendAdmin.Application.Features.Swarm.Commands.UpdateServiceResources;

public class UpdateServiceResourcesValidator : AbstractValidator<UpdateServiceResourcesCommand>
{
    public UpdateServiceResourcesValidator()
    {
        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .WithMessage("Le nom du service est requis");

        RuleFor(x => x.CpuLimit)
            .GreaterThan(0)
            .When(x => x.CpuLimit.HasValue)
            .WithMessage("La limite CPU doit etre positive");

        RuleFor(x => x.CpuReservation)
            .GreaterThan(0)
            .When(x => x.CpuReservation.HasValue)
            .WithMessage("La reservation CPU doit etre positive");

        RuleFor(x => x)
            .Must(x => !x.CpuLimit.HasValue || !x.CpuReservation.HasValue || x.CpuReservation <= x.CpuLimit)
            .WithMessage("La reservation CPU ne peut pas depasser la limite CPU");

        RuleFor(x => x.MemoryLimit)
            .Matches(@"^(\d+(\.\d+)?)(KB|K|MB|M|GB|G)?$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            .When(x => !string.IsNullOrEmpty(x.MemoryLimit))
            .WithMessage("Format de memoire invalide (ex: 512MB, 1GB)");

        RuleFor(x => x.MemoryReservation)
            .Matches(@"^(\d+(\.\d+)?)(KB|K|MB|M|GB|G)?$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            .When(x => !string.IsNullOrEmpty(x.MemoryReservation))
            .WithMessage("Format de memoire invalide (ex: 512MB, 1GB)");

        RuleFor(x => x.PidsLimit)
            .GreaterThan(0)
            .When(x => x.PidsLimit.HasValue)
            .WithMessage("La limite de PIDs doit etre positive");

        RuleFor(x => x.BlkioWeight)
            .InclusiveBetween(1, 1000)
            .When(x => x.BlkioWeight.HasValue)
            .WithMessage("Le poids I/O doit etre entre 1 et 1000");

        RuleForEach(x => x.Ulimits)
            .ChildRules(ulimit =>
            {
                ulimit.RuleFor(u => u.Name)
                    .NotEmpty()
                    .WithMessage("Le nom de l'ulimit est requis");

                ulimit.RuleFor(u => u.Soft)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("La valeur soft doit etre positive ou zero");

                ulimit.RuleFor(u => u.Hard)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("La valeur hard doit etre positive ou zero");

                ulimit.RuleFor(u => u)
                    .Must(u => u.Soft <= u.Hard)
                    .WithMessage("La valeur soft ne peut pas depasser la valeur hard");
            })
            .When(x => x.Ulimits != null && x.Ulimits.Any());
    }
}
