namespace Iccu.Application.Authentication.Login;

using Iccu.Domain.Users;
using Iccu.Domain.Common;
using Iccu.Domain.RefreshTokens;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Authentication;

public sealed record LoginCommand(string Username, string Password) : ICommand<AuthSession>;

internal sealed class LoginCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<LoginCommand, AuthSession>
{
    private const int MaxFailedAttempts = 5;

    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public async Task<Result<AuthSession>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        DateTime utcNow = dateTimeProvider.UtcNow;

        User? user = await userRepository.GetByUsernameAsync(
            request.Username.Trim().ToLowerInvariant(), cancellationToken);

        if (user is null)
        {
            passwordHasher.SimulateVerification(request.Password);
            return Result.Failure<AuthSession>(UserErrors.InvalidCredentials);
        }

        if (user.LockedUntil > utcNow)
        {
            return Result.Failure<AuthSession>(UserErrors.LockedOut);
        }

        if (!passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            RegisterFailedAttempt(user, utcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<AuthSession>(UserErrors.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return Result.Failure<AuthSession>(UserErrors.Inactive);
        }

        user.ClearLoginFailures();
        user.RecordSuccessfulLogin(utcNow);

        IssuedTokens tokens = tokenService.Issue(user, utcNow);
        refreshTokenRepository.Insert(
            RefreshToken.Issue(user.Id, tokens.RefreshTokenHash, utcNow, tokens.RefreshTokenExpiresAt));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AuthSession.Create(tokens, user);
    }

    private static void RegisterFailedAttempt(User user, DateTime utcNow)
    {
        user.RecordFailedLogin();

        if (user.FailedLoginAttempts >= MaxFailedAttempts)
        {
            user.LockUntil(utcNow.Add(LockoutDuration));
        }
    }
}
