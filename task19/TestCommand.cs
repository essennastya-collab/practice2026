using System;
using System.Threading;
using task17;

namespace task19;

public class TestCommand : ILongRunningCommand
{
    private readonly int _id;
    private int _counter = 0;
    private const int MaxExecutions = 3;
    private readonly Action<string> _log;

    public TestCommand(int id, Action<string> log = null)
    {
        _id = id;
        _log = log ?? Console.WriteLine;
    }

    public bool IsCompleted => _counter >= MaxExecutions;

    public void Execute()
    {
        if (IsCompleted) return;
        
        _counter++;
        _log($"Поток {_id} вызов {_counter}");
        
        Thread.Sleep(10); 
    }
}
