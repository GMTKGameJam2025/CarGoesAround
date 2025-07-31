using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Car : MonoBehaviour
{
    public TrackNode currentNode;
    public float thresholdDistanceToGoal = 0.2f;
    public float moveSpeed = 10f;
    public float turnSpeed = 30f;

    private TrackNode _destNode;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!currentNode) return;
        
        transform.position = currentNode.transform.position;
        _destNode = currentNode.nextNode;
    }

    // Update is called once per frame
    void Update()
    {
        if (!currentNode) return;
        
        if (!_destNode)
        {
            //Try get nextNode;
            _destNode = currentNode.nextNode;
            return;
        }

        Transform destTransform = _destNode.transform;
        Vector3 carPosNoHeight = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 destPosNoHeight = new Vector3(destTransform.position.x, 0, destTransform.position.z);
        if ((destPosNoHeight- carPosNoHeight).normalized != transform.forward)
        {
            Quaternion destRot = Quaternion.LookRotation(destPosNoHeight - carPosNoHeight, transform.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, destRot, turnSpeed * Time.deltaTime);
        }
        
        if (Vector3.Distance(transform.position, destTransform.position) > thresholdDistanceToGoal)
        {
            transform.position = Vector3.MoveTowards(transform.position, destTransform.position, moveSpeed * Time.deltaTime);
            return;
        }
        else
        {
            currentNode = _destNode;
            transform.position = currentNode.transform.position;
            _destNode = currentNode.nextNode;
        }
    }

    private void OnDrawGizmos()
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
    }
}
