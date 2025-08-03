using System.Collections;
using UnityEngine;

public class CarExplosionHandler : MonoBehaviour
{
    [Header("Ground Collision Settings")]
    public LayerMask groundCheckMask;

    [Header("Road Protection Settings")]
    public LayerMask roadLayer;
    public float roadCheckDistance = 1f;

    [Header("Crash Collision Settings")]
    public LayerMask crashLayerMask; // Objects that cause crashes (walls, obstacles, etc.)

    [Header("Explosion Settings")]
    public float explosionForce = 500f;
    public float explosionRadius = 5f;
    public float explosionUpwardModifier = 3f;
    public GameObject explosionEffectPrefab;
    public AudioClip explosionSound;

    [Header("Crash Settings")]
    public float crashForce = 200f;
    public float crashRadius = 3f;
    public float crashDownwardModifier = 2f; // Makes parts fall down instead of up
    public GameObject crashEffectPrefab;
    public AudioClip crashSound;

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
        if (_isDestroyed) return;

        // Check if the car hit the ground layer
        if (((1 << collision.gameObject.layer) & groundCheckMask) != 0)
        {
            // Only explode if we're NOT on a road
            if (!IsOnRoad())
            {
                ExplodeCar();
                EventBus.Fire<GameOverEvent>(new("You let the car off the road"));
                return;
            }
        }

        // Check if the car crashed into something
        if (((1 << collision.gameObject.layer) & crashLayerMask) != 0)
        {
            CrashCar();
            EventBus.Fire<GameOverEvent>(new("You let the car crashed into something"));
        }
    }

    private bool IsOnRoad()
    {
        // Cast a ray downward from the car to check for roads
        Vector3 rayStart = transform.position;
        Vector3 rayEnd = transform.position + Vector3.down * roadCheckDistance;

        return Physics.Linecast(rayStart, rayEnd, roadLayer);
    }

    private void CrashCar()
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

        // Play crash effect
        if (crashEffectPrefab != null)
        {
            Instantiate(crashEffectPrefab, transform.position, Quaternion.identity);
        }

        // Play crash sound
        if (crashSound != null)
        {
            AudioSource.PlayClipAtPoint(crashSound, transform.position);
        }

        // Create crash force on car parts (falling instead of exploding)
        StartCoroutine(CrashCarParts());
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

    private IEnumerator CrashCarParts()
    {
        Transform[] carParts = GetComponentsInChildren<Transform>();

        foreach (Transform part in carParts)
        {
            if (part == transform) continue; // Skip the parent object

            // Always add Rigidbody to each part
            Rigidbody partRb = part.GetComponent<Rigidbody>();
            if (partRb == null)
            {
                partRb = part.gameObject.AddComponent<Rigidbody>();
            }

            // Configure rigidbody
            partRb.mass = 0.8f; // Heavier parts for crash
            partRb.linearDamping = 0.5f; // Some air resistance

            // Add collider if missing to prevent clipping through ground
            Collider partCollider = part.GetComponent<Collider>();
            if (partCollider == null)
            {
                // Add a box collider as default
                BoxCollider boxCollider = part.gameObject.AddComponent<BoxCollider>();
                // Auto-fit the collider to the mesh if possible
                Renderer partRenderer = part.GetComponent<Renderer>();
                if (partRenderer != null)
                {
                    boxCollider.size = partRenderer.bounds.size;
                }
            }

            // Detach from parent
            part.SetParent(null);

            // Apply crash force (same as explosion - outward from center)
            Vector3 crashPosition = transform.position - Vector3.up * 1f;
            partRb.AddExplosionForce(crashForce, crashPosition, crashRadius, crashDownwardModifier);

            // Add random spinning
            Vector3 randomTorque = new Vector3(
                Random.Range(-5f, 5f),
                Random.Range(-5f, 5f),
                Random.Range(-5f, 5f)
            );
            partRb.AddTorque(randomTorque, ForceMode.Impulse);

            // Destroy the part after some time
            Destroy(part.gameObject, Random.Range(3f, 7f));
        }

        // Wait a frame then destroy the main car object
        yield return null;
        Destroy(gameObject, 0.5f);
    }

    private IEnumerator ExplodeCarParts()
    {
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
                Random.Range(-10f, 10f),
                Random.Range(-10f, 10f),
                Random.Range(-10f, 10f)
            );
            partRb.AddTorque(randomTorque, ForceMode.Impulse);

            // Destroy the part after some time
            Destroy(part.gameObject, Random.Range(3f, 8f));
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

    // Public method to manually trigger crash (if needed)
    public void TriggerCrash(Vector3 crashPoint)
    {
        CrashCar();
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

        // Draw crash radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, crashRadius);

        // Draw road check ray
        Gizmos.color = Color.blue;
        Vector3 rayStart = transform.position;
        Vector3 rayEnd = transform.position + Vector3.down * roadCheckDistance;
        Gizmos.DrawLine(rayStart, rayEnd);
    }
}