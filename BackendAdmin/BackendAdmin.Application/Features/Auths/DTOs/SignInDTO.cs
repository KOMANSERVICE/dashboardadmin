namespace BackendAdmin.Application.Features.Auths.DTOs;

public record SignInDTO(string Email, string Password, bool RememberMe);