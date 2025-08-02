using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GridBuildPiece : MonoBehaviour
{
    [Header("Properties")]
    public Vector2Int sizeOnGrid;
    public bool canBuildOnTop = false;
    public bool storeThisToGrid = true;
    public bool canBeRemovedFromGrid = true;
    
    public List<GridBuildPiece> gridObjectsOnTop;
    public List<Vector2Int> occupiedPositions;
    
    private void Awake()
    {
        gridObjectsOnTop = new List<GridBuildPiece>();
    }
}