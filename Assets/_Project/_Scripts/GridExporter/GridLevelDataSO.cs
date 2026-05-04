using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that stores a complete snapshot of a grid level.
/// Created at export time and assigned to <see cref="GridExportImportSystem"/> for loading.
/// </summary>
[CreateAssetMenu(fileName = "GridLevel", menuName = "Grid System/Grid Level Data")]
public class GridLevelDataSO : ScriptableObject
{
    // -------------------------------------------------------------------------
    // Level information
    // -------------------------------------------------------------------------

    [Header("Level Information")]
    public string levelName;

    [TextArea(3, 5)]
    public string description;

    // -------------------------------------------------------------------------
    // Grid configuration
    // -------------------------------------------------------------------------

    [Header("Grid Configuration")]
    public Vector2Int gridSize;
    public float cellSize;
    public Vector3 gridOrigin;

    // -------------------------------------------------------------------------
    // Piece data
    // -------------------------------------------------------------------------

    [Header("Build Pieces")]
    public List<GridBuildPieceData> buildPieces = new List<GridBuildPieceData>();

    // -------------------------------------------------------------------------
    // Metadata
    // -------------------------------------------------------------------------

    [Header("Metadata")]
    public string creationDate;
    public string lastModified;

    /// <summary>Kept in sync with <see cref="buildPieces"/>.Count via <see cref="OnValidate"/>.</summary>
    public int pieceCount;

    // -------------------------------------------------------------------------
    // Unity callbacks
    // -------------------------------------------------------------------------

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(levelName))
            levelName = name;

        pieceCount = buildPieces?.Count ?? 0;

        if (string.IsNullOrEmpty(creationDate))
            creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        lastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    // -------------------------------------------------------------------------
    // Validation helper
    // -------------------------------------------------------------------------

    /// <summary>Returns <c>true</c> when the asset contains enough data to load a level.</summary>
    public bool IsValid() => gridSize.x > 0 && gridSize.y > 0 && buildPieces != null;
}
