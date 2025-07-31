using UnityEngine;

public class CarFuel : MonoBehaviour
{
    public float startingFuel = 100;
    public FloatVariable fuel;

    public float fuelDepletionTime = 1f;
    public float fuelDepletionAmount = 0.5f;

    private float _nextTimeToDeplete;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fuel.Value = startingFuel;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time < _nextTimeToDeplete)
        {
            _nextTimeToDeplete = Time.time + fuelDepletionTime;
            fuel.Value -= fuelDepletionAmount;
        }
    }

    public bool IsFuelDepleted()
    {
        return fuel.Value <= 0;
    }
}
