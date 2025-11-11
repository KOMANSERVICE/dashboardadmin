using Grpc.Core;
using IDR.Library.BuildingBlocks.Repositories;
using Mapster;
using Menu.Grpc.Models;

namespace Menu.Grpc.Services;

public class MenuService(
        IGenericRepository<MenuNav> menuRepository,
        IUnitOfWork unitOfWork
    ) : MenuProtoService.MenuProtoServiceBase
{
    public override async Task<MenuModel> GetMenu(GetMenuRequest request, ServerCallContext context)
    {
        var menu = await menuRepository.FindAsync(m => m.Name == request.Name);
       
        var menuModel = menu.Adapt<MenuModel>();
        return menuModel;
    }

    public override async Task<GetAllMenuResponse> GetAllMenu(GetAllMenuRequest request, ServerCallContext context)
    {
        var menus = await menuRepository.GetAllAsync();

        var result = menus.Select(m => m.Adapt<MenuModel>()).ToList();
        var response = new GetAllMenuResponse();
        response.Menus.AddRange(result);

        return response;
    }

    public override async Task<MenuModel> CreateMenu(CreateMenuRequest request, ServerCallContext context)
    {

        if(request.Menu is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Menu data is required"));

        var menuEntity = new MenuNav
        {
            Id = Guid.NewGuid(),
            IsActif = true,
            Name = request.Menu.Name,
            ApiRoute = request.Menu.ApiRoute,
            UrlFront = request.Menu.UrlFront,
            Icon = request.Menu.Icon
        };

        await menuRepository.AddDataAsync(menuEntity);
        await unitOfWork.SaveChangesDataAsync();

        var menuModel = menuEntity.Adapt<MenuModel>();

        return menuModel;
    }

    public override async Task<MenuModel> UpdateMenu(UpdateMenuRequest request, ServerCallContext context)
    {

        var menuEntity = request.Menu.Adapt<MenuNav>();
        if (menuEntity is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Menu data is required"));

        menuRepository.UpdateData(menuEntity);
        await unitOfWork.SaveChangesDataAsync();

        var menuModel = menuEntity.Adapt<MenuModel>();
        return menuModel;
    }

    public override async Task<ActiveMenuResponse> ActiveMenu(ActiveMenuRequest request, ServerCallContext context)
    {
        var menu = await menuRepository.FindAsync(m => m.Name == request.Reference);
        if (menu is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Menu data not found"));

        menu.IsActif = true;
        menuRepository.UpdateData(menu);
        await unitOfWork.SaveChangesDataAsync();

        return new ActiveMenuResponse { Success = true};
    }

    public override async Task<ActiveMenuResponse> DeActiveMenu(ActiveMenuRequest request, ServerCallContext context)
    {
        var menu = await menuRepository.FindAsync(m => m.Name == request.Reference);
        if (menu is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Menu data not found"));

        menu.IsActif = false;
        menuRepository.UpdateData(menu);
        await unitOfWork.SaveChangesDataAsync();

        return new ActiveMenuResponse { Success = true };
    }
}
