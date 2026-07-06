namespace task11tests;

using Task11;

public class CalculatorTests
{
    [Fact]
    public void BasicOperations_ShouldReturnCorrectResults()
    {
        var calculator = DynamicCompiler.Build();

        Assert.NotNull(calculator);
        Assert.Equal(4, calculator.Add(2, 2));
        Assert.Equal(1, calculator.Minus(10, 9));
        Assert.Equal(21, calculator.Mul(7, 3));
        Assert.Equal(4, calculator.Div(8, 2));
    }

    [Fact]
    public void OperationsWithNegativeNumbers_ShouldWorkCorrectly()
    {
        var calculator = DynamicCompiler.Build();
        
        Assert.Equal(-12, calculator.Add(-9, -3));
        Assert.Equal(2, calculator.Minus(-7, -9));
        Assert.Equal(-36, calculator.Mul(-12, 3));
        Assert.Equal(1, calculator.Div(-6, -6));
    }

    [Fact]
    public void DivisionByZero_ShouldThrowException()
    {
        var calculator = DynamicCompiler.Build();

        Assert.Throws<DivideByZeroException>(() => calculator.Div(10, 0));
    }

    [Fact]
    public void Calculator_ShouldImplementInterface()
    {
        var calculator = DynamicCompiler.Build();
        
        Assert.IsAssignableFrom<ICalculator>(calculator);
    }
}
