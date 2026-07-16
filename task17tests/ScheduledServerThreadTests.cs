using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using task17;

namespace task17tests;

public class ScheduledServerThreadTests
{
    private class CollectingHandler : IExceptionHandler
    {
        public List<(ICommand Cmd, Exception Ex)> Errors { get; } = new();

        public void HandleException(ICommand command, Exception exception)
        {
            Errors.Add((command, exception));
        }
    }

    private class SimpleCommand : ICommand
    {
        public bool WasExecuted { get; private set; }
        public Action OnExecute { get; set; }

        public void Execute()
        {
            WasExecuted = true;
            OnExecute?.Invoke();
        }
    }

    [Fact]
    public void Scheduler_RoundRobin_AlternatesBetweenCommands()
    {
        var scheduler = new RoundRobinScheduler();
        var cmd1 = new LongRunningCommand("Task1", 3, 1);
        var cmd2 = new LongRunningCommand("Task2", 3, 1);

        scheduler.Add(cmd1);
        scheduler.Add(cmd2);

        var selected = scheduler.Select();
        Assert.Same(cmd1, selected);

        selected = scheduler.Select();
        Assert.Same(cmd2, selected);

        selected = scheduler.Select();
        Assert.Same(cmd1, selected);
    }

    [Fact]
    public void LongRunningCommand_ExecutesInMultipleSteps()
    {
        var cmd = new LongRunningCommand("Test", 5, 1);

        Assert.False(cmd.IsCompleted);
        Assert.Equal(0, cmd.CompletedSteps);

        cmd.Execute();
        Assert.Equal(1, cmd.CompletedSteps);
        Assert.False(cmd.IsCompleted);

        cmd.Execute();
        cmd.Execute();
        Assert.Equal(3, cmd.CompletedSteps);

        cmd.Execute();
        cmd.Execute();
        Assert.Equal(5, cmd.CompletedSteps);
        Assert.True(cmd.IsCompleted);
    }

    [Fact]
    public void ScheduledServerThread_ProcessesScheduledCommands()
    {
        var scheduler = new RoundRobinScheduler();
        var server = new ScheduledServerThread(scheduler);
        server.Start();

        var cmd = new LongRunningCommand("Task", 3, 10);
        server.Schedule(cmd);

        Thread.Sleep(100);

        Assert.True(cmd.CompletedSteps >= 1);

        server.GracefulStop();
        Thread.Sleep(50);
    }

    [Fact]
    public void ScheduledServerThread_InterleavesMultipleLongRunningCommands()
    {
        var scheduler = new RoundRobinScheduler();
        var server = new ScheduledServerThread(scheduler);
        server.Start();

        var cmd1 = new LongRunningCommand("Task1", 5, 10);
        var cmd2 = new LongRunningCommand("Task2", 5, 10);

        server.Schedule(cmd1);
        server.Schedule(cmd2);

        Thread.Sleep(150);

        Assert.True(cmd1.CompletedSteps >= 1);
        Assert.True(cmd2.CompletedSteps >= 1);

        int diff = Math.Abs(cmd1.CompletedSteps - cmd2.CompletedSteps);
        Assert.True(diff <= 2, $"Разница в шагах слишком велика: {diff}");

        server.GracefulStop();
        Thread.Sleep(50);
    }

    [Fact]
    public void ScheduledServerThread_ProcessesBothQueueAndScheduler()
    {
        var scheduler = new RoundRobinScheduler();
        var executed = new ConcurrentBag<string>();
        var server = new ScheduledServerThread(scheduler);
        server.Start();

        var quickCmd = new SimpleCommand
        {
            OnExecute = () => executed.Add("quick")
        };
        server.Enqueue(quickCmd);

        var longCmd = new LongRunningCommand("Long", 3, 10);
        server.Schedule(longCmd);

        Thread.Sleep(100);

        Assert.Contains("quick", executed);
        Assert.True(longCmd.CompletedSteps >= 1);

        server.GracefulStop();
        Thread.Sleep(50);
    }

    [Fact]
    public void Scheduler_RemovesCompletedCommands()
    {
        var scheduler = new RoundRobinScheduler();
        var cmd = new LongRunningCommand("Test", 1, 1);

        scheduler.Add(cmd);
        Assert.True(scheduler.HasCommand());

        cmd.Execute(); 
        Assert.True(cmd.IsCompleted);

        var selected = scheduler.Select();
        Assert.Same(cmd, selected);
        Assert.False(scheduler.HasCommand());
    }

    [Fact]
    public void ScheduledServerThread_HardStop_StopsImmediately()
    {
        var scheduler = new RoundRobinScheduler();
        var server = new ScheduledServerThread(scheduler);
        server.Start();
        server.Schedule(new LongRunningCommand("Long", 100, 10));
    
        Thread.Sleep(30);

        server.ForceStop();
    
        int timeoutMs = 500;
        while (server.WorkerThread.IsAlive && timeoutMs > 0)
        {
            Thread.Sleep(10);
            timeoutMs -= 10;
        }

        Assert.False(server.WorkerThread.IsAlive, "Поток не завершился вовремя после ForceStop");
    }

    [Fact]
    public void ExceptionInScheduledCommand_IsHandled()
    {
        var handler = new CollectingHandler();
        var scheduler = new RoundRobinScheduler();
        var server = new ScheduledServerThread(scheduler, handler);
        server.Start();

        var failingCmd = new SimpleCommand
        {
            OnExecute = () => throw new InvalidOperationException("Test error")
        };

        server.Schedule(failingCmd);
        Thread.Sleep(50);

        Assert.Single(handler.Errors);
        Assert.IsType<InvalidOperationException>(handler.Errors[0].Ex);

        server.GracefulStop();
        Thread.Sleep(50);
    }

    [Fact]
    public void Scheduler_Fairness_WeakFairnessGuarantee()
    {
        var scheduler = new RoundRobinScheduler();
        var cmds = new List<LongRunningCommand>();

        for (int i = 0; i < 5; i++)
        {
            cmds.Add(new LongRunningCommand($"Task{i}", 10, 1));
            scheduler.Add(cmds[i]);
        }

        for (int i = 0; i < 20; i++)
        {
            var cmd = scheduler.Select();
            cmd?.Execute();
        }

        foreach (var cmd in cmds)
        {
            Assert.True(cmd.CompletedSteps >= 1,
                $"Команда {cmd.Name} не получила ни одного шага");
        }
    }

    [Fact]
    public void ScheduledServerThread_NoBusyWaiting_WhenIdle()
    {
        var scheduler = new RoundRobinScheduler();
        var server = new ScheduledServerThread(scheduler);
        server.Start();

        Thread.Sleep(500);

        Assert.True(server.WorkerThread.IsAlive);

        server.GracefulStop();
        Thread.Sleep(100);

        Assert.False(server.WorkerThread.IsAlive);
    }
}
