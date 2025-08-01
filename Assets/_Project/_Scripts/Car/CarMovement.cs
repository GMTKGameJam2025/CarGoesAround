using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class CarMovement : MonoBehaviour {
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

    void Start() {
        _rb = GetComponent<Rigidbody>();
        _currentDirection = transform.rotation * Vector3.forward; 
        
        if (!currentNode) return;

        transform.position = currentNode.transform.position;
        _destNode = GetNextNode();
    }

    void FixedUpdate() {
        if (_destNode)
        {
            Vector3 destPos = _destNode.transform.position;
            Vector3 direction = (destPos - _rb.position);
            _currentDirection = new Vector3(direction.x, 0f, direction.z).normalized;
        }

        ApplyDiveIfFalling();
        ResetTiltIfGrounded();
        if (!IsGrounded()) return;
        
        // Compute turn angle
        float turnAngle = Vector3.Angle(transform.forward, _currentDirection);
        float speedMultiplier = Mathf.Lerp(1f, 0.3f, Mathf.InverseLerp(0f, 90f, turnAngle));
        float actualSpeed = moveSpeed * speedMultiplier;

        // Move the car
        _rb.MovePosition(_rb.position + _currentDirection * (actualSpeed * Time.fixedDeltaTime));

        // Rotate the car
        Quaternion targetRotation = Quaternion.LookRotation(_currentDirection, Vector3.up);
        _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
    }
    
    private void ApplyDiveIfFalling() {
        if (!IsGrounded()) {
            Quaternion targetRotation = Quaternion.Euler(30f, transform.eulerAngles.y, transform.eulerAngles.z); // nose down
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 60f * Time.deltaTime);
        }
    }
    
    private void ResetTiltIfGrounded() {
        if (IsGrounded()) {
            Quaternion uprightRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, uprightRotation, 100f * Time.deltaTime);
        }
    }

    private void Update()
    {
        if (!_destNode) {
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
            if (distance <= thresholdDistanceToGoal) {
                currentNode = _destNode;
                _destNode = GetNextNode();
            }
        }
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
    
    private void OnDrawGizmosSelected() {
        if (_destNode) {
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
