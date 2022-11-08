using LifePlanner.Api.Domain;

namespace LifePlanner.Api.Store;

public interface IUserStore
{
    public Task<User> CreateAsync(User user);
    public Task<User?> GetByIdAsync(long id);
}