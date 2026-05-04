using System;
using System.Collections.Generic;
using UnityEngine;

// =============================================================================
#region Export
// =============================================================================

/// <summary>
/// Responsible for reading the current runtime grid state and producing a
/// <see cref="GridLevelDataSO"/> ScriptableObject that can be saved to disk or
/// passed directly to <see cref="GridImporter"/>.
/// </summary>
public class GridExporter
{
    #region Constants

    /// <summary>Prefix applied to every log message emitted by this class.</summary>
    private const string LogPrefix = "[GridExportImportSystem]";

    #endregion

    #region Fields

    /// <summary>Reference to the owning <see cref="GridExportImportSystem"/>.</summary>
    private readonly GridExportImportSystem _system;

    #endregion

    #region Constructor

    /// <summary>
    /// Initialises a new <see cref="GridExporter"/> that reads data through
    /// the supplied <paramref name="system"/> facade.
    /// </summary>
    /// <param name="system">The owning system; must not be <c>null</c>.</param>
    public GridExporter(GridExportImportSystem system)
    {
        _system = system;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Captures the current grid state into a new <see cref="GridLevelDataSO"/> instance.
    /// The asset is <em>not</em> written to disk here; call
    /// <see cref="GridExportImportSystem.SaveToFile"/> separately when running in the editor.
    /// </summary>
    /// <param name="fileName">
    /// Optional name for the level. Defaults to a timestamp-based name when <c>null</c>.
    /// </param>
    /// <returns>
    /// A populated <see cref="GridLevelDataSO"/>, or <c>null</c> if the grid is unavailable.
    /// </returns>
    public GridLevelDataSO ExportGrid(string fileName = null)
    {
        if (_system.gridManager?.Grid == null)
        {
            Debug.LogError($"{LogPrefix} GridManager or Grid is null!");
            return null;
        }

        GridLevelDataSO levelData = ScriptableObject.CreateInstance<GridLevelDataSO>();

        levelData.levelName    = fileName ?? $"GridLevel_{DateTime.Now:yyyyMMdd_HHmmss}";
        levelData.gridSize     = new Vector2Int(_system.gridManager.Grid.Width, _system.gridManager.Grid.Height);
        levelData.cellSize     = _system.gridManager.Grid.CellSize;
        levelData.gridOrigin   = _system.gridManager.transform.position;
        levelData.creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        ExportGridPieces(levelData);

        Debug.Log($"{LogPrefix} Exported {levelData.buildPieces.Count} build pieces to ScriptableObject.");
        _system.OnGridExported?.Invoke(levelData);
        return levelData;
    }

    #endregion

    #region Private – Orchestration

    /// <summary>
    /// Top-level orchestrator for the two-pass export algorithm.
    /// Pass 1 collects unique pieces; pass 2 converts them to serialisable data.
    /// </summary>
    /// <param name="levelData">Target container that will receive the exported pieces.</param>
    private void ExportGridPieces(GridLevelDataSO levelData)
    {
        Dictionary<GridBuildPiece, string> pieceIdByPiece = new Dictionary<GridBuildPiece, string>();
        HashSet<GridBuildPiece> visitedPieces = new HashSet<GridBuildPiece>();

        CollectUniquePieces(levelData, visitedPieces, pieceIdByPiece);
        BuildPieceDataList(levelData, pieceIdByPiece);
    }

    /// <summary>
    /// Pass 1 – walks every grid cell and registers each unique
    /// <see cref="GridBuildPiece"/> together with a freshly generated GUID.
    /// </summary>
    /// <param name="levelData">Provides the grid dimensions used for iteration.</param>
    /// <param name="visitedPieces">Accumulates pieces that have already been seen.</param>
    /// <param name="pieceIdByPiece">Maps each unique piece to its export GUID.</param>
    private void CollectUniquePieces(
        GridLevelDataSO levelData,
        HashSet<GridBuildPiece> visitedPieces,
        Dictionary<GridBuildPiece, string> pieceIdByPiece)
    {
        for (int x = 0; x < levelData.gridSize.x; x++)
        {
            for (int z = 0; z < levelData.gridSize.y; z++)
            {
                GridCell cell = _system.gridManager.Grid.GetGridObject(x, z);
                if (cell != null)
                {
                    CollectAllUniquePiecesFromCell(cell, visitedPieces, pieceIdByPiece);
                }
            }
        }
    }

    /// <summary>
    /// Pass 2 – converts every registered piece in <paramref name="pieceIdByPiece"/>
    /// into a <see cref="GridBuildPieceData"/> record and appends it to
    /// <paramref name="levelData"/>.
    /// </summary>
    /// <param name="levelData">Receives the finished piece-data list.</param>
    /// <param name="pieceIdByPiece">The piece-to-GUID map produced by pass 1.</param>
    private void BuildPieceDataList(
        GridLevelDataSO levelData,
        Dictionary<GridBuildPiece, string> pieceIdByPiece)
    {
        foreach (KeyValuePair<GridBuildPiece, string> entry in pieceIdByPiece)
        {
            GridBuildPiece piece     = entry.Key;
            string         uniqueId  = entry.Value;

            GridBuildPieceData pieceData = new GridBuildPieceData
            {
                uniqueId              = uniqueId,
                pieceId               = piece.PieceData != null ? piece.PieceData.pieceId : -1,
                worldPosition         = piece.transform.position,
                rotation              = piece.transform.eulerAngles,
                sizeOnGrid            = piece.SizeOnGrid,
                canBuildOnTop         = piece.CanBuildOnTop,
                storeThisToGrid       = piece.StoreThisToGrid,
                canBeRemovedFromGrid  = piece.CanBeRemovedFromGrid,
                layer                 = piece.Layer,
                canBeBuiltOnLayers    = piece.CanBeBuiltOnLayers,
                occupiedPositions     = new List<Vector2Int>(piece.OccupiedPositions),
                placementDirection    = piece.PlacementDirection,
                originGridPosition    = piece.OriginGridPosition,
                stackDepth            = piece.StackDepth
            };

            foreach (GridBuildPiece topPiece in piece.ObjectsOnTop)
            {
                if (pieceIdByPiece.TryGetValue(topPiece, out string topId))
                {
                    pieceData.objectsOnTopIds.Add(topId);
                }
            }

            levelData.buildPieces.Add(pieceData);
        }
    }

    /// <summary>
    /// Walks every piece in <paramref name="cell"/> (including stacked pieces) and
    /// adds any not-yet-seen piece to both <paramref name="visitedPieces"/> and
    /// <paramref name="pieceIdByPiece"/>.
    /// </summary>
    /// <param name="cell">The grid cell to inspect.</param>
    /// <param name="visitedPieces">Deduplication set.</param>
    /// <param name="pieceIdByPiece">Accumulates each new piece mapped to a fresh GUID.</param>
    private void CollectAllUniquePiecesFromCell(
        GridCell cell,
        HashSet<GridBuildPiece> visitedPieces,
        Dictionary<GridBuildPiece, string> pieceIdByPiece)
    {
        List<GridBuildPiece> piecesInStack = cell.GetAllPiecesInStack();
        foreach (GridBuildPiece piece in piecesInStack)
        {
            if (piece != null && visitedPieces.Add(piece))
            {
                pieceIdByPiece[piece] = Guid.NewGuid().ToString();
            }
        }
    }

    #endregion
}

#endregion
