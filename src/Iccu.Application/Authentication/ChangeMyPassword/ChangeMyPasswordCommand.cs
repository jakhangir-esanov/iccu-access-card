namespace Iccu.Application.Authentication.ChangeMyPassword;

using Iccu.Domain.Users;
using Iccu.Domain.Common;
using Iccu.Domain.RefreshTokens;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Authentication;

public sealed record ChangeMyPasswordCommand(string CurrentPassword, string NewPassword) : ICommand;

internal sealed class ChangeMyPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ChangeMyPasswordCommand>
{
    public async Task<Result> Handle(ChangeMyPasswordCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetAsync(currentUser.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        if (!passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
        {
            return Result.Failure(UserErrors.WrongCurrentPassword);
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        user.ChangePassword(passwordHasher.Hash(request.NewPassword), utcNow);
        user.ClearLoginFailures();

        foreach (RefreshToken token in await refreshTokenRepository.GetActiveForUserAsync(user.Id, utcNow, cancellationToken))
        {
            token.Revoke(utcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
