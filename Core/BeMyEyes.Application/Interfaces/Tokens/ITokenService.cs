using BeMyEyes.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BeMyEyes.Application.Tokens
{
    public interface ITokenService
    {
        Task<string> CreateToken(User user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken();
    }
}
