using System.Collections.Generic;
using UnityEngine;

// =============================================================================
#region Data Transfer Object
// =============================================================================

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

    /// <summary>Footprint of the piece expressed in grid cells (width x depth).</summary>
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
