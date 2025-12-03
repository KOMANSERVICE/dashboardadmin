using BackendAdmin.Application.Services;
using BackendAdmin.Domain.Models;
using IDR.Library.BuildingBlocks.Helpers;
using IDR.Library.BuildingBlocks.Helpers.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendAdmin.Application.Features.Auths.Commands.SignIn;

public class SignInHandler(
    IConfiguration _configuration,
    ISecureSecretProvider _secureSecretProvider,
    AuthServices _authServices,
    IGenericRepository<RefreshToken> _refreshTokenContext,
    IUnitOfWork _unitOfWork
    ) :
    ICommandHandler<SignInCommand, SignInResult>
{
    public async Task<SignInResult> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var signIn = request.Signin;

        var Vault_EmailAdmin = _configuration["Vault:EmailAdmin"]!;
        var Vault_PasswordAdmin = _configuration["Vault:PasswordAdmin"]!;

        var EmailAdmin = await _secureSecretProvider.GetSecretAsync(Vault_EmailAdmin);
        var PasswordAdmin = await _secureSecretProvider.GetSecretAsync(Vault_PasswordAdmin);

        if (signIn.Email.Equals(EmailAdmin) && signIn.Password.Equals(PasswordAdmin))
        {
            var jwtToken = new JwtTokenModel
            {
                Email = EmailAdmin,
                UserId  = EmailAdmin,
                Role = "DashbordAdmin",
            };

            var resultToken = await _authServices.GetTokenAsync(jwtToken, signIn.RememberMe);
            
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenHash = resultToken.RefreshTokenHash,
                Email = jwtToken.Email,
                UserId = jwtToken.UserId, // Ou un vrai UserId si vous en avez
                Role = jwtToken.Role,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = resultToken.RefreshTokenExpiration,
                IsRevoked = false
            };

            await _refreshTokenContext.AddDataAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesDataAsync(cancellationToken);

            return new SignInResult(resultToken.Token);

        }

        throw new BadRequestException("Le mot de passe ou l'adresse email est incorrect.");
    }

}
