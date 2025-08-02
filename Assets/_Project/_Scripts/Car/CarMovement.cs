using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class CarMovement : MonoBehaviour
{
    public TrackNode currentNode;
    public float thresholdDistanceToGoal = 0.2f;
    public float moveSpeed = 10f;
    public float turnSpeed = 30f;

    [Header("Node detection settings")]
    public float probeRadius = 2f;
    public float probeDistance = 5f;
    public LayerMask trackNodeLayer;

    [Header("Ground settings")]
    public float groundCheckDistance = 0.5f;
    public float groundCheckLength = 10f;
    public LayerMask groundCheckMask;

    private TrackNode _destNode;
    private Rigidbody _rb;
    private Vector3 _currentDirection;
    private float _currentSpeed;
    private TrackNode _previousNode;
    private bool _isGrounded;
    private bool _wasGroundedThisFrame;
    private bool _isDestroyed = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _currentDirection = transform.rotation * Vector3.forward;

        if (!currentNode) return;

        transform.position = currentNode.transform.position;
        _destNode = GetNextNode();
    }

    void FixedUpdate()
    {
        if (_isDestroyed) return;

        if (_destNode)
        {
            Vector3 destPos = _destNode.transform.position;
            Vector3 direction = destPos - _rb.position;
            _currentDirection = new Vector3(direction.x, 0f, direction.z).normalized;
        }

        ApplyDiveIfFalling();
        _isGrounded = IsGrounded();

        if (!_wasGroundedThisFrame && _isGrounded)
        {
            ResetTiltIfGrounded();
        }

        _wasGroundedThisFrame = _isGrounded;

        if (!_isGrounded) return;

        Vector3 flatDirection = new Vector3(_currentDirection.x, 0f, _currentDirection.z).normalized;

        // Compute turn angle
        float turnAngle = Vector3.Angle(transform.forward, flatDirection);
        float speedMultiplier = Mathf.Lerp(1f, 0.3f, Mathf.InverseLerp(0f, 90f, turnAngle));
        float actualSpeed = moveSpeed * speedMultiplier;

        // Move the car
        _rb.MovePosition(_rb.position + flatDirection * (actualSpeed * Time.fixedDeltaTime));

        // Rotate the car
        Quaternion targetRotation = Quaternion.LookRotation(flatDirection, Vector3.up);
        _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
    }

    private void ApplyDiveIfFalling()
    {
        if (!IsGrounded())
        {
            Quaternion targetRotation = Quaternion.Euler(30f, transform.eulerAngles.y, transform.eulerAngles.z); // nose down
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 60f * Time.deltaTime);
        }
    }

    private void ResetTiltIfGrounded()
    {
        if (_isGrounded)
        {
            Quaternion uprightRotation = Quaternion.Euler(0f, _rb.rotation.eulerAngles.y, 0f);
            _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, uprightRotation, 100f * Time.fixedDeltaTime));
            _rb.angularVelocity = new Vector3(0f, _rb.angularVelocity.y, 0f); // Allow only yaw
        }
    }

    private void Update()
    {
        if (!_destNode)
        {
            if (currentNode) _destNode = GetNextNode();

            if (!_destNode)
            {
                // Probe for a new node ahead
                Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * (probeDistance * 0.5f), probeRadius, trackNodeLayer);
                if (hits.Length > 0)
                {
                    // Pick the closest valid node
                    TrackNode next = hits
                        .Select(hit => hit.GetComponent<TrackNode>())
                        .Where(node => node && node != currentNode)
                        .OrderBy(node => Vector3.Distance(transform.position, node.transform.position))
                        .FirstOrDefault();

                    if (next)
                    {
                        _destNode = next;
                    }
                }
            }
        }
        else
        {
            // Check if we've reached the target
            Vector3 destPos = _destNode.transform.position;
            float distance = Vector3.Distance(new Vector3(_rb.position.x, 0, _rb.position.z), new Vector3(destPos.x, 0, destPos.z));
            if (distance <= thresholdDistanceToGoal)
            {
                _previousNode = currentNode;
                currentNode = _destNode;
                _destNode = GetNextNode();

                // Try to auto-detect curve exit
                if (_previousNode && currentNode && _destNode)
                {
                    Vector3 dir1 = (currentNode.Position - _previousNode.Position).normalized;
                    Vector3 dir2 = (_destNode.Position - currentNode.Position).normalized;

                    float angle = Vector3.Angle(dir1, dir2);

                    // If we just transitioned from a curve (sharp angle), and now go straight-ish
                    if (angle is > 30f and < 150f)
                    {
                        float curveSharpness = Vector3.Dot(dir1, dir2); // closer to 1 = straight
                        if (curveSharpness < 0.95f)
                        {
                            SnapCarToGrid();
                        }
                    }
                }
            }
        }
    }

    private void SnapCarToGrid()
    {
        // Snap Y rotation to nearest 90°
        Vector3 euler = _rb.rotation.eulerAngles;
        float snappedY = Mathf.Round(euler.y / 90f) * 90f;
        Quaternion snappedRotation = Quaternion.Euler(0f, snappedY, 0f);

        _rb.MoveRotation(snappedRotation);

        // Position snap only horizontally, preserve Y
        Vector3 pos = _rb.position;
        pos.x = Mathf.Round(pos.x * 2f) / 2f; // Snap to 0.5 grid if needed
        pos.z = Mathf.Round(pos.z * 2f) / 2f;
        _rb.MovePosition(new Vector3(pos.x, pos.y, pos.z));

        // Stop physics drift
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    private bool IsGrounded()
    {
        return Physics.CheckCapsule(
            transform.position + transform.forward * (groundCheckLength * 0.5f),
            transform.position - transform.forward * (groundCheckLength * 0.5f),
            groundCheckDistance,
            groundCheckMask);
    }

    private TrackNode GetNextNode()
    {
        if (!currentNode) return null;

        return currentNode.connectedNodes
            .Where(n =>
                    Vector3.Dot((n.Position - currentNode.Position).normalized, transform.forward.normalized) > -0.2f // accept wide cone, exclude hard backward
            )
            .OrderByDescending(n =>
                Vector3.Dot((n.Position - currentNode.Position).normalized, transform.forward.normalized)
            )
            .FirstOrDefault();
    }

    // Public method to set destroyed state (called by CarExplosionHandler)
    public void SetDestroyed(bool destroyed)
    {
        _isDestroyed = destroyed;
    }

    // Public method to check if car is destroyed
    public bool IsDestroyed()
    {
        return _isDestroyed;
    }

    private void OnDrawGizmosSelected()
    {
        if (_destNode)
        {
            float height = 10f;
            Vector3 carTransform = transform.position;
            carTransform.y = height;
            Vector3 destTransform = _destNode.transform.position;
            destTransform.y = height;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(carTransform, destTransform);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_destNode.transform.position, destTransform);
        }

        // Draw forward OverlapSphere
        Gizmos.color = Color.yellow;
        Vector3 probeOrigin = transform.position + transform.forward * probeDistance * 0.5f;
        Gizmos.DrawWireSphere(probeOrigin, probeRadius);
    }
}
