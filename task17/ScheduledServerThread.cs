using System;
using System.Collections.Concurrent;
using System.Threading;

namespace task17;

public class ScheduledServerThread
{
    private readonly BlockingCollection<ICommand> _queue = new();
    private readonly IScheduler _scheduler;
    private readonly IExceptionHandler _handler;
    private readonly Thread _worker;

    private volatile bool _hardStopFlag;
    private volatile bool _softStopFlag;

    public Thread WorkerThread => _worker;
    public IScheduler Scheduler => _scheduler;

    public ScheduledServerThread(IScheduler scheduler, IExceptionHandler handler = null)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _handler = handler ?? new ConsoleExceptionHandler();
        _worker = new Thread(WorkerLoop)
        {
            IsBackground = true,
            Name = $"ScheduledServerThread-{Guid.NewGuid():N}"
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

    public void Enqueue(ICommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        if (!_hardStopFlag)
            _queue.TryAdd(command);
    }

    public void Schedule(ICommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        if (!_hardStopFlag)
            _scheduler.Add(command);
    }

    internal void ForceStop()
    {
        _hardStopFlag = true;
        while (_queue.TryTake(out _)) { }
        _queue.CompleteAdding();
    }

    internal void GracefulStop()
    {
        _softStopFlag = true;
    }

    private void WorkerLoop()
    {
        while (!_hardStopFlag)
        {
            ICommand command = null;

            if (_scheduler.HasCommand())
            {
                command = _scheduler.Select();
            }
            else if (!_queue.TryTake(out command, 50))
            {
                if (_scheduler.HasCommand())
                {
                    command = _scheduler.Select();
                }
                else
                {
                    if (_softStopFlag && !_scheduler.HasCommand() && _queue.Count == 0)
                        break;

                    continue;
                }
            }

            if (command != null)
            {
                try
                {
                    command.Execute();
                }
                catch (Exception ex)
                {
                    _handler.HandleException(command, ex);
                }
            }
        }
    }
}
