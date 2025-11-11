using Grpc.Core;
using Mapster;
using MediatR;
using MenuService.Application.Features.Menus.Commands.CreateMenu;

namespace MenuService.Grpc.Services;

public class MenuGrpcService(
        IMediator _mediator
    ) : MenuProtoService.MenuProtoServiceBase
{

    public override async Task<CreateMenuResponse> CreateMenu(CreateMenuRequest request, ServerCallContext context)
    {
        var command = request.Adapt<CreateMenuCommand>();
        var result = await _mediator.Send(command);

        return new CreateMenuResponse
        {
            Id = result.Id.ToString()
        };
    }

    public override Task<GetAllMenuResponse> GetAllMenu(GetAllMenuRequest request, ServerCallContext context)
    {
        return base.GetAllMenu(request, context);
    }
}
