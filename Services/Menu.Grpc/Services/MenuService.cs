using Grpc.Core;

namespace Menu.Grpc.Services;

public class MenuService : MenuProtoService.MenuProtoServiceBase
{
    public override Task<MenuModel> GetMenu(GetMenuRequest request, ServerCallContext context)
    {
        //var response = new MenuModel
        //{
        //    MenuJson = "{\"items\":[{\"id\":1,\"name\":\"Home\",\"url\":\"/home\"},{\"id\":2,\"name\":\"About\",\"url\":\"/about\"}]}"
        //};
        //return Task.FromResult(response);
        return base.GetMenu(request, context);
    }

    public override Task<MenuModel> CreateMenu(CreateMenuRequest request, ServerCallContext context)
    {
        return base.CreateMenu(request, context);
    }

    public override Task<MenuModel> UpdateMenu(UpdateMenuRequest request, ServerCallContext context)
    {
        return base.UpdateMenu(request, context);
    }

    public override Task<DeleteMenuResponse> DeleteMenu(DeleteMenuRequest request, ServerCallContext context)
    {
        return base.DeleteMenu(request, context);
    }
}
