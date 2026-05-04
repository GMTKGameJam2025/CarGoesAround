using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// =============================================================================
#region Import
// =============================================================================

/// <summary>
/// Responsible for reading a <see cref="GridLevelDataSO"/> and recreating its
/// build pieces at runtime on the live grid managed by
/// <see cref="GridExportImportSystem"/>.
/// </summary>
public class GridImporter
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
    /// Initialises a new <see cref="GridImporter"/> that restores data through
    /// the supplied <paramref name="system"/> facade.
    /// </summary>
    /// <param name="system">The owning system; must not be <c>null</c>.</param>
    public GridImporter(GridExportImportSystem system)
    {
        _system = system;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Clears the current grid and asynchronously spawns all pieces described in
    /// <paramref name="levelData"/>.
    /// </summary>
    /// <param name="levelData">The level data to restore; must not be <c>null</c>.</param>
    /// <returns>An <see cref="IEnumerator"/> suitable for use with <see cref="MonoBehaviour.StartCoroutine"/>.</returns>
    public IEnumerator ImportGrid(GridLevelDataSO levelData)
    {
        if (levelData == null)
        {
            Debug.LogError($"{LogPrefix} LevelData is null!");
            yield break;
        }

        if (_system.gridManager?.Grid == null)
        {
            Debug.LogError($"{LogPrefix} GridManager or Grid is null!");
            yield break;
        }

        ClearGrid();
        yield return null;

        List<GridBuildPiece> spawnedPieces = new List<GridBuildPiece>();

        foreach (GridBuildPieceData pieceData in levelData.buildPieces)
        {
            GridBuildPiece spawnedPiece = SpawnPiece(pieceData);
            if (spawnedPiece != null)
            {
                spawnedPieces.Add(spawnedPiece);
            }
        }

        yield return null;
        RestoreObjectsOnTop(levelData, spawnedPieces);

        Debug.Log($"{LogPrefix} Imported {spawnedPieces.Count} build pieces from ScriptableObject.");
        _system.OnGridImported?.Invoke(levelData);
    }

    #endregion

    #region Private – Helpers

    /// <summary>
    /// Destroys all existing <see cref="GridBuildPiece"/> children of
    /// <see cref="GridExportImportSystem.buildPiecesParent"/> and resets the grid.
    /// </summary>
    private void ClearGrid()
    {
        if (_system.buildPiecesParent != null)
        {
            foreach (Transform child in _system.buildPiecesParent)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        _system.gridManager.Grid.ClearGrid();
    }

    /// <summary>
    /// Instantiates the prefab for <paramref name="pieceData"/> and positions it on
    /// the grid according to the saved state.
    /// </summary>
    /// <param name="pieceData">Serialised state of the piece to spawn.</param>
    /// <returns>
    /// The newly created <see cref="GridBuildPiece"/>, or <c>null</c> if the
    /// corresponding prefab could not be found.
    /// </returns>
    private GridBuildPiece SpawnPiece(GridBuildPieceData pieceData)
    {
        BuildPieceData buildPieceData = _system.buildPieceDatabase.GetBuildPieceById(pieceData.pieceId);
        if (buildPieceData == null)
        {
            Debug.LogWarning($"{LogPrefix} Could not find BuildPieceData for pieceId {pieceData.pieceId}");
            return null;
        }

        GameObject prefab = buildPieceData.prefab;
        if (prefab == null)
        {
            Debug.LogWarning($"{LogPrefix} Prefab is null for pieceId {pieceData.pieceId}");
            return null;
        }

        GameObject go = UnityEngine.Object.Instantiate(
            prefab,
            pieceData.worldPosition,
            Quaternion.Euler(pieceData.rotation),
            _system.buildPiecesParent);

        GridBuildPiece piece = go.GetComponent<GridBuildPiece>();
        if (piece == null)
        {
            Debug.LogWarning($"{LogPrefix} Spawned object has no GridBuildPiece component.");
            UnityEngine.Object.Destroy(go);
            return null;
        }

        piece.UniqueId             = pieceData.uniqueId;
        piece.SizeOnGrid           = pieceData.sizeOnGrid;
        piece.CanBuildOnTop        = pieceData.canBuildOnTop;
        piece.StoreThisToGrid      = pieceData.storeThisToGrid;
        piece.CanBeRemovedFromGrid = pieceData.canBeRemovedFromGrid;
        piece.Layer                = pieceData.layer;
        piece.CanBeBuiltOnLayers   = pieceData.canBeBuiltOnLayers;
        piece.PlacementDirection   = pieceData.placementDirection;
        piece.OriginGridPosition   = pieceData.originGridPosition;
        piece.StackDepth           = pieceData.stackDepth;
        piece.OccupiedPositions    = new List<Vector2Int>(pieceData.occupiedPositions);

        if (pieceData.storeThisToGrid)
        {
            foreach (Vector2Int pos in pieceData.occupiedPositions)
            {
                GridCell cell = _system.gridManager.Grid.GetGridObject(pos.x, pos.y);
                cell?.AddPieceToStack(piece, pieceData.stackDepth);
            }
        }

        return piece;
    }

    /// <summary>
    /// Second pass: links each spawned piece's <c>ObjectsOnTop</c> list by
    /// resolving the stored unique-ID references.
    /// </summary>
    /// <param name="levelData">Source data containing the ID lists.</param>
    /// <param name="spawnedPieces">All pieces that were successfully spawned in the first pass.</param>
    private void RestoreObjectsOnTop(GridLevelDataSO levelData, List<GridBuildPiece> spawnedPieces)
    {
        foreach (GridBuildPieceData pieceData in levelData.buildPieces)
        {
            GridBuildPiece piece = FindPieceByUniqueId(spawnedPieces, pieceData.uniqueId);
            if (piece == null) continue;

            piece.ObjectsOnTop = new List<GridBuildPiece>();
            foreach (string topId in pieceData.objectsOnTopIds)
            {
                GridBuildPiece topPiece = FindPieceByUniqueId(spawnedPieces, topId);
                if (topPiece != null)
                {
                    piece.ObjectsOnTop.Add(topPiece);
                }
            }
        }
    }

    /// <summary>
    /// Searches <paramref name="pieces"/> for the first entry whose
    /// <see cref="GridBuildPiece.UniqueId"/> matches <paramref name="uniqueId"/>.
    /// </summary>
    /// <param name="pieces">The pool to search.</param>
    /// <param name="uniqueId">The ID to match.</param>
    /// <returns>The matching piece, or <c>null</c> if not found.</returns>
    private GridBuildPiece FindPieceByUniqueId(List<GridBuildPiece> pieces, string uniqueId)
    {
        return pieces.FirstOrDefault(p => p.UniqueId == uniqueId);
    }

    #endregion
}

#endregion
