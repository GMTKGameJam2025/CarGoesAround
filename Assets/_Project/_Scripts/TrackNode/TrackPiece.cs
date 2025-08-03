using System;
using System.Collections.Generic;
using UnityEngine;

public struct TrackPieceRemoved : IGameEvent 
{
    public TrackPiece RemovedPiece { get; }
    
    public TrackPieceRemoved(TrackPiece piece)
    {
        RemovedPiece = piece;
    }
}

public class TrackPiece : MonoBehaviour, IBuildable
{
    [Header("Track Nodes")]
    public Transform trackNodeParent;
    public List<TrackNode> trackNodes = new List<TrackNode>();
    
    [Header("Visual")]
    public Renderer trackRenderer;
    public Material defaultMaterial;
    public Material lockedMaterial;
    
    void Awake()
    {
        // Initialize the list
        trackNodes.Clear();
        
        // Find all track nodes under the parent
        if (trackNodeParent != null)
        {
            foreach (Transform trackNode in trackNodeParent)
            {
                if (trackNode.TryGetComponent(out TrackNode node))
                {
                    trackNodes.Add(node);
                }
            }
        }
        
        Debug.Log($"TrackPiece {name} found {trackNodes.Count} track nodes");
    }
    
    public void OnBuild(GridBuildPiece piece)
    {
        // Connect all nodes when the piece is built
        foreach (TrackNode node in trackNodes)
        {
            if (node != null)
            {
                node.AutoConnect();
            }
        }
        
        // Set visual material based on whether it can be removed
        if (trackRenderer != null)
        {
            trackRenderer.material = piece.canBeRemovedFromGrid ? defaultMaterial : lockedMaterial;
        }
        
        Debug.Log($"TrackPiece {name} built with {trackNodes.Count} connected nodes");
    }
    
    
    /// <summary>
    /// Disconnect all track nodes from their connections
    /// </summary>
    private void DisconnectAllNodes()
    {
        foreach (TrackNode node in trackNodes)
        {
            if (node != null)
            {
                node.AutoDisconnect();
            }
        }
        
        Debug.Log($"TrackPiece {name} disconnected all nodes");
    }
    
    /// <summary>
    /// Called when the object is being destroyed
    /// </summary>
    private void OnDisable()
    {
        // Emergency cleanup in case RemoveTrackPiece() wasn't called
        if (trackNodes != null)
        {
            DisconnectAllNodes();
            EventBus.Fire<TrackPieceRemoved>(new TrackPieceRemoved(this));
        }
    }
    
    /// <summary>
    /// Get all track nodes belonging to this piece
    /// </summary>
    public List<TrackNode> GetTrackNodes()
    {
        return new List<TrackNode>(trackNodes);
    }
    
    /// <summary>
    /// Get the number of track nodes on this piece
    /// </summary>
    public int GetNodeCount()
    {
        return trackNodes.Count;
    }
    
    /// <summary>
    /// Check if this piece has any connected nodes
    /// </summary>
    public bool HasConnections()
    {
        foreach (TrackNode node in trackNodes)
        {
            if (node != null && node.GetConnectionCount() > 0)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Get total number of connections across all nodes
    /// </summary>
    public int GetTotalConnections()
    {
        int totalConnections = 0;
        foreach (TrackNode node in trackNodes)
        {
            if (node != null)
            {
                totalConnections += node.GetConnectionCount();
            }
        }
        return totalConnections;
    }
}