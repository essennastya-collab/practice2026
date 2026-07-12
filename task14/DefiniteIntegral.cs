using System;
using System.Threading;

namespace task14;

public class DefiniteIntegral
{
    public static double Solve(double a, double b, Func<double, double> function, double step, int threadsNumber)
    {
        if (function == null)
            throw new ArgumentNullException(nameof(function));
        if (threadsNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(threadsNumber), "Количество потоков должно быть положительным");
        if (step <= 0)
            throw new ArgumentOutOfRangeException(nameof(step), "Шаг должен быть положительным");
        if (b <= a)
            throw new ArgumentException("Правая граница должна быть больше левой");

        double totalLength = b - a;
        double partLength = totalLength / threadsNumber;

        double[] partialSums = new double[threadsNumber];
        var barrier = new Barrier(threadsNumber + 1);

        for (int i = 0; i < threadsNumber; i++)
        {
            int index = i;
            double localA = a + i * partLength;
            double localB = (i == threadsNumber - 1) ? b : a + (i + 1) * partLength;

            new Thread(() =>
            {
                int n = (int)Math.Round((localB - localA) / step);
                if (n <= 0) n = 1;
                double h = (localB - localA) / n;

                double localSum = (function(localA) + function(localB)) / 2.0;
                for (int k = 1; k < n; k++)
                {
                    localSum += function(localA + k * h);
                }
                localSum *= h;

                partialSums[index] = localSum;
                barrier.SignalAndWait();
            }).Start();
        }

        barrier.SignalAndWait();

        double totalSum = 0;
        for (int i = 0; i < threadsNumber; i++)
        {
            totalSum += partialSums[i];
        }

        return totalSum;
    }

    public static double SolveSingleThreaded(double a, double b, Func<double, double> function, double step)
    {
        if (function == null)
            throw new ArgumentNullException(nameof(function));
        if (step <= 0)
            throw new ArgumentOutOfRangeException(nameof(step), "Шаг должен быть положительным");
        if (b <= a)
            throw new ArgumentException("Правая граница должна быть больше левой");

        int n = (int)Math.Round((b - a) / step);
        if (n <= 0) n = 1;
        double h = (b - a) / n;

        double sum = (function(a) + function(b)) / 2.0;
        for (int k = 1; k < n; k++)
        {
            sum += function(a + k * h);
        }
        sum *= h;

        return sum;
    }
}
