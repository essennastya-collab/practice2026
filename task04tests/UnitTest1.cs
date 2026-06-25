using Xunit;
using task04;
public class SpaceshipTests
{
    [Fact]
    public void Cruiser_ShouldHaveCorrectStats()
    {
        ISpaceship cruiser = new Cruiser();
        Assert.Equal(50, cruiser.Speed);
        Assert.Equal(100, cruiser.FirePower);
    }

    [Fact]
    public void Fighter_ShouldBeFasterThanCruiser()
    {
        var fighter = new Fighter();
        var cruiser = new Cruiser();
        Assert.True(fighter.Speed > cruiser.Speed);
    }
    [Fact]
    public void Fighter_ShouldHaveCorrectStats()
    {
        ISpaceship fighter = new Fighter();
        Assert.Equal(100, fighter.Speed);
        Assert.Equal(50, fighter.FirePower);
    }
        [Fact]
    public void Cruiser_ShouldBePowerfulThanFighter()
    {
        var fighter = new Fighter();
        var cruiser = new Cruiser();
        Assert.True(cruiser.FirePower > fighter.FirePower);
    }
        [Fact]
    public void Ships_ShouldImplementISpaceship()
    {
        var fighter = new Fighter();
        var cruiser = new Cruiser();
        Assert.IsAssignableFrom<ISpaceship>(fighter);
        Assert.IsAssignableFrom<ISpaceship>(cruiser);
    }
    [Fact]
    public void Fighter_And_Cruiser_DifferentTypes()
    {
        var fighter = new Fighter();
        var cruiser = new Cruiser();
        Assert.NotEqual(fighter.GetType(), cruiser.GetType());
    }
    [Fact]
    public void BothShips_ShouldHavePositiveSpeed()
    {
        var fighter = new Fighter();
        var cruiser = new Cruiser();
        Assert.True(fighter.Speed > 0);
        Assert.True(cruiser.Speed > 0);
    }
    [Fact]
    public void Rotate_ShouldWorkCorrectly()
    {
        ISpaceship ship = new Fighter();
        var ex = Record.Exception(() => ship.Rotate(90));
        Assert.Null(ex);
    }
    [Fact]
    public void MoveForward_ShouldWorkCorrectly()
    {
        ISpaceship fighter = new Fighter();
        ISpaceship cruiser = new Cruiser();
        var ex1 = Record.Exception(() => fighter.MoveForward());
        var ex2 = Record.Exception(() => cruiser.MoveForward());
        Assert.Null(ex1);
        Assert.Null(ex2);
    }
}
