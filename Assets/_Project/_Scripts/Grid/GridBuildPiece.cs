using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class GridBuildPiece : MonoBehaviour
{
    [Header("PieceData")]
    public int id;
    
    [Header("Properties")]
    public Vector2Int sizeOnGrid = Vector2Int.one;
    public bool canBuildOnTop = false;
    public bool storeThisToGrid = true;
    public bool canBeRemovedFromGrid = true;

    [Header("Build Layering")]
    public BuildLayer layer = BuildLayer.Ground;
    public BuildLayer canBeBuiltOnLayers = BuildLayer.Ground;

    public List<GridBuildPiece> gridObjectsOnTop;
    public List<Vector2Int> occupiedPositions;
    
    public void Init(BuildPieceData data)
    {
        if (data == null)
        {
            Debug.LogError("BuildPieceData is null.");
            return;
        }

        id = data.ID;
        sizeOnGrid = data.Size;
        canBuildOnTop = data.canBuildOnTop;
        storeThisToGrid = data.storeThisToGrid;
        layer = data.layer;
        canBeBuiltOnLayers = data.canBeBuiltOnLayers;

        // Initialize empty lists just in case (defensive)
        gridObjectsOnTop = new List<GridBuildPiece>();
        occupiedPositions = new List<Vector2Int>();

        OnBuild();
    }

    public void OnBuild()
    {
        List<IBuildable> allBuildable = GetComponents<IBuildable>().ToList();

        foreach (IBuildable buildable in allBuildable)
        {
            buildable.OnBuild();
        }
    }
}