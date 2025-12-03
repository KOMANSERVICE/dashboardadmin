using BackendAdmin.Application.Features.Auths.Commands.SignIn;
using BackendAdmin.Application.Features.Auths.DTOs;

namespace BackendAdmin.Api.Endpoints.Auths;

public record SignInRequest(SignInDTO Signin);
public record SignInResponse(string Token);
public class SignIn : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/signin", async (SignInRequest request, ISender sender) =>
        {
            var query = request.Adapt<SignInCommand>();

            var result = await sender.Send(query);

            var response = result.Adapt<SignInResponse>();
            var baseResponse = ResponseFactory.Success(response, "Connexion réussi", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("signin")
       .WithTags("Auth")
       .Produces<BaseResponse<SignInResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .WithSummary("SignIn")
       .WithDescription("SignIn")
       .WithOpenApi();
    }
}
