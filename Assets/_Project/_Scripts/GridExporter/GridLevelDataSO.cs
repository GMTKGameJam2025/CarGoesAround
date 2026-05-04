using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores all data required to recreate a grid level, including grid configuration,
/// placed build pieces, and metadata such as creation and modification timestamps.
/// </summary>
[CreateAssetMenu(fileName = "GridLevel", menuName = "Grid System/Grid Level Data")]
public class GridLevelDataSO : ScriptableObject
{
    #region Fields

    [Header("Level Information")]
    /// <summary>Human-readable name identifying this level.</summary>
    public string levelName;

    /// <summary>Optional multi-line description of the level's purpose or contents.</summary>
    [TextArea(3, 5)]
    public string description;

    [Header("Grid Configuration")]
    /// <summary>Number of columns (x) and rows (y) in the grid.</summary>
    public Vector2Int gridSize;

    /// <summary>World-space size of a single grid cell.</summary>
    public float cellSize;

    /// <summary>World-space position of the grid's origin (bottom-left corner).</summary>
    public Vector3 gridOrigin;

    [Header("Build Pieces")]
    /// <summary>All build pieces that have been placed on this grid level.</summary>
    public List<GridBuildPieceData> buildPieces = new List<GridBuildPieceData>();

    [Header("Metadata")]
    /// <summary>ISO-8601 timestamp recorded when this asset was first created.</summary>
    public string creationDate;

    /// <summary>ISO-8601 timestamp updated each time the asset is modified outside of batch/CI mode.</summary>
    public string lastModified;

    /// <summary>Cached count of build pieces; refreshed automatically in OnValidate.</summary>
    public int pieceCount;

    #endregion

    #region Validation

    /// <summary>
    /// Called by the Unity editor whenever the asset is loaded or a field is changed.
    /// Ensures default values are populated and metadata is kept up to date.
    /// The <see cref="lastModified"/> timestamp is intentionally skipped in batch mode
    /// to avoid silent timestamp churn during CI builds or headless imports.
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(levelName))
            levelName = name;

        pieceCount = buildPieces?.Count ?? 0;

        if (string.IsNullOrEmpty(creationDate))
            creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        if (!Application.isBatchMode)
            lastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Returns <c>true</c> when the asset contains a valid, non-zero grid size
    /// and a non-null build-piece list, indicating it is safe to use at runtime.
    /// </summary>
    /// <returns><c>true</c> if the level data is considered valid; otherwise <c>false</c>.</returns>
    public bool IsValid()
    {
        return gridSize.x > 0 && gridSize.y > 0 && buildPieces != null;
    }

    #endregion
}