using System;
using UnityEngine;
using UnityEngine.UI;

public class CarFuelBar : MonoBehaviour
{
    public CarFuel carFuel;
    public Slider fillBar;

    private void Awake()
    {
        fillBar.maxValue = carFuel.maxFuel;
    }

    private void Update()
    {
        UpdateFill(carFuel.fuel);
    }

    private void UpdateFill(float value)
    {
        fillBar.value = value;
    }
}
