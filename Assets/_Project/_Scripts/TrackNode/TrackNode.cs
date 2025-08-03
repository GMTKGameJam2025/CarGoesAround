using System;
using System.Collections.Generic;
using System.Linq;
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

    public void ConnectTo(TrackNode other) {
        if (!connectedNodes.Contains(other))
        {
            connectedNodes.Add(other);
        }

        if (!other.connectedNodes.Contains(this))
        {
            other.connectedNodes.Add(this);
        }
    }

    public void CleanNodeList(TrackPieceRemoved @event)
    {
        int pieceNumber = 0;
        connectedNodes.RemoveAll(x =>
        {
            if (x == null)
            {
                pieceNumber += 1;
            }
            return x == null;
        });

        if (pieceNumber > 0)
        {
            Debug.Log("Removing null pieces");
        }
    }

    private void Awake()
    {
        EventBus.Subscribe<TrackPieceRemoved>(CleanNodeList);
    }
    
    private void OnDestroy()
    {
        EventBus.Unsubscribe<TrackPieceRemoved>(CleanNodeList);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, probeRadius);
    }
}
