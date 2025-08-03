using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackNode : MonoBehaviour 
{ 
    [Header("Connection Settings")]
    public List<TrackNode> connectedNodes = new List<TrackNode>();
    public bool probeConnectionOnSpawn = false; 
    public float probeRadius = 1.0f; 
    public LayerMask probeMask = -1;
    
    [Header("Debug")]
    public bool showConnections = true;
    
    public Vector3 Position => transform.position;
    
    private void Start()
    {
        // Subscribe to track piece removal events for cleanup
        EventBus.Subscribe<TrackPieceRemoved>(CleanNodeList);
    }
    
    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        EventBus.Unsubscribe<TrackPieceRemoved>(CleanNodeList);
    }
 
    public void AutoConnect() 
    { 
        if (!probeConnectionOnSpawn) return; 
        
        Collider[] hits = Physics.OverlapSphere(transform.position, probeRadius, probeMask); 
        
        foreach (Collider col in hits) 
        { 
            if (col.TryGetComponent(out TrackNode node)) 
            { 
                if (node != null && node != this && !connectedNodes.Contains(node)) 
                { 
                    ConnectTo(node); 
                } 
            } 
        } 
    }
 
    public void ConnectTo(TrackNode other) 
    { 
        if (other == null || other == this) return;
        
        if (!connectedNodes.Contains(other)) 
        { 
            connectedNodes.Add(other); 
        }
 
        if (!other.connectedNodes.Contains(this)) 
        { 
            other.connectedNodes.Add(this); 
        }
    }
    
    /// <summary>
    /// Disconnect this node from all connected nodes
    /// </summary>
    public void AutoDisconnect()
    {
        // Create a copy to avoid modification during iteration
        List<TrackNode> nodesToDisconnect = new List<TrackNode>(connectedNodes);
        
        foreach (TrackNode connectedNode in nodesToDisconnect)
        {
            DisconnectFrom(connectedNode);
        }
        
        // Clear the list
        connectedNodes.Clear();
    }
    
    /// <summary>
    /// Disconnect this node from a specific node (bidirectional)
    /// </summary>
    public void DisconnectFrom(TrackNode other)
    {
        if (other == null) return;
        
        // Remove from this node's connections
        connectedNodes.Remove(other);
        
        // Remove from other node's connections (bidirectional)
        // Use null check in case other node is being destroyed
        if (other.connectedNodes != null)
        {
            other.connectedNodes.Remove(this);
        }
    }
 
    public void CleanNodeList(TrackPieceRemoved @event) 
    { 
        int nullCount = connectedNodes.RemoveAll(x => x == null);
        
        if (nullCount > 0) 
        { 
            Debug.Log($"{name}: Removed {nullCount} null node references");
        } 
    }

    /// <summary>
    /// Check if this node is connected to another node
    /// </summary>
    public bool IsConnectedTo(TrackNode other)
    {
        return connectedNodes.Contains(other);
    }
    
    /// <summary>
    /// Get all connected nodes (returns a copy for safety)
    /// </summary>
    public List<TrackNode> GetConnectedNodes()
    {
        return new List<TrackNode>(connectedNodes);
    }
    
    /// <summary>
    /// Get the number of connections
    /// </summary>
    public int GetConnectionCount()
    {
        return connectedNodes.Count;
    }
 
    private void OnDrawGizmosSelected() 
    { 
        // Draw probe radius
        Gizmos.color = Color.cyan; 
        Gizmos.DrawWireSphere(transform.position, probeRadius);
        
        // Draw connections
        if (showConnections && connectedNodes != null)
        {
            Gizmos.color = Color.green;
            foreach (TrackNode connectedNode in connectedNodes)
            {
                if (connectedNode != null)
                {
                    Gizmos.DrawLine(transform.position, connectedNode.transform.position);
                    
                    // Draw a small sphere at the connected node
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(connectedNode.transform.position, 0.1f);
                    Gizmos.color = Color.green;
                }
            }
        }
    } 
}