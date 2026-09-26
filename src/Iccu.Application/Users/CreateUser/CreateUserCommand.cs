namespace Iccu.Application.Users.CreateUser;

using Iccu.Domain.Users;
using Iccu.Domain.Common;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Authentication;

public sealed record CreateUserCommand(
    string Username,
    string FullName,
    UserRole Role,
    string Password) : ICommand<Guid>;

internal sealed class CreateUserCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        string username = request.Username.Trim().ToLowerInvariant();

        if (await userRepository.IsUsernameTakenAsync(username, cancellationToken))
        {
            return Result.Failure<Guid>(UserErrors.UsernameTaken);
        }

        var user = User.Create(
            username,
            request.FullName.Trim(),
            request.Role,
            passwordHasher.Hash(request.Password),
            dateTimeProvider.UtcNow);

        userRepository.Insert(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
