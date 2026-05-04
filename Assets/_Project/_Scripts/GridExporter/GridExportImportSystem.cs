```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// =============================================================================
// #region Data Transfer Object
// =============================================================================

#region Data Transfer Object

/// <summary>
/// Serializable data container representing a single build piece as it exists on the grid.
/// Used for both export (capturing runtime state) and import (restoring saved state).
/// </summary>
[System.Serializable]
public class GridBuildPieceData
{
    /// <summary>Globally unique identifier assigned at export time.</summary>
    public string uniqueId;

    /// <summary>Numeric identifier that maps this piece to a <see cref="BuildPieceData"/> entry in the database.</summary>
    public int pieceId;

    /// <summary>World-space position of the piece's transform.</summary>
    public Vector3 worldPosition;

    /// <summary>Euler angles of the piece's transform at the time of export.</summary>
    public Vector3 rotation;

    /// <summary>Footprint of the piece expressed in grid cells (width × depth).</summary>
    public Vector2Int sizeOnGrid;

    /// <summary>Whether other pieces may be stacked on top of this one.</summary>
    public bool canBuildOnTop;

    /// <summary>Whether this piece should be written back into the <see cref="GridCell"/> stack.</summary>
    public bool storeThisToGrid;

    /// <summary>Whether the player is allowed to remove this piece from the grid.</summary>
    public bool canBeRemovedFromGrid;

    /// <summary>The <see cref="BuildLayer"/> this piece belongs to.</summary>
    public BuildLayer layer;

    /// <summary>The bitmask of layers on which this piece is allowed to be placed.</summary>
    public BuildLayer canBeBuiltOnLayers;

    /// <summary>All grid positions occupied by this piece.</summary>
    public List<Vector2Int> occupiedPositions = new List<Vector2Int>();

    /// <summary>Unique IDs of every <see cref="GridBuildPiece"/> that sits directly on top of this piece.</summary>
    public List<string> objectsOnTopIds = new List<string>();

    /// <summary>Placement direction used when the piece was originally added to the grid.</summary>
    public Direction placementDirection;

    /// <summary>Grid coordinate of the piece's origin cell (index 0 of <see cref="occupiedPositions"/>).</summary>
    public Vector2Int originGridPosition;

    /// <summary>
    /// How deep in the vertical stack this piece sits.
    /// A value of 0 means the piece rests directly on the ground level.
    /// </summary>
    public int stackDepth;
}

#endregion

// =============================================================================
// #region Export
// =============================================================================

#region Export

/// <summary>
/// Responsible for reading the current runtime grid state and producing a
/// <see cref="GridLevelDataSO"/> ScriptableObject that can be saved to disk or
/// passed directly to <see cref="GridImporter"/>.
/// </summary>
public class GridExporter
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>Prefix applied to every log message emitted by this class.</summary>
    private const string LogPrefix = "[GridExportImportSystem]";

    // -------------------------------------------------------------------------
    // Fields
    // -------------------------------------------------------------------------

    /// <summary>Reference to the owning <see cref="GridExportImportSystem"/>.</summary>
    private readonly GridExportImportSystem _system;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialises a new <see cref="GridExporter"/> that reads data through
    /// the supplied <paramref name="system"/> facade.
    /// </summary>
    /// <param name="system">The owning system; must not be <c>null</c>.</param>
    public GridExporter(GridExportImportSystem system)
    {
        _system = system;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Captures the current grid state into a new <see cref="GridLevelDataSO"/> instance.
    /// The asset is <em>not</em> written to disk here; call
    /// <see cref="SaveToFile"/> separately when running in the editor.
    /// </summary>
    /// <param name="fileName">
    /// Optional name for the level.  Defaults to a timestamp-based name when <c>null</c>.
    /// </param>
    /// <returns>
    /// A populated <see cref="GridLevelDataSO"/>, or <c>null</c> if the grid is
    /// unavailable.
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

    // -------------------------------------------------------------------------
    // Private – orchestration
    // -------------------------------------------------------------------------

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
    /// Pass 2 – converts every entry in <paramref name="pieceIdByPiece"/> into a
    /// <see cref="GridBuildPieceData"/> and appends it to <paramref name="levelData"/>.
    /// </summary>
    /// <param name="levelData">Target container that will receive the built data list.</param>
    /// <param name="pieceIdByPiece">Source map populated during pass 1.</param>
    private void BuildPieceDataList(
        GridLevelDataSO levelData,
        Dictionary<GridBuildPiece, string> pieceIdByPiece)
    {
        foreach (var entry in pieceIdByPiece)
        {
            GridBuildPiece piece    = entry.Key;
            string         uniqueId = entry.Value;

            GridBuildPieceData pieceData = CreatePieceData(piece, uniqueId, pieceIdByPiece);
            levelData.buildPieces.Add(pieceData);
        }
    }

    // -------------------------------------------------------------------------
    // Private – cell-level helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Inspects all stack levels within <paramref name="cell"/> and registers any
    /// pieces that have not yet been seen in <paramref name="visitedPieces"/>.
    /// </summary>
    /// <param name="cell">The grid cell to inspect.</param>
    /// <param name="visitedPieces">Set of already-registered pieces.</param>
    /// <param name="pieceIdByPiece">Map receiving newly discovered pieces and their IDs.</param>
    private void CollectAllUniquePiecesFromCell(
        GridCell cell,
        HashSet<GridBuildPiece> visitedPieces,
        Dictionary<GridBuildPiece, string> pieceIdByPiece)
    {
        List<GridBuildPiece> piecesInStack = GetAllPiecesInStackNonDestructive(cell);

        foreach (GridBuildPiece piece in piecesInStack)
        {
            if (!visitedPieces.Contains(piece))
            {
                visitedPieces.Add(piece);
                pieceIdByPiece[piece] = Guid.NewGuid().ToString();
            }
        }
    }

    /// <summary>
    /// Returns all pieces in <paramref name="cell"/>'s stack ordered from bottom to top,
    /// leaving the stack in its original state afterwards.
    /// </summary>
    /// <param name="cell">The cell whose stack will be read.</param>
    /// <returns>A new list containing pieces in bottom-to-top order.</returns>
    private List<GridBuildPiece> GetAllPiecesInStackNonDestructive(GridCell cell)
    {
        List<GridBuildPiece>  pieces    = new List<GridBuildPiece>();
        Stack<GridBuildPiece> tempStack = new Stack<GridBuildPiece>();

        while (cell.GetTopGridObject() != null)
        {
            GridBuildPiece piece = cell.RemoveTopGridBuildPiece();
            if (piece != null)
            {
                tempStack.Push(piece);
            }
        }

        while (tempStack.Count > 0)
        {
            GridBuildPiece piece = tempStack.Pop();
            pieces.Add(piece);
            cell.AddGridBuildPiece(piece);
        }

        return pieces;
    }

    // -------------------------------------------------------------------------
    // Private – piece data creation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Constructs a fully populated <see cref="GridBuildPieceData"/> from a live
    /// <see cref="GridBuildPiece"/> instance.
    /// </summary>
    /// <param name="piece">The runtime piece to serialise.</param>
    /// <param name="uniqueId">Pre-generated GUID for this piece.</param>
    /// <param name="pieceIdByPiece">Used to resolve references to on-top neighbours.</param>
    /// <returns>A new <see cref="GridBuildPieceData"/> ready for storage.</returns>
    private GridBuildPieceData CreatePieceData(
        GridBuildPiece piece,
        string uniqueId,
        Dictionary<GridBuildPiece, string> pieceIdByPiece)
    {
        GridBuildPieceData data = new GridBuildPieceData
        {
            uniqueId              = uniqueId,
            pieceId               = piece.id,
            worldPosition         = piece.transform.position,
            rotation              = piece.transform.eulerAngles,
            sizeOnGrid            = piece.sizeOnGrid,
            canBuildOnTop         = piece.CanBuildOnTop,
            storeThisToGrid       = piece.storeThisToGrid,
            canBeRemovedFromGrid  = piece.CanBeRemovedFromGrid,
            layer                 = piece.Layer,
            canBeBuiltOnLayers    = piece.CanBeBuiltOnLayers,
            occupiedPositions     = new List<Vector2Int>(piece.OccupiedPositions),
            stackDepth            = CalculateStackDepth(piece)
        };

        if (piece.GridObjectsOnTop != null)
        {
            foreach (GridBuildPiece topPiece in piece.GridObjectsOnTop)
            {
                if (pieceIdByPiece.ContainsKey(topPiece))
                {
                    data.objectsOnTopIds.Add(pieceIdByPiece[topPiece]);
                }
            }
        }

        if (data.occupiedPositions.Count > 0)
        {
            data.originGridPosition = data.occupiedPositions[0];
            data.placementDirection = CalculateDirection(data.occupiedPositions, data.sizeOnGrid);
        }

        return data;
    }

    // -------------------------------------------------------------------------
    // Private – stack-depth helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determines how many pieces lie below <paramref name="piece"/> by querying
    /// the grid manager for each occupied position.
    /// </summary>
    /// <param name="piece">The piece whose depth should be calculated.</param>
    /// <returns>Zero-based depth index (0 = ground level).</returns>
    private int CalculateStackDepth(GridBuildPiece piece)
    {
        int maxDepth = 0;

        foreach (Vector2Int pos in piece.OccupiedPositions)
        {
            if (_system.gridManager.Grid.IsGridObjectInGrid(pos))
            {
                GridBuildPiece topPiece = _system.gridManager.GetTopLevelObject(pos);
                if (topPiece != null)
                {
                    int depth = GetDepthInStackUsingGridManager(pos, piece);
                    maxDepth = Mathf.Max(maxDepth, depth);
                }
            }
        }

        return maxDepth;
    }

    /// <summary>
    /// Returns the zero-based index of <paramref name="targetPiece"/> within
    /// the stack at <paramref name="position"/> (0 = bottom of stack).
    /// </summary>
    /// <param name="position">Grid coordinate of the cell to inspect.</param>
    /// <param name="targetPiece">The piece whose index should be found.</param>
    /// <returns>The depth index, or 0 if the piece is not found.</returns>
    private int GetDepthInStackUsingGridManager(Vector2Int position, GridBuildPiece targetPiece)
    {
        GridCell cell = _system.gridManager.Grid.GetGridObject(position.x, position.y);
        if (cell == null) return 0;

        List<GridBuildPiece> piecesInStack = GetAllPiecesInStackNonDestructive(cell);

        for (int i = 0; i < piecesInStack.Count; i++)
        {
            if (piecesInStack[i] == targetPiece)
            {
                return i;
            }
        }

        return 0;
    }

    // -------------------------------------------------------------------------
    // Private