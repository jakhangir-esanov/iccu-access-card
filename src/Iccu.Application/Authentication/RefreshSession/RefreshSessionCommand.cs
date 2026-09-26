namespace Iccu.Application.Authentication.RefreshSession;

using Iccu.Domain.Users;
using Iccu.Domain.Common;
using Iccu.Domain.RefreshTokens;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Authentication;

public sealed record RefreshSessionCommand(string? RefreshToken) : ICommand<AuthSession>;

internal sealed class RefreshSessionCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenService tokenService,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RefreshSessionCommand, AuthSession>
{
    public async Task<Result<AuthSession>> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Failure<AuthSession>(UserErrors.InvalidRefreshToken);
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        RefreshToken? presented = await refreshTokenRepository.GetByHashAsync(
            tokenService.HashRefreshToken(request.RefreshToken), cancellationToken);

        if (presented is null)
        {
            return Result.Failure<AuthSession>(UserErrors.InvalidRefreshToken);
        }

        if (presented.RevokedAt is not null)
        {
            await RevokeAllSessionsAsync(presented.UserId, utcNow, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<AuthSession>(UserErrors.InvalidRefreshToken);
        }

        User? user = await userRepository.GetAsync(presented.UserId, cancellationToken);

        if (presented.ExpiresAt <= utcNow || user is not { IsActive: true })
        {
            presented.Revoke(utcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<AuthSession>(UserErrors.InvalidRefreshToken);
        }

        IssuedTokens tokens = tokenService.Issue(user, utcNow);
        var next = RefreshToken.Issue(user.Id, tokens.RefreshTokenHash, utcNow, tokens.RefreshTokenExpiresAt);

        refreshTokenRepository.Insert(next);
        presented.Revoke(utcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AuthSession.Create(tokens, user);
    }

    private async Task RevokeAllSessionsAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken)
    {
        foreach (RefreshToken token in await refreshTokenRepository.GetActiveForUserAsync(userId, utcNow, cancellationToken))
        {
            token.Revoke(utcNow);
        }
    }
}
