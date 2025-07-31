using UnityEngine;

public class CarFuel : MonoBehaviour
{
    public float maxFuel = 100;
    public float fuel;

    public float fuelDepletionTime = 1f;
    public float fuelDepletionAmount = 0.5f;

    private float _nextTimeToDeplete;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fuel = maxFuel;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > _nextTimeToDeplete)
        {
            _nextTimeToDeplete = Time.time + fuelDepletionTime;
            fuel -= fuelDepletionAmount;
        }
    }

    public bool IsFuelDepleted()
    {
        return fuel <= 0;
    }

    public void AddFuel(float amount)
    {
        float newFuel = fuel + amount;
        if (newFuel > maxFuel)
        {
            newFuel = maxFuel;
        }
        fuel = newFuel;
    }
}
