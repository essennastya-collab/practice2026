using System;
using Xunit;
using task14;

namespace task14tests;

public class IntegralTests
{
    private readonly Func<double, double> X = x => x;
    private readonly Func<double, double> SIN = x => Math.Sin(x);

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
    public void IntegralOfX_From0To5_ShouldBe12_5()
    {
        var result = DefiniteIntegral.Solve(0, 5, X, 1e-6, 8);
        Assert.Equal(12.5, result, 5);
    }

    [Fact]
    public void InvalidArguments_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DefiniteIntegral.Solve(0, 1, null!, 1e-5, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DefiniteIntegral.Solve(0, 1, X, 1e-5, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DefiniteIntegral.Solve(0, 1, X, -1, 2));
        Assert.Throws<ArgumentException>(() =>
            DefiniteIntegral.Solve(1, 0, X, 1e-5, 2));
    }
}