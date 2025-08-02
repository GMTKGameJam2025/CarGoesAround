using System.Collections;
using UnityEngine;

public class CarExplosionHandler : MonoBehaviour
{
    [Header("Ground Collision Settings")]
    public LayerMask groundCheckMask;

    [Header("Road Protection Settings")]
    public LayerMask roadLayer;
    public float roadCheckDistance = 1f;

    [Header("Explosion Settings")]
    public float explosionForce = 500f;
    public float explosionRadius = 5f;
    public float explosionUpwardModifier = 3f;
    public GameObject explosionEffectPrefab;
    public AudioClip explosionSound;

    private bool _isDestroyed = false;
    private Rigidbody _rb;
    private CarMovement _carMovement;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _carMovement = GetComponent<CarMovement>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the car hit the ground layer
        if (((1 << collision.gameObject.layer) & groundCheckMask) != 0 && !_isDestroyed)
        {
            // Only explode if we're NOT on a road
            if (!IsOnRoad())
            {
                ExplodeCar();
            }
        }
    }

    private bool IsOnRoad()
    {
        // Cast a ray downward from the car to check for roads
        Vector3 rayStart = transform.position;
        Vector3 rayEnd = transform.position + Vector3.down * roadCheckDistance;

        return Physics.Linecast(rayStart, rayEnd, roadLayer);
    }

    private void ExplodeCar()
    {
        if (_isDestroyed) return;
        _isDestroyed = true;

        // Notify CarMovement that we're destroyed
        if (_carMovement != null)
        {
            _carMovement.SetDestroyed(true);
        }

        // Stop car movement
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        // Play explosion effect
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Play explosion sound
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // Create explosion force on car parts
        StartCoroutine(ExplodeCarParts());
    }

    private IEnumerator ExplodeCarParts()
    {
        // Get all child objects (car parts)
        Transform[] carParts = GetComponentsInChildren<Transform>();

        foreach (Transform part in carParts)
        {
            if (part == transform) continue; // Skip the parent object

            // Add Rigidbody to each part if it doesn't have one
            Rigidbody partRb = part.GetComponent<Rigidbody>();
            if (partRb == null)
            {
                partRb = part.gameObject.AddComponent<Rigidbody>();
                partRb.mass = 0.5f; // Light parts
            }

            // Detach from parent
            part.SetParent(null);

            // Apply explosion force
            Vector3 explosionPosition = transform.position - Vector3.up * 1f;
            partRb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, explosionUpwardModifier);

            // Add some random torque for realistic spinning
            Vector3 randomTorque = new Vector3(
                UnityEngine.Random.Range(-10f, 10f),
                UnityEngine.Random.Range(-10f, 10f),
                UnityEngine.Random.Range(-10f, 10f)
            );
            partRb.AddTorque(randomTorque, ForceMode.Impulse);

            // Destroy the part after some time
            Destroy(part.gameObject, UnityEngine.Random.Range(3f, 8f));
        }

        // Wait a frame then destroy the main car object
        yield return null;
        Destroy(gameObject, 0.5f);
    }

    // Public method to manually trigger explosion (if needed)
    public void TriggerExplosion()
    {
        ExplodeCar();
    }

    // Check if the car is destroyed
    public bool IsDestroyed()
    {
        return _isDestroyed;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        // Draw road check ray
        Gizmos.color = Color.blue;
        Vector3 rayStart = transform.position;
        Vector3 rayEnd = transform.position + Vector3.down * roadCheckDistance;
        Gizmos.DrawLine(rayStart, rayEnd);
    }
}