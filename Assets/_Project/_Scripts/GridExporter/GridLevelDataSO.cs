using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "GridLevel", menuName = "Grid System/Grid Level Data")]
public class GridLevelDataSO : ScriptableObject
{
    [Header("Level Information")]
    public string levelName;
    [TextArea(3, 5)]
    public string description;
    
    [Header("Grid Configuration")]
    public Vector2Int gridSize;
    public float cellSize;
    public Vector3 gridOrigin;
    
    [Header("Build Pieces")]
    public List<GridBuildPieceData> buildPieces = new List<GridBuildPieceData>();
    
    [Header("Metadata")]
    public string creationDate;
    public string lastModified;
    public int pieceCount;
    
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(levelName))
            levelName = name;
        
        pieceCount = buildPieces?.Count ?? 0;
        
        if (string.IsNullOrEmpty(creationDate))
            creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        lastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    public bool IsValid()
    {
        return gridSize.x > 0 && gridSize.y > 0 && buildPieces != null;
    }
}