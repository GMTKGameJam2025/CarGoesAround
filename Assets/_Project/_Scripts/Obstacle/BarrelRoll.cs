using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BarrelRoll : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float lifetime = 5f;

    private Rigidbody rb;
    private Vector3 rollDirection = Vector3.forward; // Default forward direction

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, 1f);

        if (isGrounded)
        {
            Vector3 velocity = rollDirection * moveSpeed;
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;
        }
    }

    public void SetRollDirection(Vector3 direction)
    {
        rollDirection = direction.normalized;
    }
}