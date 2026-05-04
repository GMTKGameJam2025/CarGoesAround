using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Handles all grid-export logic for <see cref="GridExportImportSystem"/>.
/// Walks the live grid, collects every unique <see cref="GridBuildPiece"/>, and
/// serialises them into a new <see cref="GridLevelDataSO"/> asset.
/// </summary>
public class GridExporter
{
    private readonly GridExportImportSystem _system;

    public GridExporter(GridExportImportSystem system)
    {
        _system = system;
    }

    // -------------------------------------------------------------------------
    // Public entry point
    // -------------------------------------------------------------------------

    /// <summary>
    /// Snapshots the current grid into a new <see cref="GridLevelDataSO"/> instance
    /// and fires <see cref="GridExportImportSystem.OnGridExported"/>.
    /// </summary>
    /// <param name="fileName">Optional asset name; defaults to a timestamped string.</param>
    public GridLevelDataSO ExportGrid(string fileName = null)
    {
        if (_system.gridManager?.Grid == null)
        {
            Debug.LogError("[GridExporter] GridManager or Grid is null.");
            return null;
        }

        GridLevelDataSO levelData = ScriptableObject.CreateInstance<GridLevelDataSO>();
        PopulateLevelMetadata(levelData, fileName);
        ExportAllPieces(levelData);

        Debug.Log($"[GridExporter] Exported {levelData.buildPieces.Count} piece(s) to '{levelData.levelName}'.");
        _system.OnGridExported?.Invoke(levelData);
        return levelData;
    }

#if UNITY_EDITOR
    /// <summary>Writes <paramref name="levelData"/> to disk as a Unity asset file.</summary>
    /// <param name="fileName">Optional file name (without path); defaults to the level name.</param>
    /// <returns><c>true</c> on success.</returns>
    public bool SaveToFile(GridLevelDataSO levelData, string fileName = null)
    {
        if (levelData == null)
        {
            Debug.LogError("[GridExporter] Cannot save null GridLevelDataSO.");
            return false;
        }

        string folder = _system.defaultSaveFolder;
        if (!System.IO.Directory.Exists(folder))
            System.IO.Directory.CreateDirectory(folder);

        string name = fileName ?? (levelData.levelName + ".asset");
        if (!name.EndsWith(".asset")) name += ".asset";

        string path = System.IO.Path.Combine(folder, name);
        AssetDatabase.CreateAsset(levelData, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[GridExporter] Saved GridLevelDataSO to '{path}'.");
        return true;
    }
#endif

    // -------------------------------------------------------------------------
    // Private helpers — metadata
    // -------------------------------------------------------------------------

    private void PopulateLevelMetadata(GridLevelDataSO levelData, string fileName)
    {
        levelData.levelName    = fileName ?? $"GridLevel_{DateTime.Now:yyyyMMdd_HHmmss}";
        levelData.gridSize     = new Vector2Int(_system.gridManager.Grid.Width, _system.gridManager.Grid.Height);
        levelData.cellSize     = _system.gridManager.Grid.CellSize;
        levelData.gridOrigin   = _system.gridManager.transform.position;
        levelData.creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    // -------------------------------------------------------------------------
    // Private helpers — piece collection
    // -------------------------------------------------------------------------

    private void ExportAllPieces(GridLevelDataSO levelData)
    {
        var pieceToId      = new Dictionary<GridBuildPiece, string>();
        var processedPieces = new HashSet<GridBuildPiece>();

        // Pass 1 — discover every unique piece and assign a stable GUID.
        for (int x = 0; x < levelData.gridSize.x; x++)
        {
            for (int z = 0; z < levelData.gridSize.y; z++)
            {
                GridCell cell = _system.gridManager.Grid.GetGridObject(x, z);
                if (cell != null)
                    CollectPiecesFromCell(cell, processedPieces, pieceToId);
            }
        }

        // Pass 2 — serialise each discovered piece.
        foreach (var kvp in pieceToId)
            levelData.buildPieces.Add(BuildPieceData(kvp.Key, kvp.Value, pieceToId));
    }

    /// <summary>
    /// Non-destructively reads the full stack in <paramref name="cell"/> and registers
    /// any piece not yet in <paramref name="pieceToId"/>.
    /// </summary>
    private void CollectPiecesFromCell(
        GridCell cell,
        HashSet<GridBuildPiece> processed,
        Dictionary<GridBuildPiece, string> pieceToId)
    {
        foreach (GridBuildPiece piece in GetStackNonDestructive(cell))
        {
            if (piece == null || processed.Contains(piece)) continue;
            processed.Add(piece);
            pieceToId[piece] = Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Returns all pieces in the cell's stack without modifying the stack.
    /// Bottom piece is first; top piece is last.
    /// </summary>
    private static List<GridBuildPiece> GetStackNonDestructive(GridCell cell)
    {
        var result = new List<GridBuildPiece>();
        GridBuildPiece top = cell.GetTopGridObject();

        if (top == null) return result;

        // Walk from top piece downward via objectsOnTop inverse relationships.
        // Since GridCell only exposes Peek(), we reconstruct using GridObjectsOnTop.
        CollectPieceRecursive(top, result, new HashSet<GridBuildPiece>());
        return result;
    }

    private static void CollectPieceRecursive(
        GridBuildPiece piece,
        List<GridBuildPiece> result,
        HashSet<GridBuildPiece> visited)
    {
        if (piece == null || visited.Contains(piece)) return;
        visited.Add(piece);
        result.Add(piece);

        foreach (GridBuildPiece child in piece.GridObjectsOnTop)
            CollectPieceRecursive(child, result, visited);
    }

    // -------------------------------------------------------------------------
    // Private helpers — piece serialisation
    // -------------------------------------------------------------------------

    private static GridBuildPieceData BuildPieceData(
        GridBuildPiece piece,
        string uniqueId,
        Dictionary<GridBuildPiece, string> pieceToId)
    {
        var data = new GridBuildPieceData
        {
            uniqueId             = uniqueId,
            pieceId              = piece.id,
            worldPosition        = piece.transform.position,
            rotation             = piece.transform.eulerAngles,
            sizeOnGrid           = piece.sizeOnGrid,
            canBuildOnTop        = piece.CanBuildOnTop,
            storeThisToGrid      = piece.storeThisToGrid,
            canBeRemovedFromGrid = piece.CanBeRemovedFromGrid,
            layer                = piece.Layer,
            canBeBuiltOnLayers   = piece.CanBeBuiltOnLayers,
            occupiedPositions    = new List<Vector2Int>(piece.OccupiedPositions),
            originGridPosition   = piece.OccupiedPositions.Count > 0
                                       ? piece.OccupiedPositions[0]
                                       : Vector2Int.zero,
            stackDepth           = CalculateStackDepth(piece),
        };

        // Record IDs of pieces stacked on top.
        foreach (GridBuildPiece child in piece.GridObjectsOnTop)
        {
            if (pieceToId.TryGetValue(child, out string childId))
                data.objectsOnTopIds.Add(childId);
        }

        return data;
    }

    /// <summary>
    /// Returns how deep this piece is in the stack by counting how many pieces
    /// are stacked on top of it transitively. Ground pieces return 0.
    /// </summary>
    private static int CalculateStackDepth(GridBuildPiece piece)
    {
        int depth = 0;
        GridBuildPiece current = piece;

        while (current.GridObjectsOnTop != null && current.GridObjectsOnTop.Count > 0)
        {
            current = current.GridObjectsOnTop[0];
            depth++;
        }

        return depth;
    }
}
