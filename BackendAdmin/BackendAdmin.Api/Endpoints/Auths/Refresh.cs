using BackendAdmin.Application.Features.Auths.Commands.Refresh;

namespace BackendAdmin.Api.Endpoints.Auths;

public record RefreshTokenRequest(bool RemenberMe);
public record RefreshTokenResponse(string AccessToken);

public class Refresh : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh", async (RefreshTokenRequest request,ISender sender) =>
        {

            var query = request.Adapt<RefreshTokenCommand>();

            var result = await sender.Send(query);

            var response = result.Adapt<RefreshTokenResponse>();
            var baseResponse = ResponseFactory.Success(response, "Token rafraîchi", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);

        })
        .WithName("RefreshToken")
        .WithTags("Auth")
        .AllowAnonymous()
        .Produces<BaseResponse<RefreshTokenResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("RefreshToken")
        .WithDescription("RefreshToken");
    }
}
