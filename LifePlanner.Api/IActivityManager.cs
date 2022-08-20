using LifePlanner.Api.Domain;

namespace LifePlanner.Api;

public interface IActivityManager
{
    public Task<List<Activity>> GetAll();
    public Task<Activity?> GetById(Guid id);
    public Task<Activity> CreateAsync(Activity activity);
    public Task<Activity> Update(Activity activity);
    public Task<Activity> Delete(Guid id);
}