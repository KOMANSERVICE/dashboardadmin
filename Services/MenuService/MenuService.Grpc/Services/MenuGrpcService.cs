using Grpc.Core;
using Mapster;
using MediatR;
using MenuService.Application.Features.Menus.Commands.CreateMenu;
using MenuService.Application.Features.Menus.Queries.GetAllActifMenu;
using MenuService.Application.Features.Menus.Queries.GetAllMenu;

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

    public override async Task<GetAllMenuResponse> GetAllMenu(GetAllMenuRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetAllMenuQuery());
        var menus = result.Menus.Select(m => m.Adapt<MenuSateModel>()).ToList();
        var response = new GetAllMenuResponse();
        response.Menus.AddRange(menus);
        return response;
    }

    public override async Task<GetAllActifMenuResponse> GetAllActifMenu(GetAllMenuRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetAllActifMenuQuery());
        var menus = result.Menus.Select(m => m.Adapt<MenuModel>()).ToList();
        var response = new GetAllActifMenuResponse();
        response.Menus.AddRange(menus);
        return response;
    }

    public override Task<UpdateMenuResponse> UpdateMenu(UpdateMenuRequest request, ServerCallContext context)
    {
        return base.UpdateMenu(request, context);
    }

    public override Task<ActiveMenuResponse> ActiveMenu(ActiveMenuRequest request, ServerCallContext context)
    {
        return base.ActiveMenu(request, context);
    }

    public override Task<ActiveMenuResponse> InactiveMenu(ActiveMenuRequest request, ServerCallContext context)
    {
        return base.InactiveMenu(request, context);
    }
}
