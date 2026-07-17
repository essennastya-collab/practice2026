using System;
using System.Threading;

namespace task17;
public class LongRunningCommand : ILongRunningCommand
{
    private readonly int _totalSteps;
    private readonly int _stepDurationMs;
    private int _currentStep;
    private readonly string _name;

    public bool IsCompleted { get; private set; }
    public int CompletedSteps => _currentStep;
    public string Name => _name;

    public LongRunningCommand(string name, int totalSteps, int stepDurationMs = 10)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        if (totalSteps <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalSteps));
        if (stepDurationMs < 0)
            throw new ArgumentOutOfRangeException(nameof(stepDurationMs));

        _totalSteps = totalSteps;
        _stepDurationMs = stepDurationMs;
        _currentStep = 0;
        IsCompleted = false;
    }

    public void Execute()
    {
        if (IsCompleted)
            return;
        Thread.Sleep(_stepDurationMs);
        Interlocked.Increment(ref _currentStep);

        if (_currentStep >= _totalSteps)
        {
            IsCompleted = true;
        }
    }
}
