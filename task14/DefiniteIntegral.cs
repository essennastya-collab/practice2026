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
        double sharedSum = 0.0;

        var barrier = new Barrier(threadsNumber + 1);

        Thread[] threads = new Thread[threadsNumber];

        for (int i = 0; i < threadsNumber; i++)
        {
            double localA = a + i * partLength;
            double localB = (i == threadsNumber - 1) ? b : a + (i + 1) * partLength;

            threads[i] = new Thread(() =>
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

                double initialValue, computedValue;
                do
                {
                    initialValue = sharedSum;
                    computedValue = initialValue + localSum;
                }
                while (Interlocked.CompareExchange(ref sharedSum, computedValue, initialValue) != initialValue);

                barrier.SignalAndWait();
            });

            threads[i].Start();
        }

        barrier.SignalAndWait();

        return sharedSum;
    }
}