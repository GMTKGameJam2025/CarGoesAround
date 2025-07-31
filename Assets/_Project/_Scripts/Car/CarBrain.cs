using UnityEngine;

public class CarBrain : MonoBehaviour
{
    public CarMovement movement;
    public CarFuel fuel;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!fuel.IsFuelDepleted())
        {
            movement.MoveCar();
        }
    }
}
