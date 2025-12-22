using IDR.Library.BuildingBlocks.CQRS;
using TresorerieService.Application.Features.Budgets.DTOs;

namespace TresorerieService.Application.Features.Budgets.Queries.GetBudgetById;

/// <summary>
/// Query pour recuperer le detail complet d'un budget par son Id
/// Inclut: montants, liste des depenses, repartition par categorie, evolution temporelle
/// </summary>
public record GetBudgetByIdQuery(
    string ApplicationId,
    string BoutiqueId,
    Guid BudgetId
) : IQuery<GetBudgetByIdResponse>;
