using System;
using System.Collections.Generic;
using UnityEngine;

public struct TrackPieceRemoved : IGameEvent {}

public class TrackPiece : MonoBehaviour, IBuildable
{
    public Transform trackNodeParent;

    public List<TrackNode> trackNodes;

    public Renderer trackRenderer;
    public Material defaultMaterial;
    public Material lockedMaterial;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        foreach (Transform trackNode in trackNodeParent)
        {
            if (trackNode.TryGetComponent(out TrackNode node))
            {
                trackNodes.Add(node);
            }
        }
    }

    public void OnBuild(GridBuildPiece piece)
    {
        foreach (TrackNode node in trackNodes)
        {
            node.AutoConnect();
        }

        trackRenderer.material = piece.canBeRemovedFromGrid ? defaultMaterial : lockedMaterial;
    }

    private void OnDisable()
    {
        EventBus.Fire<TrackPieceRemoved>(new());
    }
}
