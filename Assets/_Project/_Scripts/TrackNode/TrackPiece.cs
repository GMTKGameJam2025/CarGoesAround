using System.Collections.Generic;
using UnityEngine;

public class TrackPiece : MonoBehaviour
{
    public Transform trackNodeParent;

    public List<TrackNode> trackNodes;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform trackNode in trackNodeParent)
        {
            if (trackNode.TryGetComponent(out TrackNode node))
            {
                trackNodes.Add(node);    
            }
        }
        OnBuild();
    }

    void OnBuild()
    {
        foreach (TrackNode node in trackNodes)
        {
            node.AutoConnect();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
