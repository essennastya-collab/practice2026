namespace task04;
public interface ISpaceship
{
    void MoveForward();      
    void Rotate(int angle);  
    void Fire();             
    int Speed { get; }      
    int FirePower { get; }   
}