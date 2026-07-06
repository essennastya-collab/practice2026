namespace task04;

public class Fighter : ISpaceship
{
    private int x = 0;
    private int y = 0;
    private int a = 0;

    public int Speed => 100;  
    public int FirePower => 50;  

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
