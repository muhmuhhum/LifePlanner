using LifePlanner.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Api.Store;

public class ActivityStore : IActivityStore
{
    private DatabaseContext Context { get; set; }
    
    public ActivityStore(DatabaseContext context)
    {
        Context = context;
    }

    public Task<List<Activity>> GetAll()
    {
        return Context.Activities.ToListAsync();
    }

    public Task<Activity?> GetById(Guid id)
    {
        return Context.Activities.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Activity> CreateAsync(Activity activity)
    {
        Context.Activities.Add(activity);
        await Context.SaveChangesAsync();
        return activity;
    }

    public async Task<Activity> Update(Activity activity)
    {
        Context.Update(activity);
        await Context.SaveChangesAsync();
        return activity;
    }

    public async Task<Activity> Delete(Guid id)
    {
        var activity = Context.Activities.First(x => x.Id == id);
        Context.Activities.Remove(activity);
        await Context.SaveChangesAsync();
        return activity;
    }
}