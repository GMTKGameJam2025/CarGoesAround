using UnityEngine;

public class CarSpawner : MonoBehaviour, IBuildable
{
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject carVisual;
    [SerializeField] private float dropHeight;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnBuild()
    {
        carVisual.SetActive(false);
        Vector3 dropPosition = carVisual.transform.position;
        dropPosition.y += dropHeight;
        Quaternion lookRotation = carVisual.transform.rotation;
        Instantiate(carPrefab, dropPosition, lookRotation);
    }
}
