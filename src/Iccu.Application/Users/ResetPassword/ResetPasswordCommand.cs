namespace Iccu.Application.Users.ResetPassword;

using Iccu.Domain.Users;
using Iccu.Domain.Common;
using Iccu.Domain.RefreshTokens;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Authentication;

public sealed record ResetPasswordCommand(Guid Id, string NewPassword) : ICommand;

internal sealed class ResetPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        user.ChangePassword(passwordHasher.Hash(request.NewPassword));
        user.ClearLoginFailures();

        foreach (RefreshToken token in await refreshTokenRepository.GetActiveForUserAsync(user.Id, utcNow, cancellationToken))
        {
            token.Revoke(utcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
