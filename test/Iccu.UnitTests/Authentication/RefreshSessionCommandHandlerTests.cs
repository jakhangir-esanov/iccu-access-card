namespace Iccu.UnitTests.Authentication;

using Iccu.Domain.Users;
using Iccu.UnitTests.Fakes;
using Iccu.Domain.Common.Enums;
using Iccu.Domain.RefreshTokens;
using Iccu.Application.Abstractions.Authentication;
using Iccu.Application.Authentication.RefreshSession;

public class RefreshSessionCommandHandlerTests
{
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeUserRepository _users = new();
    private readonly FakeRefreshTokenRepository _refreshTokens = new();
    private readonly FakeTokenService _tokenService = new();
    private readonly FakeClock _clock = new();

    private RefreshSessionCommandHandler Handler() => new(
        _unitOfWork,
        _users,
        _refreshTokens,
        _tokenService,
        _clock);

    private (User User, string RefreshToken) SignIn()
    {
        var user = User.Create("dilnoza", "Dilnoza Karimova", UserRole.Receptionist, "hash", _clock.UtcNow);
        _users.Insert(user);

        IssuedTokens tokens = _tokenService.Issue(user, _clock.UtcNow);
        _refreshTokens.Insert(RefreshToken.Issue(
            user.Id, tokens.RefreshTokenHash, _clock.UtcNow, tokens.RefreshTokenExpiresAt));

        return (user, tokens.RefreshToken);
    }

    [Fact]
    public async Task Handle_Should_RotateTheRefreshToken()
    {
        (_, string refreshToken) = SignIn();

        var result = await Handler().Handle(new RefreshSessionCommand(refreshToken), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(refreshToken, result.Data.RefreshToken);
        RefreshToken previous = _refreshTokens.Tokens[0];
        RefreshToken next = _refreshTokens.Tokens[1];
        Assert.Equal(_clock.UtcNow, previous.RevokedAt);
        Assert.Equal(next.Id, previous.ReplacedByTokenId);
        Assert.Null(next.RevokedAt);
        Assert.True(next.ExpiresAt > _clock.UtcNow);
    }

    [Fact]
    public async Task Handle_WhenARotatedTokenIsReused_RevokesEverySessionOfTheUser()
    {
        (_, string stolenToken) = SignIn();
        var rotation = await Handler().Handle(new RefreshSessionCommand(stolenToken), CancellationToken.None);

        var reuse = await Handler().Handle(new RefreshSessionCommand(stolenToken), CancellationToken.None);

        Assert.Equal(UserErrors.InvalidRefreshToken, reuse.Error);
        Assert.All(_refreshTokens.Tokens, token => Assert.NotNull(token.RevokedAt));

        var legitimate = await Handler().Handle(new RefreshSessionCommand(rotation.Data.RefreshToken), CancellationToken.None);
        Assert.True(legitimate.IsFailure);
    }

    [Fact]
    public async Task Handle_WhenTheTokenHasExpired_ReturnsInvalidRefreshToken()
    {
        (_, string refreshToken) = SignIn();
        _clock.UtcNow = _clock.UtcNow.AddDays(8);

        var result = await Handler().Handle(new RefreshSessionCommand(refreshToken), CancellationToken.None);

        Assert.Equal(UserErrors.InvalidRefreshToken, result.Error);
    }

    [Fact]
    public async Task Handle_WhenTheAccountWasDeactivated_ReturnsInvalidRefreshToken()
    {
        (User user, string refreshToken) = SignIn();
        user.Update(user.FullName, user.Role, isActive: false);

        var result = await Handler().Handle(new RefreshSessionCommand(refreshToken), CancellationToken.None);

        Assert.Equal(UserErrors.InvalidRefreshToken, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown-token")]
    public async Task Handle_WithoutAKnownToken_ReturnsInvalidRefreshToken(string? refreshToken)
    {
        var result = await Handler().Handle(new RefreshSessionCommand(refreshToken), CancellationToken.None);

        Assert.Equal(UserErrors.InvalidRefreshToken, result.Error);
    }
}
