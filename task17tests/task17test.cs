using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using task17;

namespace task17tests;

public class ServerThreadTests
{
    private class TrackingCommand : ICommand
    {
        public bool WasExecuted { get; private set; }
        public Action Action { get; set; }

        public void Execute()
        {
            WasExecuted = true;
            Action?.Invoke();
        }
    }

    private class CollectingHandler : IExceptionHandler
    {
        public List<(ICommand Cmd, Exception Ex)> Errors { get; } = new();

        public void HandleException(ICommand command, Exception exception)
        {
            Errors.Add((command, exception));
        }
    }

    [Fact]
    public void HardStop_FromExternalThread_ThrowsException()
    {
        var server = new ServerThread();
        var cmd = new HardStopCommand(server);

        var ex = Assert.Throws<InvalidOperationException>(() => cmd.Execute());
        Assert.Contains("must be executed by the target thread", ex.Message);
    }

    [Fact]
    public void SoftStop_FromExternalThread_ThrowsException()
    {
        var server = new ServerThread();
        var cmd = new SoftStopCommand(server);

        var ex = Assert.Throws<InvalidOperationException>(() => cmd.Execute());
        Assert.Contains("must be executed by the target thread", ex.Message);
    }

    [Fact]
    public void HardStop_DiscardsPendingCommands()
    {
        var executed = new ConcurrentBag<string>();
        var server = new ServerThread();
        server.Start();

        server.AddCommand(new TrackingCommand { Action = () => executed.Add("first") });
        server.AddCommand(new HardStopCommand(server));
        server.AddCommand(new TrackingCommand { Action = () => executed.Add("should_not_run") });

        Thread.Sleep(300);

        Assert.Contains("first", executed);
        Assert.DoesNotContain("should_not_run", executed);
        Assert.False(server.WorkerThread.IsAlive);
    }

    [Fact]
    public void SoftStop_WaitsForQueueToEmpty()
    {
        var executed = new ConcurrentBag<string>();
        var server = new ServerThread();
        server.Start();

        server.AddCommand(new TrackingCommand { Action = () => executed.Add("cmd1") });
        server.AddCommand(new SoftStopCommand(server));
        server.AddCommand(new TrackingCommand { Action = () => executed.Add("cmd2") });

        Thread.Sleep(400);

        Assert.Contains("cmd1", executed);
        Assert.Contains("cmd2", executed);
        Assert.False(server.WorkerThread.IsAlive);
    }

    [Fact]
    public void Exception_HandledByExceptionHandler()
    {
        var handler = new CollectingHandler();
        var server = new ServerThread(handler);
        server.Start();

        var failingCmd = new TrackingCommand
        {
            Action = () => throw new InvalidOperationException("Test failure")
        };

        server.AddCommand(failingCmd);
        server.AddCommand(new SoftStopCommand(server));

        Thread.Sleep(300);

        Assert.Single(handler.Errors);
        Assert.Same(failingCmd, handler.Errors[0].Cmd);
        Assert.IsType<InvalidOperationException>(handler.Errors[0].Ex);
        Assert.Equal("Test failure", handler.Errors[0].Ex.Message);
    }

    [Fact]
    public void IdleThread_LowCpuUsage()
    {
        var server = new ServerThread();
        server.Start();

        Thread.Sleep(500);

        Assert.True(server.WorkerThread.IsAlive);

        server.AddCommand(new SoftStopCommand(server));
        Thread.Sleep(100);
        
        Assert.False(server.WorkerThread.IsAlive);
    }

    [Fact]
    public void MultipleCommands_ProcessedInOrder()
    {
        var order = new ConcurrentBag<int>();
        var server = new ServerThread();
        server.Start();

        for (int i = 0; i < 5; i++)
        {
            int index = i;
            server.AddCommand(new TrackingCommand { Action = () => order.Add(index) });
        }

        server.AddCommand(new SoftStopCommand(server));
        Thread.Sleep(300);

        Assert.Equal(5, order.Count);
        Assert.False(server.WorkerThread.IsAlive);
    }
}
