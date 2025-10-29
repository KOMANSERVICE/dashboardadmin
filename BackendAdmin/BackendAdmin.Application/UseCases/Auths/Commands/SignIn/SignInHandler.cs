using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BackendAdmin.Application.UseCases.Auths.Commands.SignIn;

public class SignInHandler(
    IConfiguration _configuration,
    ISecureSecretProvider _secureSecretProvider
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
            var authClains = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, EmailAdmin),
                        new Claim(ClaimTypes.Upn, EmailAdmin),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Role, "DashbordAdmin")
                    };
            var token = await GetTokenAsync(authClains);
            return new SignInResult(token);

        }

        throw new BadRequestException("Le mot de passe ou l'adresse email est incorrect.");
    }

    private async Task<string> GetTokenAsync(List<Claim> authClains)
    {
        var JWT_Secret = _configuration["JWT:Secret"]!;
        var JWT_ValidIssuer = _configuration["JWT:ValidIssuer"]!;
        var JWT_ValidAudience = _configuration["JWT:ValidAudience"]!;

        var secret = await _secureSecretProvider.GetSecretAsync(JWT_Secret);
        var issuer = await _secureSecretProvider.GetSecretAsync(JWT_ValidIssuer);
        var audience = await _secureSecretProvider.GetSecretAsync(JWT_ValidAudience);
        
        var authSigninkey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            expires: DateTime.Now.AddMinutes(30),
            claims: authClains,
            signingCredentials: new SigningCredentials(authSigninkey, SecurityAlgorithms.HmacSha256Signature)
        );

        var strToken = new JwtSecurityTokenHandler().WriteToken(token);

        return strToken;
    }
}
