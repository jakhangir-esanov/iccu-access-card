namespace Iccu.UnitTests.Infrastructure;

using Iccu.Domain.Users;
using Iccu.UnitTests.Fakes;
using Iccu.Domain.Common.Enums;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Iccu.Infrastructure.Authentication;
using Microsoft.IdentityModel.JsonWebTokens;
using Iccu.Application.Abstractions.Authentication;

public class AuthenticationTests
{
    private const string SigningKey = "unit-test-signing-key-that-is-long-enough";

    private readonly UserPasswordHasher _hasher = new();
    private readonly JwtTokenService _tokenService = new(Options.Create(new JwtOptions
    {
        SigningKey = SigningKey,
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7
    }));

    private static User Receptionist() =>
        User.Create("dilnoza", "Dilnoza Karimova", UserRole.Receptionist, "hash", FakeClock.DefaultUtcNow);

    [Fact]
    public void PasswordHasher_Should_VerifyOnlyTheOriginalPassword()
    {
        string hash = _hasher.Hash("Parol2026");

        Assert.NotEqual("Parol2026", hash);
        Assert.True(_hasher.Verify(hash, "Parol2026"));
        Assert.False(_hasher.Verify(hash, "parol2026"));
    }

    [Fact]
    public void PasswordHasher_Should_SaltEveryHash()
    {
        Assert.NotEqual(_hasher.Hash("Parol2026"), _hasher.Hash("Parol2026"));
    }

    [Fact]
    public void TokenService_Should_IssueAnAccessTokenCarryingTheUserClaims()
    {
        User user = Receptionist();

        IssuedTokens tokens = _tokenService.Issue(user, FakeClock.DefaultUtcNow);

        JsonWebToken jwt = new JsonWebTokenHandler().ReadJsonWebToken(tokens.AccessToken);
        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Equal("dilnoza", jwt.GetClaim("name").Value);
        Assert.Equal(nameof(UserRole.Receptionist), jwt.GetClaim("role").Value);
        Assert.Equal(FakeClock.DefaultUtcNow.AddMinutes(15), tokens.AccessTokenExpiresAt);
    }

    [Fact]
    public void TokenService_Should_IssueUniqueRefreshTokensWithAStableHash()
    {
        User user = Receptionist();

        IssuedTokens first = _tokenService.Issue(user, FakeClock.DefaultUtcNow);
        IssuedTokens second = _tokenService.Issue(user, FakeClock.DefaultUtcNow);

        Assert.NotEqual(first.RefreshToken, second.RefreshToken);
        Assert.Equal(first.RefreshTokenHash, _tokenService.HashRefreshToken(first.RefreshToken));
        Assert.Equal(64, first.RefreshTokenHash.Length);
        Assert.Equal(FakeClock.DefaultUtcNow.AddDays(7), first.RefreshTokenExpiresAt);
    }

    [Fact]
    public void JwtOptions_WithAShortSigningKey_FailValidation()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:SigningKey"] = "too-short" })
            .Build();

        using ServiceProvider provider = new ServiceCollection()
            .AddSingleton(configuration)
            .AddAuthenticationInternal()
            .BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<JwtOptions>>().Value);
    }
}
