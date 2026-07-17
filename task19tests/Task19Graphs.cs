using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ScottPlot;
using Xunit;
using Xunit.Abstractions;
using task17;
using task19;

namespace task19tests;

public class Task19Graphs
{
    private readonly ITestOutputHelper _output;
    public Task19Graphs(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Generate_TestCommand_Progress_Graph()
    {
        _output.WriteLine("\n=== Генерация графика выполнения 5 TestCommand (Задание 19) ===\n");

        var scheduler = new RoundRobinScheduler();
        var server = new ScheduledServerThread(scheduler);
        
        var executionLog = new ConcurrentBag<(double TimeMs, int CommandId, int CallNumber)>();
        var startTime = Stopwatch.GetTimestamp();

        for (int i = 1; i <= 5; i++)
        {
            int cmdId = i;
            server.Schedule(new TestCommand(cmdId, msg => 
            {
                var elapsedMs = (Stopwatch.GetTimestamp() - startTime) / (double)Stopwatch.Frequency * 1000.0;
                var parts = msg.Split(' ');
                int id = int.Parse(parts[1]);
                int call = int.Parse(parts[3]);
                executionLog.Add((elapsedMs, id, call));
            }));
        }

        server.Start();
        Thread.Sleep(250); 
        server.GracefulStop();
        Thread.Sleep(50);

        var totalCalls = executionLog.Count;
        _output.WriteLine($"Всего выполнено вызовов: {totalCalls} (ожидалось 15)");
        
        var grouped = executionLog.GroupBy(x => x.CommandId);
        foreach (var g in grouped.OrderBy(x => x.Key))
        {
            _output.WriteLine($"  Команда {g.Key}: {g.Count()} вызовов");
        }

        PlotTestCommandProgress(executionLog.ToList());
        _output.WriteLine("График сохранен: task19_testcommand_progress.png\n");

        Assert.Equal(15, totalCalls);
        Assert.All(grouped, g => Assert.Equal(3, g.Count()));
    }

    private void PlotTestCommandProgress(List<(double TimeMs, int CommandId, int CallNumber)> executionLog)
    {
        var plt = new Plot();
        
        var groupedData = executionLog
            .GroupBy(x => x.CommandId)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.TimeMs).ToList());

        var colors = new[] {
            ScottPlot.Color.FromHex("#0bb084"), 
            ScottPlot.Color.FromHex("#bb0000"), 
            ScottPlot.Color.FromHex("#c4199c"), 
            ScottPlot.Color.FromHex("#0066cc"), 
            ScottPlot.Color.FromHex("#ff9900") 
        };

        foreach (var kvp in groupedData)
        {
            int cmdId = kvp.Key;
            var data = kvp.Value;

            double[] times = data.Select(x => x.TimeMs).ToArray();
            double[] calls = data.Select(x => (double)x.CallNumber).ToArray();

            var scatter = plt.Add.Scatter(times, calls);
            scatter.Color = colors[cmdId - 1];
            scatter.LineWidth = 2;
            scatter.MarkerSize = 8;
            scatter.LegendText = $"Command {cmdId}";
        }

        plt.Title("TestCommand Execution (Task 19)\n5 commands × 3 calls each (Round Robin)");
        plt.XLabel("Time (ms)");
        plt.YLabel("Execute() Call Number");
        plt.Legend.IsVisible = true;
        plt.Grid.MajorLineColor = ScottPlot.Color.FromHex("#e0e0e0");

        plt.SavePng("task19_testcommand_progress.png", 900, 600);
    }
}