using System;

namespace task17;

public interface IExceptionHandler
{
    void HandleException(ICommand command, Exception exception);
}

public class ConsoleExceptionHandler : IExceptionHandler
{
    public void HandleException(ICommand command, Exception exception)
    {
        Console.Error.WriteLine($"[ERROR] Command {command.GetType().Name} failed: {exception.Message}");
    }
}
