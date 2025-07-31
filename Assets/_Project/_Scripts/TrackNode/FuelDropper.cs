using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class FuelDropper : MonoBehaviour
{
   public GameObject fuelCan;
   public float fuelDroppingFrequency = 10f;
   public float dropHeight = 10f;

   private float _nextTimeToDrop;
   private void Start()
   {
      _nextTimeToDrop = Time.time + fuelDroppingFrequency;
   }
   private void Update()
   {
      if (Time.time > _nextTimeToDrop)
      {
         _nextTimeToDrop = Time.time + fuelDroppingFrequency;
         Vector3 dropPosition = transform.GetChild(Random.Range(0, transform.childCount)).position;
         dropPosition.y += dropHeight;
         Instantiate(fuelCan, dropPosition, Quaternion.identity);
      }
   }
}
