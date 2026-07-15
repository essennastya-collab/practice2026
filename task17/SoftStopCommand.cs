using System;
using System.Threading;

namespace task17;

public class SoftStopCommand : ICommand
{
    private readonly ServerThread _server;

    public SoftStopCommand(ServerThread server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
    }

    public void Execute()
    {
        if (!Thread.CurrentThread.Equals(_server.WorkerThread))
        {
            throw new InvalidOperationException(
                $"SoftStopCommand must be executed by the target thread, " +
                $"but was called from {Thread.CurrentThread.Name ?? "unnamed thread"}");
        }
        
        _server.GracefulStop();
    }
}
