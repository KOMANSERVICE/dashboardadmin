using BackendAdmin.Application.Data;
using BackendAdmin.Application.UseCases.AppAdmins.DTOs;

namespace BackendAdmin.Application.UseCases.AppAdmins.Commands.CreateAppAdmin;

public class CreateAppAdminHandler(
        IGenericRepository<AppAdmin> _genericRepository,
        IApplicationDbContext _applicationDbContext,
        IUnitOfWork _unitOfWork
    )
    : ICommandHandler<CreateAppAdminCommand, CreateAppAdminResult>
{
    public async Task<CreateAppAdminResult> Handle(CreateAppAdminCommand request, CancellationToken cancellationToken)
    {
        var appAdminDto = request.AppAdmin;
        var existe = await _applicationDbContext.Applications
            .AnyAsync(a => a.Reference == appAdminDto.Reference);

        if (existe)
            throw new BadRequestException("Ce code existe déjà");

        var appAdmin = CreateNewApp(appAdminDto);
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
