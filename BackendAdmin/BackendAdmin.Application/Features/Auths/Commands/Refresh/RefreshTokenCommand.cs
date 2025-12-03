namespace BackendAdmin.Application.Features.Auths.Commands.Refresh;

public record RefreshTokenCommand(bool RemenberMe)
    : ICommand<RefreshTokenResult>;

public record RefreshTokenResult(string AccessToken);
