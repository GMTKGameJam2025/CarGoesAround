using UnityEngine;

public class CarSpawner : MonoBehaviour, IBuildable
{
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject carVisual;
    [SerializeField] private float dropHeight;

    public void OnBuild(GridBuildPiece piece)
    {
        carVisual.SetActive(false);
        Vector3 dropPosition = carVisual.transform.position;
        dropPosition.y += dropHeight;
        Quaternion lookRotation = carVisual.transform.rotation;
        Instantiate(carPrefab, dropPosition, lookRotation);

        Destroy(gameObject);
    }
}
