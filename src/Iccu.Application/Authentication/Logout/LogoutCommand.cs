namespace Iccu.Application.Authentication.Logout;

using Iccu.Domain.Common;
using Iccu.Domain.RefreshTokens;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Authentication;

public sealed record LogoutCommand(string? RefreshToken) : ICommand;

internal sealed class LogoutCommandHandler(
    IUnitOfWork unitOfWork,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenService tokenService,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Success();
        }

        RefreshToken? refreshToken = await refreshTokenRepository.GetByHashAsync(
            tokenService.HashRefreshToken(request.RefreshToken), cancellationToken);

        if (refreshToken is not { RevokedAt: null })
        {
            return Result.Success();
        }

        refreshToken.Revoke(dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
