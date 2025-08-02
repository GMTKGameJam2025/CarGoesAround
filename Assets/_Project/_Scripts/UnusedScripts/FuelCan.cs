using System;
using UnityEngine;
public class FuelCan : MonoBehaviour
{
    public float fuelAddedAmount;

    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CarFuel carFuel))
        {
            carFuel.AddFuel(fuelAddedAmount);
            Destroy(gameObject);
        }
    }
}