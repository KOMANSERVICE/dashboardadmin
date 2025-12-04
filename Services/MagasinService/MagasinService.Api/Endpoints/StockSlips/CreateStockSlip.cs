using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using MagasinService.Application.Features.StockSlips.Commands.CreateStockSlip;
using Mapster;

namespace MagasinService.Api.Endpoints.StockSlips;

public record CreateStockSlipRequest(
    Guid BoutiqueId,
    Guid SourceLocationId,
    Guid DestinationLocationId,
    string Note,
    bool IsInbound,
    List<StockSlipItemRequest> Items);

public record StockSlipItemRequest(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    string Note);

public record CreateStockSlipApiResponse(Guid Id, string Reference, int ItemsCount);

public class CreateStockSlip : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/stock-slips", async (CreateStockSlipRequest request, ISender sender) =>
        {
            var command = new CreateStockSlipCommand
            {
                BoutiqueId = request.BoutiqueId,
                SourceLocationId = request.SourceLocationId,
                DestinationLocationId = request.DestinationLocationId,
                Note = request.Note,
                IsInbound = request.IsInbound,
                Items = request.Items.Select(i => new StockSlipItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Note = i.Note
                }).ToList()
            };

            var result = await sender.Send(command) as CreateStockSlipResponse;

            if (result == null || !result.Success)
            {
                var errorMessage = result?.Message ?? "Erreur lors de la création du bordereau";
                var errorResponse = ResponseFactory.Success(new CreateStockSlipApiResponse(Guid.Empty, string.Empty, 0), errorMessage, StatusCodes.Status400BadRequest);
                return Results.BadRequest(errorResponse);
            }

            var response = new CreateStockSlipApiResponse(result.Id, result.Reference, result.ItemsCount);
            var baseResponse = ResponseFactory.Success(response, "Bordereau de mouvement créé avec succès", StatusCodes.Status201Created);

            return Results.Created($"/stock-slips/{result.Id}", baseResponse);
        })
        .WithName("CreateStockSlip")
        .WithTags("Stock Slips")
        .Produces<BaseResponse<CreateStockSlipApiResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Créer un bordereau de mouvement")
        .WithDescription("Permet de créer un bordereau de mouvement (entrée ou sortie) avec plusieurs produits");
        //.RequireAuthorization();
    }
}