using BackendAdmin.Application.UseCases.AppAdmins.DTOs;

namespace BackendAdmin.Application.UseCases.AppAdmins.Commands.CreateAppAdmin;

public record CreateAppAdminCommand(AppAdminDTO AppAdmin)
    : ICommand<CreateAppAdminResult>;

public record CreateAppAdminResult(Guid Id);

public class CreateAppAdminValidator : AbstractValidator<CreateAppAdminCommand>
{
    public CreateAppAdminValidator()
    {
        RuleFor(x => x.AppAdmin).NotNull().WithMessage("L'application est requise.");
        RuleFor(x => x.AppAdmin.Name)
            .NotEmpty().WithMessage("Le nom de l'application est requis.")
            .MaximumLength(100).WithMessage("Le nom de l'application ne doit pas dépasser 100 caractères.");
        RuleFor(x => x.AppAdmin.Reference)
            .NotEmpty().WithMessage("La référence de l'application est requise.")
            .MaximumLength(50).WithMessage("La référence de l'application ne doit pas dépasser 50 caractères.");
        RuleFor(x => x.AppAdmin.Description)
            .MaximumLength(500).WithMessage("La description de l'application ne doit pas dépasser 500 caractères.");
        RuleFor(x => x.AppAdmin.Link)
            .MaximumLength(200).WithMessage("Le lien de l'application ne doit pas dépasser 200 caractères.");
    }
}