using TresorerieService.Application.Features.Categories.DTOs;

namespace TresorerieService.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryHandler(
        IGenericRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork
    )
    : ICommandHandler<CreateCategoryCommand, CreateCategoryResult>
{
    public async Task<CreateCategoryResult> Handle(
        CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        // Vérifier l'unicité du nom pour cette boutique
        var existingCategories = await categoryRepository.GetByConditionAsync(
            c => c.ApplicationId == command.ApplicationId
                 && c.BoutiqueId == command.BoutiqueId
                 && c.Name == command.Name
                 && c.IsActive,
            cancellationToken);

        if (existingCategories.Any())
        {
            throw new BadRequestException($"Une catégorie avec le nom '{command.Name}' existe déjà pour cette boutique");
        }

        // Créer la nouvelle catégorie
        // Note: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy sont gérés automatiquement par IAuditableEntity via UnitOfWork
        var category = new Category
        {
            Id = Guid.NewGuid(),
            ApplicationId = command.ApplicationId,
            BoutiqueId = command.BoutiqueId,
            Name = command.Name,
            Type = command.Type,
            Icon = command.Icon,
            IsActive = true
        };

        await categoryRepository.AddDataAsync(category, cancellationToken);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        var categoryDto = category.Adapt<CategoryDTO>();

        return new CreateCategoryResult(categoryDto);
    }
}
