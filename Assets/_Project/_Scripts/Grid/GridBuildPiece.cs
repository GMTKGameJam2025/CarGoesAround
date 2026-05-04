using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// MonoBehaviour component that represents a piece placed (or placeable) on a <see cref="GridXZ{GridCell}"/>.
/// Holds all runtime state for the piece and exposes read-only views of its grid relationships.
/// </summary>
public class GridBuildPiece : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Piece Data")]
    [Tooltip("Database ID — set automatically by Init().")]
    public int id;

    [Header("Properties")]
    [Tooltip("Footprint of this piece on the grid.")]
    public Vector2Int sizeOnGrid = Vector2Int.one;

    [Tooltip("When true, other pieces may be placed on top of this one.")]
    public bool CanBuildOnTop;

    [Tooltip("When false, this piece is not tracked in the grid data structure (cosmetic-only pieces).")]
    public bool storeThisToGrid = true;

    [Tooltip("When false, this piece cannot be removed by the player.")]
    public bool CanBeRemovedFromGrid = true;

    [Header("Build Layering")]
    [Tooltip("The layer this piece occupies once placed.")]
    public BuildLayer Layer = BuildLayer.Ground;

    [Tooltip("Which layers this piece is allowed to be placed on top of.")]
    public BuildLayer CanBeBuiltOnLayers = BuildLayer.Ground;

    // -------------------------------------------------------------------------
    // Runtime state (read-only public views)
    // -------------------------------------------------------------------------

    /// <summary>Read-only view of the pieces that are currently stacked on top of this one.</summary>
    public IReadOnlyList<GridBuildPiece> GridObjectsOnTop => _gridObjectsOnTop;

    /// <summary>Read-only view of every grid cell this piece occupies.</summary>
    public IReadOnlyList<Vector2Int> OccupiedPositions => _occupiedPositions;

    private List<GridBuildPiece> _gridObjectsOnTop  = new();
    private List<Vector2Int>     _occupiedPositions = new();

    /// <summary>Identifies which system or source triggered the build (e.g. "LevelLoad", "Player").</summary>
    public string builtSource;

    // -------------------------------------------------------------------------
    // Internal mutation helpers (called by GridManager / GridCell)
    // -------------------------------------------------------------------------

    /// <summary>Registers <paramref name="piece"/> as sitting on top of this piece.</summary>
    internal void AddObjectOnTop(GridBuildPiece piece)    => _gridObjectsOnTop.Add(piece);

    /// <summary>Un-registers <paramref name="piece"/> from the on-top list.</summary>
    internal void RemoveObjectOnTop(GridBuildPiece piece) => _gridObjectsOnTop.Remove(piece);

    /// <summary>Records that this piece occupies <paramref name="position"/> on the grid.</summary>
    internal void AddOccupiedPosition(Vector2Int position) => _occupiedPositions.Add(position);

    // -------------------------------------------------------------------------
    // Initialisation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Seeds this component from a <see cref="BuildPieceData"/> definition.
    /// Safe to call on a freshly-instantiated or already-initialised piece —
    /// the occupied-position and on-top lists are always reset.
    /// </summary>
    /// <param name="data">The database entry to read from.</param>
    /// <param name="source">Optional tag identifying who triggered the build.</param>
    public void Init(BuildPieceData data, string source = "")
    {
        if (data == null)
        {
            Debug.LogError("[GridBuildPiece] BuildPieceData is null — cannot initialise.");
            return;
        }

        id                    = data.ID;
        sizeOnGrid            = data.Size;
        CanBuildOnTop         = data.canBuildOnTop;
        storeThisToGrid       = data.storeThisToGrid;
        CanBeRemovedFromGrid  = data.canBeRemovedFromGrid;
        Layer                 = data.layer;
        CanBeBuiltOnLayers    = data.canBeBuiltOnLayers;
        builtSource           = source;

        // Always reset lists so re-initialisation is safe.
        _gridObjectsOnTop  = new List<GridBuildPiece>();
        _occupiedPositions = new List<Vector2Int>();

        OnBuild();
    }

    /// <summary>
    /// Calls <see cref="IBuildable.OnBuild"/> on every <see cref="IBuildable"/> component
    /// attached to this GameObject.
    /// </summary>
    public void OnBuild()
    {
        foreach (IBuildable buildable in GetComponents<IBuildable>())
            buildable.OnBuild(this);
    }
}
