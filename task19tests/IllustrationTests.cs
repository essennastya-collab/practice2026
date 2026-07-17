using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using task17;
using task19;

namespace task19tests;

public class IllustrationTests
{
    private readonly ITestOutputHelper _output;

    public IllustrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Illustrate_PseudoParallelExecution_With_HardStop()
    {
        var scheduler = new RoundRobinScheduler();
        var server = new ScheduledServerThread(scheduler);
        
        var logs = new List<string>();
        void Log(string msg) 
        {
            lock (logs) { logs.Add(msg); }
            _output.WriteLine(msg); 
        }

        for (int i = 1; i <= 5; i++)
        {
            server.Schedule(new TestCommand(i, Log));
        }

        server.Start();

        Thread.Sleep(80);

        server.ForceStop();
        
        Thread.Sleep(50);

        Assert.False(server.WorkerThread.IsAlive, "Поток должен быть остановлен");
        
        Assert.NotEmpty(logs);

        Assert.True(logs.Count < 15, $"Ожидалось прерывание HardStop, но выполнено вызовов: {logs.Count}");
        
        _output.WriteLine($"\n--- ИТОГ: Всего выполнено вызовов до HardStop: {logs.Count} из 15 ---");
    }
}
