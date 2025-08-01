using System;
using System.Collections.Generic;
using UnityEngine;

public class TrackNode : MonoBehaviour
{
    public List<TrackNode> connectedNodes;
    
    public Vector3 Position => transform.position;

    public bool probeConnectionOnSpawn = false;
    public float probeRadius = 1.0f;
    public LayerMask probeMask;

    public void AutoConnect()
    {
        if (!probeConnectionOnSpawn) return;
        
        Collider[] hits = Physics.OverlapSphere(transform.position, probeRadius, probeMask);
        
        foreach (Collider col in hits) {
            if (col.TryGetComponent(out TrackNode node))
            {
                if (node != null && node != this && !connectedNodes.Contains(node)) {
                    ConnectTo(node);
                }
            }
        }
    }

    private void ConnectTo(TrackNode other) {
        connectedNodes.Add(other);
        other.connectedNodes.Add(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, probeRadius);
    }
}
