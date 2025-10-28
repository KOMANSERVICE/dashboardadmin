using BackendAdmin.Application.Interfaces;
using BackendAdmin.Application.UseCases.AppAdmins.DTOs;

namespace BackendAdmin.Application.UseCases.AppAdmins.Commands.CreateAppAdmin;

public class CreateAppAdminHandler(
        IGenericRepository<AppAdmin> _genericRepository,
        IUnitOfWork _unitOfWork
    )
    : ICommandHandler<CreateAppAdminCommand, CreateAppAdminResult>
{
    public async Task<CreateAppAdminResult> Handle(CreateAppAdminCommand request, CancellationToken cancellationToken)
    {


        var appAdmin = CreateNewApp(request.AppAdmin);
        await _genericRepository.AddDataAsync(appAdmin, cancellationToken);
        await _unitOfWork.SaveChangesDataAsync(cancellationToken);


        return new CreateAppAdminResult(appAdmin.Id.Value);
    }

    private AppAdmin CreateNewApp(AppAdminDTO AppAdmin)
    {
        return new AppAdmin
        {
            Name = AppAdmin.Name,
            Reference = AppAdmin.Reference,
            Description = AppAdmin.Description,
            Link = AppAdmin.Link
        };
    }
}
