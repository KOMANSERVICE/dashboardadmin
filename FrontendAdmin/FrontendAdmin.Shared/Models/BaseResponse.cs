namespace FrontendAdmin.Shared.Models;
public record BaseResponse<T>(
    T Data,
    string Message,
    int StatusCode,
    DateTime Timestamp,
    bool Success = true
);

