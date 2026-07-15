using System;
using System.Collections.Concurrent;
using System.Threading;

namespace task17;

public class ServerThread
{
    private readonly BlockingCollection<ICommand> _commands = new();
    private readonly IExceptionHandler _handler;
    private readonly Thread _worker;
    
    private volatile bool _hardStopFlag;
    private volatile bool _softStopFlag;

    public Thread WorkerThread => _worker;

    public ServerThread(IExceptionHandler handler = null)
    {
        _handler = handler ?? new ConsoleExceptionHandler();
        _worker = new Thread(ProcessCommands)
        {
            IsBackground = true,
            Name = $"ServerThread-{Guid.NewGuid():N}"
        };
    }

    public void Start()
    {
        if (_worker.IsAlive)
            throw new InvalidOperationException("Thread is already running");
        
        _hardStopFlag = false;
        _softStopFlag = false;
        _worker.Start();
    }

    public void AddCommand(ICommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));
        
        if (!_hardStopFlag)
            _commands.TryAdd(command);
    }

    internal void ForceStop()
    {
        _hardStopFlag = true;
        
        while (_commands.TryTake(out _)) { }
        
        _commands.CompleteAdding();
    }

    internal void GracefulStop()
    {
        _softStopFlag = true;
    }

    private void ProcessCommands()
    {
        while (!_hardStopFlag)
        {
            if (_softStopFlag && _commands.Count == 0)
                break;

            if (_commands.TryTake(out ICommand cmd, 50))
            {
                try
                {
                    cmd.Execute();
                }
                catch (Exception ex)
                {
                    _handler.HandleException(cmd, ex);
                }
            }
        }
    }
}
