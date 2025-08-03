using UnityEngine;
using System.Collections;

public class BarrelSpawner : MonoBehaviour, IBuildable
{
    [SerializeField] private GameObject barrelPrefab;
    [SerializeField] private GameObject barrelVisual;
    [SerializeField] private float dropHeight = 2f;
    [SerializeField] private float respawnTime = 5f;

    public void OnBuild()
    {
        StartCoroutine(SpawnBarrels());
    }

    private IEnumerator SpawnBarrels()
    {
        while (true)
        {
            SpawnBarrel();
            yield return new WaitForSeconds(respawnTime);
        }
    }

    private void SpawnBarrel()
    {
        if (barrelVisual != null)
            barrelVisual.SetActive(false);

        Vector3 spawnPosition = transform.position;
        spawnPosition.y += dropHeight;

        Quaternion spawnRotation = transform.rotation;

        GameObject spawnedBarrel = Instantiate(barrelPrefab, spawnPosition, spawnRotation);

        // Set the barrel's roll direction to match spawner's forward direction
        BarrelRoll barrelRoll = spawnedBarrel.GetComponent<BarrelRoll>();
        if (barrelRoll != null)
        {
            barrelRoll.SetRollDirection(transform.forward);
        }

        // Show visual again after spawn
        if (barrelVisual != null)
            barrelVisual.SetActive(true);
    }
}