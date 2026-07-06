using Xunit;
using task04;
public class SpaceshipTests
{
    [Fact]
    public void Fighter_ShouldImplementISpaceship()
    {
        ISpaceship fighter = new Fighter();
        Assert.IsAssignableFrom<ISpaceship>(fighter);
    }

    [Fact]
    public void Fighter_ShouldHaveCorrectSpeed()
    {
        ISpaceship fighter = new Fighter();
        Assert.Equal(100, fighter.Speed);
    }

    [Fact]
    public void Fighter_ShouldHaveCorrectFirePower()
    {
        ISpaceship fighter = new Fighter();
        Assert.Equal(50, fighter.FirePower);
    }

    [Fact]
    public void Fighter_ShouldHavePositiveSpeed()
    {
        var fighter = new Fighter();
        Assert.True(fighter.Speed > 0);
    }

    [Fact]
    public void Fighter_ShouldHavePositiveFirePower()
    {
        var fighter = new Fighter();
        Assert.True(fighter.FirePower > 0);
    }

    [Fact]
    public void Fighter_ShouldBeFasterThanCruiser()
    {
        var fighter = new Fighter();
        var cruiser = new Cruiser();
        Assert.True(fighter.Speed > cruiser.Speed);
    }

    [Fact]
    public void Fighter_MoveForward_ShouldNotThrow()
    {
        ISpaceship fighter = new Fighter();
        var exception = Record.Exception(() => fighter.MoveForward());
        Assert.Null(exception);
    }

    [Fact]
    public void Fighter_Rotate_ShouldNotThrow()
    {
        ISpaceship fighter = new Fighter();
        var exception = Record.Exception(() => fighter.Rotate(90));
        Assert.Null(exception);
    }
    [Fact]
    public void Cruiser_ShouldImplementISpaceship()
    {
        ISpaceship cruiser = new Cruiser();
        Assert.IsAssignableFrom<ISpaceship>(cruiser);
    }

    [Fact]
    public void Cruiser_ShouldHaveCorrectSpeed()
    {
        ISpaceship cruiser = new Cruiser();
        Assert.Equal(50, cruiser.Speed);
    }

    [Fact]
    public void Cruiser_ShouldHaveCorrectFirePower()
    {
        ISpaceship cruiser = new Cruiser();
        Assert.Equal(100, cruiser.FirePower);
    }

    [Fact]
    public void Cruiser_ShouldHavePositiveSpeed()
    {
        var cruiser = new Cruiser();
        Assert.True(cruiser.Speed > 0);
    }

    [Fact]
    public void Cruiser_ShouldHavePositiveFirePower()
    {
        var cruiser = new Cruiser();
        Assert.True(cruiser.FirePower > 0);
    }

    [Fact]
    public void Cruiser_ShouldBeMorePowerfulThanFighter()
    {
        var fighter = new Fighter();
        var cruiser = new Cruiser();
        Assert.True(cruiser.FirePower > fighter.FirePower);
    }

    [Fact]
    public void Cruiser_MoveForward_ShouldNotThrow()
    {
        ISpaceship cruiser = new Cruiser();
        var exception = Record.Exception(() => cruiser.MoveForward());
        Assert.Null(exception);
    }

    [Fact]
    public void Cruiser_Rotate_ShouldNotThrow()
    {
        ISpaceship cruiser = new Cruiser();
        var exception = Record.Exception(() => cruiser.Rotate(90));
        Assert.Null(exception);
    }
    [Fact]
    public void Fighter_And_Cruiser_ShouldHaveDifferentTypes()
    {
        var fighter = new Fighter();
        var cruiser = new Cruiser();
        Assert.NotEqual(fighter.GetType(), cruiser.GetType());
    }

    [Fact]
    public void BothShips_ShouldImplementISpaceship()
    {
        var fighter = new Fighter();
        var cruiser = new Cruiser();
        Assert.IsAssignableFrom<ISpaceship>(fighter);
        Assert.IsAssignableFrom<ISpaceship>(cruiser);
    }
}
