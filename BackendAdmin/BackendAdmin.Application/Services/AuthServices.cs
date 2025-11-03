using IDR.Library.BuildingBlocks.Helpers;
using IDR.Library.BuildingBlocks.Helpers.Models;
using IDR.Library.BuildingBlocks.Security.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BackendAdmin.Application.Services;

public class AuthServices(
    IConfiguration _configuration,
    ISecureSecretProvider _secureSecretProvider,
    IHttpContextAccessor _httpContextAccessor
) 
{
    public async Task<AccessToken> GetTokenAsync(JwtTokenModel jwtTokenModel)
    {
        var JWT_Secret = _configuration["JWT:Secret"]!;
        var JWT_ValidIssuer = _configuration["JWT:ValidIssuer"]!;
        var JWT_ValidAudience = _configuration["JWT:ValidAudience"]!;

        var secret = await _secureSecretProvider.GetSecretAsync(JWT_Secret);
        var issuer = await _secureSecretProvider.GetSecretAsync(JWT_ValidIssuer);
        var audience = await _secureSecretProvider.GetSecretAsync(JWT_ValidAudience);
        var jwtToken = new JwtTokenModel
        {
            Email = jwtTokenModel.Email,
            UserId = jwtTokenModel.UserId,
            Role = jwtTokenModel.Role,
            JwtSecret = secret,
            JwtIssuer = issuer,
            JwtAudience = audience,
            Expiration = DateTime.Now.AddMinutes(1)
        };
        var token = AuthHelper.GetTokenAsync(jwtToken);

        var httpContext = _httpContextAccessor.HttpContext;
        var refreshToken = AuthHelper.GenerateRefreshToken(
                httpContext: httpContext,
                expiration: DateTime.Now.AddDays(7)
            );
        return new AccessToken
        {
            Token = token,
            RefreshToken = refreshToken
        };
    }
}
