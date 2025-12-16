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
        // Vérifier l'unicité du nom pour cette application
        var existingCategories = await categoryRepository.GetByConditionAsync(
            c => c.ApplicationId == command.ApplicationId
                 && c.Name == command.Name
                 && c.IsActive,
            cancellationToken);

        if (existingCategories.Any())
        {
            throw new BadRequestException($"Une catégorie avec le nom '{command.Name}' existe déjà pour cette application");
        }

        // Créer la nouvelle catégorie
        var category = new Category
        {
            Id = Guid.NewGuid(),
            ApplicationId = command.ApplicationId,
            Name = command.Name,
            Type = command.Type,
            Icon = command.Icon,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            UpdatedBy = "system"
        };

        await categoryRepository.AddDataAsync(category, cancellationToken);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        var categoryDto = category.Adapt<CategoryDTO>();

        return new CreateCategoryResult(categoryDto);
    }
}
