namespace Iccu.Infrastructure.Authentication;

using System.Text;
using Iccu.Domain.Users;
using System.Buffers.Text;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using Iccu.Application.Abstractions.Authentication;

internal sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    private const int RefreshTokenByteLength = 32;

    private static readonly JsonWebTokenHandler TokenHandler = new();

    public IssuedTokens Issue(User user, DateTime utcNow)
    {
        JwtOptions jwt = options.Value;
        DateTime accessTokenExpiresAt = utcNow.AddMinutes(jwt.AccessTokenMinutes);
        string refreshToken = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(RefreshTokenByteLength));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            IssuedAt = utcNow,
            NotBefore = utcNow,
            Expires = accessTokenExpiresAt,
            Subject = new ClaimsIdentity(
            [
                new Claim(CustomClaims.Sub, user.Id.ToString()),
                new Claim(CustomClaims.Name, user.Username),
                new Claim(CustomClaims.Role, user.Role.ToString()),
                new Claim(CustomClaims.FullName, user.FullName)
            ]),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(jwt.SigningKeyBytes),
                SecurityAlgorithms.HmacSha256)
        };

        return new IssuedTokens(
            TokenHandler.CreateToken(descriptor),
            accessTokenExpiresAt,
            refreshToken,
            HashRefreshToken(refreshToken),
            utcNow.AddDays(jwt.RefreshTokenDays));
    }

    public string HashRefreshToken(string refreshToken) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}
