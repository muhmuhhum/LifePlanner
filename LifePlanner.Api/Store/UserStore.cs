using LifePlanner.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Api.Store;

public class UserStore : IUserStore
{
    private DatabaseContext _context;


    public UserStore(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
    }
}