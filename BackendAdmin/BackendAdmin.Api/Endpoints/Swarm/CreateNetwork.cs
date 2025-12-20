using BackendAdmin.Application.Features.Swarm.Commands.CreateNetwork;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record CreateNetworkApiRequest(
    string Name,
    string Driver = "overlay",
    bool IsAttachable = true,
    string? Subnet = null,
    string? Gateway = null,
    string? IpRange = null,
    Dictionary<string, string>? Labels = null,
    Dictionary<string, string>? Options = null
);

public record CreateNetworkResponse(string NetworkId, string NetworkName);

public class CreateNetwork : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/networks", async (CreateNetworkApiRequest request, ISender sender) =>
        {
            var command = new CreateNetworkCommand(
                Name: request.Name,
                Driver: request.Driver,
                IsAttachable: request.IsAttachable,
                Subnet: request.Subnet,
                Gateway: request.Gateway,
                IpRange: request.IpRange,
                Labels: request.Labels,
                Options: request.Options
            );

            var result = await sender.Send(command);
            var response = result.Adapt<CreateNetworkResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Reseau '{request.Name}' cree avec succes",
                StatusCodes.Status201Created);

            return Results.Created($"/api/swarm/networks/{request.Name}", baseResponse);
        })
        .WithName("CreateNetwork")
        .WithTags("Swarm", "Networks")
        .Produces<BaseResponse<CreateNetworkResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Cree un nouveau reseau Docker")
        .WithDescription("Cree un nouveau reseau Docker avec le driver et la configuration IPAM specifies. Par defaut, cree un reseau overlay attachable.")
        .RequireAuthorization();
    }
}
