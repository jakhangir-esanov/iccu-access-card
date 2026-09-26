namespace Iccu.Infrastructure.Users;

using Iccu.Domain.Users;
using Iccu.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

internal sealed class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.SingleOrDefaultAsync(x => x.Username == username, cancellationToken);
    }

    public async Task<bool> IsUsernameTakenAsync(string username, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.AnyAsync(x => x.Username == username, cancellationToken);
    }

    public void Insert(User user)
    {
        dbContext.Users.Add(user);
    }
}
