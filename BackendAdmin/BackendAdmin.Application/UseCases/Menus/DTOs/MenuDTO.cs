namespace BackendAdmin.Application.UseCases.Menus.DTOs;

public record MenuDTO(Guid Id, string Name, string ApiRoute, string UrlFront, string Icon);
