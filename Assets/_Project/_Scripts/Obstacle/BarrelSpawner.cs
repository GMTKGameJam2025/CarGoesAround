using UnityEngine;
using System.Collections;

public class BarrelSpawner : MonoBehaviour, IBuildable
{
    [SerializeField] private GameObject barrelPrefab;
    [SerializeField] private GameObject barrelVisual;
    [SerializeField] private float dropHeight = 2f;
    [SerializeField] private float respawnTime = 5f;
    [SerializeField] private float initialDelay = 2f;

    private bool isSpawning = false;
    private Coroutine spawnCoroutine;

    public void OnBuild(GridBuildPiece piece)
    {
        if (!isSpawning)
        {
            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnBarrels());
        }
    }

    private IEnumerator SpawnBarrels()
    {
        yield return new WaitForSeconds(initialDelay);

        while (isSpawning)
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

        BarrelRoll barrelRoll = spawnedBarrel.GetComponent<BarrelRoll>();
        if (barrelRoll != null)
        {
            barrelRoll.SetRollDirection(transform.forward);
        }

        if (barrelVisual != null)
            barrelVisual.SetActive(true);
    }

    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    void OnGUI()
    {
        if (Application.isEditor)
        {
            GUI.Label(new Rect(10, 10 + (transform.GetInstanceID() % 3) * 20, 200, 20),
                     $"Spawner {gameObject.name}: {(isSpawning ? "Active" : "Inactive")}");
        }
    }
}