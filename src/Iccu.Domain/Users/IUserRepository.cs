namespace Iccu.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<bool> IsUsernameTakenAsync(string username, CancellationToken cancellationToken = default);

    void Insert(User user);
}
