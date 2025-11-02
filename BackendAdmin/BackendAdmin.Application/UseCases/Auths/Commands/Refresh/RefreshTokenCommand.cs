namespace BackendAdmin.Application.UseCases.Auths.Commands.Refresh;

public record RefreshTokenCommand()
    : ICommand<RefreshTokenResult>;

public record RefreshTokenResult(string AccessToken);
