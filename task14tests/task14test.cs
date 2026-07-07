using System;
using Xunit;
using task14;

namespace task14tests;

public class IntegralTests
{
    private readonly Func<double, double> X = x => x;
    private readonly Func<double, double> SIN = x => Math.Sin(x);
    private readonly Func<double, double> CONST = x => 1.0;
    private readonly Func<double, double> XSQ = x => x * x;

    [Fact]
    public void IntegralOfX_FromMinus1To1_ShouldBeZero()
    {
        var result = DefiniteIntegral.Solve(-1, 1, X, 1e-4, 2);
        Assert.Equal(0, result, 4);
    }

    [Fact]
    public void IntegralOfSin_FromMinus1To1_ShouldBeZero()
    {
        var result = DefiniteIntegral.Solve(-1, 1, SIN, 1e-5, 8);
        Assert.Equal(0, result, 4);
    }

    [Fact]
    public void IntegralOfConst_ShouldReturnLength()
    {
        var result = DefiniteIntegral.Solve(0, 10, CONST, 1e-5, 4);
        Assert.Equal(10.0, result, 4);
    }

    [Fact]
    public void SingleThread_ShouldWork()
    {
        var result = DefiniteIntegral.Solve(0, 1, X, 1e-5, 1);
        Assert.Equal(0.5, result, 4);
    }

    [Fact]
    public void ManyThreads_ShouldWork()
    {
        var result = DefiniteIntegral.Solve(0, 1, X, 1e-5, 16);
        Assert.Equal(0.5, result, 4);
    }

    [Fact]
    public void InvalidArguments_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DefiniteIntegral.Solve(0, 1, null, 1e-5, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DefiniteIntegral.Solve(0, 1, X, 1e-5, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DefiniteIntegral.Solve(0, 1, X, -1, 2));
        Assert.Throws<ArgumentException>(() =>
            DefiniteIntegral.Solve(1, 0, X, 1e-5, 2));
    }

    [Fact]
    public void ResultShouldBeConsistentAcrossRuns()
    {
        double result1 = DefiniteIntegral.Solve(0, 5, X, 1e-5, 4);
        double result2 = DefiniteIntegral.Solve(0, 5, X, 1e-5, 4);
        Assert.Equal(result1, result2, 10);
    }
}