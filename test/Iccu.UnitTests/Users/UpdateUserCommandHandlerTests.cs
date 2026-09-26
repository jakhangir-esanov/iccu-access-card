namespace Iccu.UnitTests.Users;

using Iccu.Domain.Users;
using Iccu.UnitTests.Fakes;
using Iccu.Domain.Common.Enums;
using Iccu.Domain.RefreshTokens;
using Iccu.Application.Users.UpdateUser;

public class UpdateUserCommandHandlerTests
{
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeUserRepository _users = new();
    private readonly FakeRefreshTokenRepository _refreshTokens = new();
    private readonly FakeClock _clock = new();

    private UpdateUserCommandHandler Handler(Guid currentUserId) => new(
        _unitOfWork,
        new FakeCurrentUser(currentUserId),
        _users,
        _refreshTokens,
        _clock);

    private User Add(UserRole role)
    {
        var user = User.Create($"user{_users.Users.Count}", "Xodim", role, "hash", _clock.UtcNow);
        _users.Insert(user);
        return user;
    }

    [Theory]
    [InlineData(UserRole.Receptionist, true)]
    [InlineData(UserRole.Admin, false)]
    public async Task Handle_WhenAnAdminDemotesOrDeactivatesThemselves_ReturnsCannotDemoteSelf(UserRole role, bool isActive)
    {
        User admin = Add(UserRole.Admin);

        var result = await Handler(admin.Id).Handle(
            new UpdateUserCommand(admin.Id, admin.FullName, role, isActive), CancellationToken.None);

        Assert.Equal(UserErrors.CannotDemoteSelf, result.Error);
        Assert.Equal(UserRole.Admin, admin.Role);
    }

    [Fact]
    public async Task Handle_WhenDeactivatingAnotherUser_RevokesTheirSessions()
    {
        User admin = Add(UserRole.Admin);
        User receptionist = Add(UserRole.Receptionist);
        _refreshTokens.Insert(RefreshToken.Issue(receptionist.Id, "hash", _clock.UtcNow, _clock.UtcNow.AddDays(7)));

        var result = await Handler(admin.Id).Handle(
            new UpdateUserCommand(receptionist.Id, "Xodim", UserRole.Receptionist, IsActive: false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(receptionist.IsActive);
        Assert.NotNull(Assert.Single(_refreshTokens.Tokens).RevokedAt);
    }
}
