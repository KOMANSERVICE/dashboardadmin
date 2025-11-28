using MagasinService.Application.Features.Magasins.Commands.UpdateMagasin;
using MagasinService.Application.Features.Magasins.DTOs;

namespace MagasinService.Api.Endpoints.Magasins;

public record UpdateMagasinRequest(StockLocationUpdateDTO StockLocation);
public record UpdateMagasinResponse(Guid Id);
public class UpdateMagasin : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/magasin/{BoutiqueId}/{StockLocationId}", async (Guid BoutiqueId, Guid StockLocationId, UpdateMagasinRequest request, ISender sender) =>
        {
            var stockLocation = request.StockLocation;
            var command = new UpdateMagasinCommand(stockLocation, BoutiqueId, StockLocationId);

            var result = await sender.Send(command);

            var response = result.Adapt<UpdateMagasinResponse>();
            var baseResponse = ResponseFactory.Success(response, "Magasin modifié avec succèss", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("UpdateMagasin")
       .WithTags("Magasin")
       .Produces<BaseResponse<UpdateMagasinResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .WithSummary("UpdateMagasin")
       .WithDescription("UpdateMagasin");
        //.RequireAuthorization();
    }
}
