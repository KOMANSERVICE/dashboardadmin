using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using MagasinService.Application.Features.Magasins.Commands.CreateMagasin;
using MagasinService.Application.Features.Magasins.DTOs;
using Mapster;

namespace MagasinService.Api.Endpoints.Magasins;

public record CreateMagasinRequest(StockLocationDTO StockLocation);
public record CreateMagasinResponse(Guid Id);
public class CreateMagasin : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/magasin/{BoutiqueId}", async (Guid BoutiqueId, CreateMagasinRequest request, ISender sender) =>
        {
            var stockLocation = request.StockLocation;
            var command = new CreateMagasinCommand(stockLocation,BoutiqueId);

            var result = await sender.Send(command);

            var response = result.Adapt<CreateMagasinResponse>();
            var baseResponse = ResponseFactory.Success(response, "Magasin créer avec succèss", StatusCodes.Status201Created);

            return Results.Created($"/magasin", baseResponse);
        })
       .WithName("CreateMagasin")
       .WithTags("Application")
       .Produces<BaseResponse<CreateMagasinResponse>>(StatusCodes.Status201Created)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .WithSummary("CreateMagasin")
       .WithDescription("CreateMagasin");
       //.RequireAuthorization();
    }
}
