namespace task04;

public class Cruiser : ISpaceship
{
    private int x = 0;
    private int y = 0;
    private int a = 0;

    public int Speed => 50;      
    public int FirePower => 100;  

    public void MoveForward()
    { 
        x += Speed; y += Speed;
    }
    public void Rotate(int angle) 
    {
         a = (a + angle) % 360; 
    }
    public void Fire() { }
}
