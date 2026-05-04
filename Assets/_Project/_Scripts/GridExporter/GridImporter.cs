Assets/_Project/_Scripts/GridExporter/GridImporter.cs
```
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles all grid-import logic for <see cref="GridExportImportSystem"/>.
/// Validates a <see cref="GridLevelDataSO"/>, clears the live grid, then
/// re-instantiates every piece in correct bottom-up stacking order.
/// </summary>
public class GridImporter
{
    private readonly GridExportImportSystem _system;

    public GridImporter(GridExportImportSystem system)
    {
        _system = system;
    }

    // -------------------------------------------------------------------------
    // Public entry point
    // -------------------------------------------------------------------------

    /// <summary>
    /// Clears the current grid and rebuilds it from <paramref name="levelData"/>.
    /// Fires <see cref="GridExportImportSystem.OnGridLoaded"/> on success, or
    /// <see cref="GridExportImportSystem.OnGridLoadFailed"/> on failure.
    /// </summary>
    /// <returns><c>true</c> on success.</returns>
    public bool LoadGrid(GridLevelDataSO levelData)
    {
        if (!ValidateBeforeLoad(levelData)) return false;

        ClearExistingGrid();

        List<GridBuildPieceData> ordered = OrderByStackDepth(levelData.buildPieces);
        bool allPlaced = PlaceAllPieces(ordered);

        if (allPlaced)
        {
            Debug.Log($"[GridImporter] Loaded '{levelData.levelName}' ({levelData.buildPieces.Count} piece(s)).");
            _system.OnGridLoaded?.Invoke(levelData);
        }
        else
        {
            Debug.LogWarning($"[GridImporter] Some pieces could not be placed from '{levelData.levelName}'.");
            _system.OnGridLoadFailed?.Invoke();
        }

        return allPlaced;
    }

    // -------------------------------------------------------------------------
    // Validation
    // -------------------------------------------------------------------------

    private bool ValidateBeforeLoad(GridLevelDataSO levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("[GridImporter] GridLevelDataSO is null.");
            _system.OnGridLoadFailed?.Invoke();
            return false;
        }

        if (!levelData.IsValid())
        {
            Debug.LogError($"[GridImporter] '{levelData.levelName}' failed validation (invalid grid size or null piece list).");
            _system.OnGridLoadFailed?.Invoke();
            return false;
        }

        if (_system.database == null)
        {
            Debug.LogError("[GridImporter] BuildPieceDatabaseSO is not assigned.");
            _system.OnGridLoadFailed?.Invoke();
            return false;
        }

        if (_system.gridManager?.Grid == null)
        {
            Debug.LogError("[GridImporter] GridManager or Grid is null.");
            _system.OnGridLoadFailed?.Invoke();
            return false;
        }

        return true;
    }

    // -------------------------------------------------------------------------
    // Grid clearing
    // -------------------------------------------------------------------------

    /// <summary>Tracks destroyed pieces to avoid double-destroying multi-cell pieces.</summary>
    private void ClearExistingGrid()
    {
        int width  = _system.gridManager.Grid.Width;
        int height = _system.gridManager.Grid.Height;

        HashSet<GridBuildPiece> destroyed = new();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridCell cell = _system.gridManager.Grid.GetGridObject(x, z);
                if (cell == null) continue;

                GridBuildPiece piece;
                while ((piece = cell.RemoveTopGridBuildPiece()) != null)
                {
                    if (destroyed.Contains(piece)) continue;

                    destroyed.Add(piece);
                    _system.buildingManager.DestroyBuildPiece(piece);
                }
            }
        }
    }

    // -------------------------------------------------------------------------
    // Placement
    // -------------------------------------------------------------------------

    /// <summary>Sorts pieces ascending by stack depth so ground pieces are placed first.</summary>
    private static List<GridBuildPieceData> OrderByStackDepth(List<GridBuildPieceData> pieces)
        => pieces.OrderBy(p => p.stackDepth).ToList();

    private bool PlaceAllPieces(List<GridBuildPieceData> ordered)
    {
        bool allSucceeded = true;

        foreach (GridBuildPieceData pieceData in ordered)
        {
            if (!TryPlacePiece(pieceData))
            {
                Debug.LogWarning($"[GridImporter] Failed to place piece ID={pieceData.pieceId} at {pieceData.originGridPosition}.");
                allSucceeded = false;
            }
        }

        return allSucceeded;
    }

    private bool TryPlacePiece(GridBuildPieceData pieceData)
    {
        BuildPieceData dbEntry = FindInDatabase(pieceData.pieceId);
        if (dbEntry == null)
        {
            Debug.LogWarning($"[GridImporter] No database entry found for pieceId={pieceData.pieceId}.");
            return false;
        }

        // Validate every occupied cell before spawning.
        if (!AllCellsAcceptPlacement(pieceData, dbEntry)) return false;

        // Instantiate via BuildingManager so the piece is parented and initialised correctly.
        Quaternion rotation = Quaternion.Euler(pieceData.rotation);
        GridBuildPiece piece = _system.buildingManager.CreateBuildPiece(dbEntry, pieceData.worldPosition, rotation, "LevelLoad");

        // Register in the grid.
        if (pieceData.occupiedPositions != null && pieceData.occupiedPositions.Count > 0)
            _system.gridManager.AddObjectToGrid(piece, pieceData.occupiedPositions);
        else
            _system.gridManager.AddObjectToGrid(piece, pieceData.originGridPosition, pieceData.sizeOnGrid, pieceData.placementDirection);

        return true;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private BuildPieceData FindInDatabase(int pieceId)
        => _system.database.objectsData.FirstOrDefault(d => d.ID == pieceId);

    private bool AllCellsAcceptPlacement(GridBuildPieceData pieceData, BuildPieceData dbEntry)
    {
        List<Vector2Int> positions = pieceData.occupiedPositions != null && pieceData.occupiedPositions.Count > 0
            ? pieceData.occupiedPositions
            : pieceData.originGridPosition.GetGridPositionList(pieceData.sizeOnGrid, pieceData.placementDirection);

        foreach (Vector2Int pos in positions)
        {
            if (!_system.gridManager.CanBuildOnCell(pos, dbEntry.layer))
            {
                Debug.LogWarning($"[GridImporter] Cell {pos} is not available for pieceId={pieceData.pieceId}.");
                return false;
            }
        }

        return true;
    }
}
```