using Grpc.Core;
using Mapster;
using MediatR;
using MenuService.Application.Features.Menus.Commands.ActiveMenu;
using MenuService.Application.Features.Menus.Commands.CreateMenu;
using MenuService.Application.Features.Menus.Commands.InactiveMenu;
using MenuService.Application.Features.Menus.Commands.UpdateMenu;
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
        var query = request.Adapt<GetAllMenuQuery>();
        var result = await _mediator.Send(query);
        var menus = result.Menus.Select(m => m.Adapt<MenuSateModel>()).ToList();
        var response = new GetAllMenuResponse();
        response.Menus.AddRange(menus);
        return response;
    }

    public override async Task<GetAllByAppMenuResponse> GetAllByAppMenu(GetAllByAppMenuRequest request, ServerCallContext context)
    {
        var query = request.Adapt<GetAllActifMenuQuery>();
        var result = await _mediator.Send(query);
        var menus = result.Menus.Select(m => m.Adapt<MenuModel>()).ToList();
        var response = new GetAllByAppMenuResponse();
        response.Menus.AddRange(menus);
        return response;
    }

    public override async Task<UpdateMenuResponse> UpdateMenu(UpdateMenuRequest request, ServerCallContext context)
    {
        var command = request.Adapt<UpdateMenuCommand>();
        var result = await _mediator.Send(command);

        var menu = result.Adapt<UpdateMenuResponse>();

        return menu;
    }

    public override async Task<ActiveMenuResponse> ActiveMenu(ActiveMenuRequest request, ServerCallContext context)
    {
        var command = request.Adapt<ActiveMenuCommand>();
        var result = await _mediator.Send(command);
        var success = result.Adapt<ActiveMenuResponse>();

        return success;
    }

    public override async Task<ActiveMenuResponse> InactiveMenu(ActiveMenuRequest request, ServerCallContext context)
    {

        var command = request.Adapt<InactiveMenuCommand>();
        var result = await _mediator.Send(command);
        var success = result.Adapt<ActiveMenuResponse>();
        return success;
    }
}
