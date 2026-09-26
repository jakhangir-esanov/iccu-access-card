namespace Iccu.Application.Users.UpdateUser;

using Iccu.Domain.Users;
using Iccu.Domain.Common;
using Iccu.Domain.Common.Enums;
using Iccu.Domain.RefreshTokens;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Authentication;

public sealed record UpdateUserCommand(
    Guid Id,
    string FullName,
    UserRole Role,
    bool IsActive) : ICommand;

internal sealed class UpdateUserCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        bool demotesSelf = user.Id == currentUser.UserId && (!request.IsActive || request.Role != UserRole.Admin);
        if (demotesSelf)
        {
            return Result.Failure(UserErrors.CannotDemoteSelf);
        }

        user.Update(request.FullName.Trim(), request.Role, request.IsActive);

        if (!request.IsActive)
        {
            DateTime utcNow = dateTimeProvider.UtcNow;

            foreach (RefreshToken token in await refreshTokenRepository.GetActiveForUserAsync(user.Id, utcNow, cancellationToken))
            {
                token.Revoke(utcNow);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
