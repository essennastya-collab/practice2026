using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using task14;
using ScottPlot;

namespace task14tests;

public class Measurements   
{
    private const int WarmupIterations = 3;
    private const int MeasureIterations = 10;
    private static readonly Func<double, double> SinFunc = Math.Sin;
    private const double A = -100.0;
    private const double B = 100.0;
    private const double TargetAccuracy = 1e-4;

    public static void Run()
    {
        Console.WriteLine("Definite integral of sin(x) on [-100, 100]\n");

        Console.WriteLine("Step 3: Finding optimal step size");
        double optimalStep = FindOptimalStep();
        Console.WriteLine($"Optimal step: {optimalStep}\n");

        Console.WriteLine("Step 4: Finding optimal thread count");
        var (optimalThreads, threadResults) = FindOptimalThreadCount(optimalStep);
        Console.WriteLine($"Optimal thread count: {optimalThreads}\n");

        Console.WriteLine("Step 5: Building performance chart");
        PlotResults(threadResults, optimalStep);
        Console.WriteLine("Chart saved to threads_performance.png\n");

        Console.WriteLine("Step 6: Comparison with single-threaded version");
        var (multiTime, singleTime, speedup) = CompareWithSingleThread(optimalStep, optimalThreads);
        Console.WriteLine($"Multithreaded version: {multiTime:F2} ms");
        Console.WriteLine($"Single-threaded version: {singleTime:F2} ms");
        Console.WriteLine($"Speedup: {speedup:F2}x ({(speedup - 1) * 100:F1}%)\n");

        Console.WriteLine("Step 8: Saving results to file");
        SaveResults(optimalStep, optimalThreads, multiTime, singleTime, speedup);
        Console.WriteLine("Results saved to results.txt\n");
    }

    private static double FindOptimalStep()
    {
        double[] steps = { 1e-1, 1e-2, 1e-3, 1e-4, 1e-5, 1e-6 };
        double exactValue = 0.0; 

        Console.WriteLine($"{"Step",-10} {"Value",-20} {"Error",-15} {"Time (ms)",-15} {"Accuracy",-10}");
        Console.WriteLine(new string('-', 70));

        double optimalStep = steps[0];
        double bestTime = double.MaxValue;

        foreach (var step in steps)
        {
            for (int i = 0; i < WarmupIterations; i++)
            {
                DefiniteIntegral.SolveSingleThreaded(A, B, SinFunc, step);
            }

            var sw = Stopwatch.StartNew();
            double result = 0;
            for (int i = 0; i < MeasureIterations; i++)
            {
                result = DefiniteIntegral.SolveSingleThreaded(A, B, SinFunc, step);
            }
            sw.Stop();

            double avgTime = sw.Elapsed.TotalMilliseconds / MeasureIterations;
            double error = Math.Abs(result - exactValue);
            bool isAccurate = error <= TargetAccuracy;

            Console.WriteLine($"{step,-10:E1} {result,-20:F10} {error,-15:E4} {avgTime,-15:F2} {(isAccurate ? "OK" : "FAIL")}");

            if (isAccurate && avgTime < bestTime)
            {
                bestTime = avgTime;
                optimalStep = step;
            }
        }

        return optimalStep;
    }

    private static (int, List<(int Threads, double Time)>) FindOptimalThreadCount(double step)
    {
        int[] threadCounts = { 1, 2, 4, 8, 16, 32 };
        var results = new List<(int Threads, double Time)>();

        Console.WriteLine($"{"Threads",-10} {"Time (ms)",-15}");
        Console.WriteLine(new string('-', 25));

        int optimalThreads = 1;
        double bestTime = double.MaxValue;

        foreach (var threads in threadCounts)
        {
            for (int i = 0; i < WarmupIterations; i++)
            {
                DefiniteIntegral.Solve(A, B, SinFunc, step, threads);
            }

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < MeasureIterations; i++)
            {
                DefiniteIntegral.Solve(A, B, SinFunc, step, threads);
            }
            sw.Stop();

            double avgTime = sw.Elapsed.TotalMilliseconds / MeasureIterations;
            results.Add((threads, avgTime));

            Console.WriteLine($"{threads,-10} {avgTime,-15:F2}");

            if (avgTime < bestTime)
            {
                bestTime = avgTime;
                optimalThreads = threads;
            }
        }

        return (optimalThreads, results);
    }

    private static void PlotResults(List<(int Threads, double Time)> results, double step)
    {
        var plt = new ScottPlot.Plot(800, 600);

        double[] threads = results.Select(r => (double)r.Threads).ToArray();
        double[] times = results.Select(r => r.Time).ToArray();

        plt.AddScatterPoints(threads, times, color: System.Drawing.Color.Blue, markerSize: 10);
        plt.AddScatterLines(threads, times, color: System.Drawing.Color.Blue);

        plt.Title($"Execution Time vs Thread Count\n(step = {step:E1}, sin(x) on [-100, 100])");
        plt.XLabel("Number of Threads");
        plt.YLabel("Execution Time (ms)");

        plt.SaveFig("threads_performance.png");
    }

    private static (double MultiTime, double SingleTime, double Speedup) CompareWithSingleThread(double step, int optimalThreads)
    {
        for (int i = 0; i < WarmupIterations; i++)
        {
            DefiniteIntegral.Solve(A, B, SinFunc, step, optimalThreads);
            DefiniteIntegral.SolveSingleThreaded(A, B, SinFunc, step);
        }

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < MeasureIterations; i++)
        {
            DefiniteIntegral.Solve(A, B, SinFunc, step, optimalThreads);
        }
        sw.Stop();
        double multiTime = sw.Elapsed.TotalMilliseconds / MeasureIterations;

        sw.Restart();
        for (int i = 0; i < MeasureIterations; i++)
        {
            DefiniteIntegral.SolveSingleThreaded(A, B, SinFunc, step);
        }
        sw.Stop();
        double singleTime = sw.Elapsed.TotalMilliseconds / MeasureIterations;

        double speedup = singleTime / multiTime;

        return (multiTime, singleTime, speedup);
    }

    private static void SaveResults(double step, int threads, double multiTime, double singleTime, double speedup)
    {
        var lines = new List<string>
        {
            "Definite integral of sin(x) on [-100, 100]",
            "",
            $"1. Optimal step size: {step:E1}",
            $"   Explanation: the integration step that provides 1e-4 accuracy",
            $"   with minimal execution time.",
            "",
            $"2. Optimal thread count: {threads}",
            $"   Explanation: the number of threads that achieves minimal",
            $"   execution time of the Solve function for the chosen step.",
            "",
            $"3. Multithreaded version execution time: {multiTime:F2} ms",
            $"   Explanation: average time over {MeasureIterations} runs",
            $"   of the Solve function with {threads} threads.",
            "",
            $"4. Single-threaded version execution time: {singleTime:F2} ms",
            $"   Explanation: average time over {MeasureIterations} runs",
            $"   of the SolveSingleThreaded function (without using threads).",
            "",
            $"5. Multithreaded version speedup: {speedup:F2}x",
            $"   Explanation: the multithreaded version is {speedup:F2} times faster",
            $"   than the single-threaded version. Performance difference: {(speedup - 1) * 100:F1}%",
            "",
            speedup >= 1.15 
                ? "Criterion met: multithreaded version is 15%+ faster." 
                : "Criterion not met: additional optimization is required."
        };

        File.WriteAllLines("results.txt", lines);
    }
}