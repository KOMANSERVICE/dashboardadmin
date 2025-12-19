using MenuService.Application.Data;

namespace MenuService.Application.Features.Menus.Commands.ReorderMenus;

public class ReorderMenusHandler(
        IMenuDbContext _dbContext,
        IUnitOfWork unitOfWork
    )
    : ICommandHandler<ReorderMenusCommand, ReorderMenusResult>
{
    public async Task<ReorderMenusResult> Handle(ReorderMenusCommand request, CancellationToken cancellationToken)
    {
        var references = request.Items.Select(i => i.Reference).ToList();

        var menus = await _dbContext.Menus
            .Where(m => m.AppAdminReference == request.AppAdminReference && references.Contains(m.Reference))
            .ToListAsync(cancellationToken);

        if (menus.Count != request.Items.Count)
        {
            var foundRefs = menus.Select(m => m.Reference).ToHashSet();
            var missingRefs = references.Where(r => !foundRefs.Contains(r)).ToList();
            throw new NotFoundException($"Menus not found: {string.Join(", ", missingRefs)}");
        }

        var sortOrderMap = request.Items.ToDictionary(i => i.Reference, i => i.SortOrder);

        foreach (var menu in menus)
        {
            menu.SortOrder = sortOrderMap[menu.Reference];
        }

        await unitOfWork.SaveChangesDataAsync();

        return new ReorderMenusResult(true, menus.Count);
    }
}
