using UnityEngine;
using System.Collections;

public class GridPlacementAnimator : MonoBehaviour
{
    [SerializeField] private float fallHeight = 1f;
    [SerializeField] private float fallSpeed = 5f;
    
    public void StartFallAnimation(Vector3 targetPosition)
    {
        // Set starting position above target
        transform.position = targetPosition + Vector3.up * fallHeight;
        
        // Start falling
        StartCoroutine(Fall(targetPosition));
    }
    
    private IEnumerator Fall(Vector3 targetPosition)
    {
        while (transform.position.y > targetPosition.y)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);
            yield return null;
        }
        
        // Snap to exact position
        transform.position = targetPosition;
    }
}