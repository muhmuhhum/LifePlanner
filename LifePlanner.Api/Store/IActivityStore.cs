using LifePlanner.Api.Domain;

namespace LifePlanner.Api.Store;

public interface IActivityStore
{
    public Task<List<Activity>> GetAll();
    public Task<List<Activity>> GetWithoutExecutedToday();
    public Task<Activity?> GetById(Guid id);
    public Task<Activity> CreateAsync(Activity activity);
    public Task<Activity> Update(Activity activity);
    public Task<Activity> Delete(Guid id);
}