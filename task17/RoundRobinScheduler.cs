using System;
using System.Collections.Generic;

namespace task17;
public class RoundRobinScheduler : IScheduler
{
    private readonly Queue<ICommand> _queue = new();
    private readonly object _lock = new();

    public bool HasCommand()
    {
        lock (_lock)
        {
            return _queue.Count > 0;
        }
    }

    public ICommand Select()
    {
        lock (_lock)
        {
            if (_queue.Count == 0)
                return null;

            var command = _queue.Dequeue();

            if (command is ILongRunningCommand longCmd && !longCmd.IsCompleted)
            {
                _queue.Enqueue(command);
            }

            return command;
        }
    }

    public void Add(ICommand cmd)
    {
        if (cmd == null)
            throw new ArgumentNullException(nameof(cmd));

        lock (_lock)
        {
            _queue.Enqueue(cmd);
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _queue.Count;
            }
        }
    }
}