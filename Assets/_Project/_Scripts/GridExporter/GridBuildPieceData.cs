using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serialisable snapshot of a single <see cref="GridBuildPiece"/> stored inside a
/// <see cref="GridLevelDataSO"/>. All fields are plain value-types or primitives so
/// Unity's asset serialiser can round-trip them without extra work.
/// </summary>
[System.Serializable]
public class GridBuildPieceData
{
    // -------------------------------------------------------------------------
    // Identity
    // -------------------------------------------------------------------------

    /// <summary>Unique string ID assigned at export time (GUID).</summary>
    public string uniqueId;

    /// <summary>Numeric ID that maps back to a <see cref="BuildPieceData"/> in the database.</summary>
    public int pieceId;

    // -------------------------------------------------------------------------
    // Transform
    // -------------------------------------------------------------------------

    /// <summary>World-space position at the time of export.</summary>
    public Vector3 worldPosition;

    /// <summary>Euler angles at the time of export.</summary>
    public Vector3 rotation;

    /// <summary>Grid-space origin cell (column, row).</summary>
    public Vector2Int originGridPosition;

    /// <summary>Direction the piece was facing when placed.</summary>
    public Direction placementDirection;

    // -------------------------------------------------------------------------
    // Grid footprint
    // -------------------------------------------------------------------------

    /// <summary>Footprint of this piece in grid cells (columns x rows).</summary>
    public Vector2Int sizeOnGrid;

    /// <summary>Every grid cell occupied by this piece.</summary>
    public List<Vector2Int> occupiedPositions = new List<Vector2Int>();

    // -------------------------------------------------------------------------
    // Build rules  (mirrored from BuildPieceData so the level is self-contained)
    // -------------------------------------------------------------------------

    public bool canBuildOnTop;
    public bool storeThisToGrid;
    public bool canBeRemovedFromGrid;
    public BuildLayer layer;
    public BuildLayer canBeBuiltOnLayers;

    // -------------------------------------------------------------------------
    // Stack relationships
    // -------------------------------------------------------------------------

    /// <summary>
    /// Unique IDs of pieces that sit directly on top of this one.
    /// Used during import to reconstruct stacking order.
    /// </summary>
    public List<string> objectsOnTopIds = new List<string>();

    /// <summary>
    /// Depth in the vertical stack (0 = ground level).
    /// Pieces are imported bottom-up so dependencies are always placed before dependants.
    /// </summary>
    public int stackDepth;
}

/// <summary>
/// Minimal legacy container used only when converting old JSON exports.
/// Not used at runtime — see <see cref="GridLevelConverter"/>.
/// </summary>
[System.Serializable]
public class GridExportDataLegacy
{
    public Vector2Int gridSize;
    public float cellSize;
    public Vector3 gridOrigin;
    public List<GridBuildPieceData> buildPieces = new List<GridBuildPieceData>();
}
