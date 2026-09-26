namespace Iccu.UnitTests.Authentication;

using Iccu.Domain.Users;
using Iccu.UnitTests.Fakes;
using Iccu.Domain.Common.Enums;
using Iccu.Domain.RefreshTokens;
using Iccu.Application.Authentication.Login;

public class LoginCommandHandlerTests
{
    private const string Password = "Parol2026";
    private const int MaxFailedAttempts = 5;

    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeUserRepository _users = new();
    private readonly FakeRefreshTokenRepository _refreshTokens = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeClock _clock = new();

    private LoginCommandHandler Handler() => new(
        _unitOfWork,
        _users,
        _refreshTokens,
        _passwordHasher,
        new FakeTokenService(),
        _clock);

    private User AddUser(bool isActive = true)
    {
        var user = User.Create("dilnoza", "Dilnoza Karimova", UserRole.Receptionist, _passwordHasher.Hash(Password), _clock.UtcNow);
        user.Update(user.FullName, user.Role, isActive);
        _users.Insert(user);
        return user;
    }

    private async Task FailRepeatedly(int attempts)
    {
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            await Handler().Handle(new LoginCommand("dilnoza", "wrong"), CancellationToken.None);
        }
    }

    [Fact]
    public async Task Handle_WithValidCredentials_IssuesASessionAndStoresOnlyTheRefreshTokenHash()
    {
        User user = AddUser();

        var result = await Handler().Handle(new LoginCommand(" Dilnoza ", Password), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("access-for-dilnoza", result.Data.AccessToken);
        Assert.Equal(user.Id, result.Data.User.Id);
        RefreshToken stored = Assert.Single(_refreshTokens.Tokens);
        Assert.Equal($"hash:{result.Data.RefreshToken}", stored.TokenHash);
        Assert.Equal(user.Id, stored.UserId);
        Assert.Equal(_clock.UtcNow, user.LastLoginAt);
    }

    [Fact]
    public async Task Handle_ForAnUnknownUser_ReturnsInvalidCredentialsAfterSpendingTheSameHashingTime()
    {
        var result = await Handler().Handle(new LoginCommand("nobody", Password), CancellationToken.None);

        Assert.Equal(UserErrors.InvalidCredentials, result.Error);
        Assert.Equal(1, _passwordHasher.SimulatedVerifications);
    }

    [Fact]
    public async Task Handle_WithAWrongPassword_CountsTheFailure()
    {
        User user = AddUser();

        var result = await Handler().Handle(new LoginCommand("dilnoza", "wrong"), CancellationToken.None);

        Assert.Equal(UserErrors.InvalidCredentials, result.Error);
        Assert.Equal(1, user.FailedLoginAttempts);
        Assert.Equal(1, _unitOfWork.SaveCount);
        Assert.Empty(_refreshTokens.Tokens);
    }

    [Fact]
    public async Task Handle_AfterFiveWrongPasswords_LocksTheAccountForFifteenMinutes()
    {
        User user = AddUser();

        await FailRepeatedly(MaxFailedAttempts);

        var result = await Handler().Handle(new LoginCommand("dilnoza", Password), CancellationToken.None);

        Assert.Equal(UserErrors.LockedOut, result.Error);
        Assert.Equal(_clock.UtcNow.AddMinutes(15), user.LockedUntil);
    }

    [Fact]
    public async Task Handle_AfterFourWrongPasswords_StillAcceptsTheCorrectPasswordAndResetsTheCounter()
    {
        User user = AddUser();

        await FailRepeatedly(MaxFailedAttempts - 1);

        var result = await Handler().Handle(new LoginCommand("dilnoza", Password), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockedUntil);
    }

    [Fact]
    public async Task Handle_WhenTheLockoutHasPassed_AcceptsTheCorrectPassword()
    {
        AddUser();
        await FailRepeatedly(MaxFailedAttempts);

        _clock.UtcNow = _clock.UtcNow.AddMinutes(15).AddSeconds(1);

        var result = await Handler().Handle(new LoginCommand("dilnoza", Password), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ForADeactivatedAccount_ReturnsInactive()
    {
        AddUser(isActive: false);

        var result = await Handler().Handle(new LoginCommand("dilnoza", Password), CancellationToken.None);

        Assert.Equal(UserErrors.Inactive, result.Error);
    }
}
