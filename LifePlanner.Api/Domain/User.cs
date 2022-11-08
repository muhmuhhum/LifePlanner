namespace LifePlanner.Api.Domain;

public class User
{
    public long Id { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public List<Activity> Activities { get; set; }
}