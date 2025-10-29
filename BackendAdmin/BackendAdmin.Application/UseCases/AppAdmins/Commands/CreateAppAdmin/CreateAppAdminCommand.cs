using BackendAdmin.Application.UseCases.AppAdmins.DTOs;

namespace BackendAdmin.Application.UseCases.AppAdmins.Commands.CreateAppAdmin;

public record CreateAppAdminCommand(AppAdminDTO AppAdmin)
    : ICommand<CreateAppAdminResult>;

public record CreateAppAdminResult(Guid Id);
