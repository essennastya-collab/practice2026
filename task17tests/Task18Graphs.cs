using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ScottPlot;
using Xunit;
using Xunit.Abstractions;
using task17;

namespace task17tests;

public class RoundRobinProgressGraph
{
    private readonly ITestOutputHelper _output;
    public RoundRobinProgressGraph(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Generate_RoundRobin_Progress_Graph()
    {
        _output.WriteLine("\n=== Генерация графика прогресса выполнения команд ===\n");

        var scheduler = new RoundRobinScheduler();
        var server = new ScheduledServerThread(scheduler);
        
        var progressLog = new ConcurrentBag<(double TimeMs, int CommandId, double ProgressPercent)>();
        var startTime = Stopwatch.GetTimestamp();

        int[] stepsPerCommand = { 8, 12, 10 };
        var commands = new List<LongRunningCommand>();

        for (int i = 0; i < 3; i++)
        {
            int cmdIndex = i + 1;
            int maxSteps = stepsPerCommand[i];
            
            var cmd = new LongRunningCommand($"Task{cmdIndex}", maxSteps, stepDurationMs: 5);
            commands.Add(cmd);
            
            server.Schedule(cmd);
        }

        server.Start();

        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < 1000)
        {
            Thread.Sleep(30);
            
            double currentTime = sw.ElapsedMilliseconds;
            
            for (int i = 0; i < commands.Count; i++)
            {
                var cmd = commands[i];
                if (cmd.CompletedSteps > 0)
                {
                    double progress = (cmd.CompletedSteps / (double)10) * 100; 
                    progressLog.Add((currentTime, i + 1, progress));
                }
            }

            if (commands.All(c => c.IsCompleted))
                break;
        }

        server.GracefulStop();
        Thread.Sleep(50);

        _output.WriteLine("Результаты выполнения:");
        for (int i = 0; i < commands.Count; i++)
        {
            _output.WriteLine($"  Команда {i + 1}: {commands[i].CompletedSteps} шагов");
        }

        PlotProgressGraph(progressLog);
        _output.WriteLine("График сохранен: roundrobin_progress_graph.png\n");
    }

    private void PlotProgressGraph(ConcurrentBag<(double TimeMs, int CommandId, double ProgressPercent)> progressLog)
    {
        var plt = new Plot();
        
        var groupedData = progressLog
            .GroupBy(x => x.CommandId)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.TimeMs).ToList());

        var colors = new[] {
            ScottPlot.Color.FromHex("#0bb084"), 
            ScottPlot.Color.FromHex("#bb0000"), 
            ScottPlot.Color.FromHex("#c4199c")  
        };

        foreach (var kvp in groupedData)
        {
            int cmdId = kvp.Key;
            var data = kvp.Value;

            double[] times = data.Select(x => x.TimeMs).ToArray();
            double[] progress = data.Select(x => x.ProgressPercent).ToArray();

            var scatter = plt.Add.Scatter(times, progress);
            scatter.Color = colors[cmdId - 1];
            scatter.LineWidth = 2;
            scatter.MarkerSize = 6;
            scatter.MarkerShape = cmdId == 1 ? ScottPlot.MarkerShape.OpenCircle : 
                                  cmdId == 2 ? ScottPlot.MarkerShape.FilledSquare : 
                                               ScottPlot.MarkerShape.FilledDiamond;
            scatter.LegendText = $"Команда {cmdId}";
        }

        plt.Title("Работа планировщика Round Robin", 16);
        plt.XLabel("Время выполнения (мс)", 14);
        plt.YLabel("Процент выполнения", 14);
        plt.Legend.IsVisible = true;
        plt.Grid.MajorLineColor = ScottPlot.Color.FromHex("#e0e0e0");
        plt.Axes.AutoScale();

        plt.SavePng("roundrobin_progress_graph.png", 800, 600);
    }
}
