namespace LifePlanner.Api.Telegram.Commands;

public interface ICommandState
{
    public Task<bool> Execute(string message);
}