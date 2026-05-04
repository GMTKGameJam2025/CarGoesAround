using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Owns and manages the runtime <see cref="GridXZ{GridCell}"/> instance.
/// Provides high-level helpers for placing, removing, and querying build pieces.
/// </summary>
public class GridManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [SerializeField] private Vector2Int gridSize = new Vector2Int(10, 10);
    [SerializeField] private float cellSize = 1f;

    [Tooltip("Toggle to draw a white wireframe grid in the Scene view via OnDrawGizmos.")]
    public bool showDebugGrid;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>The live grid instance. Populated during <c>Awake</c>.</summary>
    public GridXZ<GridCell> Grid { get; private set; }

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        Grid = new GridXZ<GridCell>(
            gridSize.x, gridSize.y,
            cellSize,
            transform.position,
            (g, x, y) => new GridCell(g, new Vector2Int(x, y)));
    }

    // -------------------------------------------------------------------------
    // Placement
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds <paramref name="obj"/> to the single cell at <paramref name="originPosition"/>
    /// and records that position as occupied by the piece.
    /// </summary>
    public void AddObjectToGrid(GridBuildPiece obj, Vector2Int originPosition)
    {
        GridCell cell = Grid.GetGridObject(originPosition.x, originPosition.y);
        obj.AddOccupiedPosition(originPosition);
        cell.AddGridBuildPiece(obj);
    }

    /// <summary>Adds <paramref name="obj"/> to every cell in <paramref name="positions"/>.</summary>
    public void AddObjectToGrid(GridBuildPiece obj, List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
            AddObjectToGrid(obj, position);
    }

    /// <summary>
    /// Adds <paramref name="obj"/> to all cells covered by an object of <paramref name="size"/>
    /// placed at <paramref name="originPosition"/> facing <paramref name="direction"/>.
    /// </summary>
    public void AddObjectToGrid(GridBuildPiece obj, Vector2Int originPosition, Vector2Int size,
                                Direction direction = Direction.Down)
    {
        foreach (Vector2Int position in originPosition.GetGridPositionList(size, direction))
            AddObjectToGrid(obj, position);
    }

    // -------------------------------------------------------------------------
    // Removal
    // -------------------------------------------------------------------------

    /// <summary>
    /// Removes the top build piece from <paramref name="originPosition"/> and from every
    /// other cell it occupies. Returns the removed piece, or <c>null</c> if the cell was empty.
    /// </summary>
    public GridBuildPiece RemoveObjectFromGrid(Vector2Int originPosition)
    {
        GridCell cell = Grid.GetGridObject(originPosition.x, originPosition.y);
        GridBuildPiece piece = cell.RemoveTopGridBuildPiece();

        if (piece == null) return null;

        foreach (Vector2Int pos in piece.OccupiedPositions)
        {
            if (pos == originPosition) continue;
            Grid.GetGridObject(pos.x, pos.y)?.RemoveTopGridBuildPiece();
        }

        return piece;
    }

    // -------------------------------------------------------------------------
    // Validity queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns <c>true</c> when a single-cell placement at <paramref name="originPosition"/>
    /// is valid for the given <paramref name="layer"/>.
    /// </summary>
    public bool CanBuildOnCell(Vector2Int originPosition, BuildLayer layer)
    {
        if (!Grid.IsInBounds(originPosition)) return false;

        GridCell cell = Grid.GetGridObject(originPosition.x, originPosition.y);
        return cell.CanBuild() && cell.CompareCurrentTopLayer(layer);
    }

    /// <summary>
    /// Returns <c>true</c> when every cell covered by an object of <paramref name="size"/>
    /// placed at <paramref name="originPosition"/> facing <paramref name="direction"/> is
    /// a valid placement for the given <paramref name="layer"/>.
    /// </summary>
    public bool CanBuildOnCell(Vector2Int originPosition, Vector2Int size, BuildLayer layer,
                               Direction direction = Direction.Down)
    {
        List<Vector2Int> positions = originPosition.GetGridPositionList(size, direction);
        return positions.All(pos =>
            Grid.IsInBounds(pos) &&
            Grid.GetGridObject(pos.x, pos.y).CanBuild() &&
            Grid.GetGridObject(pos.x, pos.y).CompareCurrentTopLayer(layer));
    }

    /// <summary>
    /// Returns <c>true</c> when the top piece at <paramref name="position"/> can be removed.
    /// </summary>
    public bool CanRemoveOnCell(Vector2Int position)
    {
        GridCell cell = Grid.GetGridObject(position.x, position.y);
        return cell != null && cell.CanRemove();
    }

    // -------------------------------------------------------------------------
    // Object queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the top-most <see cref="GridBuildPiece"/> at <paramref name="position"/>,
    /// or <c>null</c> if the cell is empty or out of bounds.
    /// </summary>
    public GridBuildPiece GetTopLevelObject(Vector2Int position)
        => Grid.GetGridObject(position.x, position.y)?.GetTopGridObject();

    // -------------------------------------------------------------------------
    // Editor / debug visualisation
    // -------------------------------------------------------------------------

    private void OnDrawGizmos()
    {
        if (!showDebugGrid) return;

        Gizmos.color = Color.white;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int z = 0; z < gridSize.y; z++)
            {
                Vector3 origin = new Vector3(x,     0, z)     * cellSize + transform.position;
                Vector3 up     = new Vector3(x,     0, z + 1) * cellSize + transform.position;
                Vector3 right  = new Vector3(x + 1, 0, z)     * cellSize + transform.position;
                Gizmos.DrawLine(origin, up);
                Gizmos.DrawLine(origin, right);
            }
        }

        // Close the far edges.
        Gizmos.DrawLine(
            new Vector3(0,          0, gridSize.y) * cellSize + transform.position,
            new Vector3(gridSize.x, 0, gridSize.y) * cellSize + transform.position);
        Gizmos.DrawLine(
            new Vector3(gridSize.x, 0, 0)          * cellSize + transform.position,
            new Vector3(gridSize.x, 0, gridSize.y) * cellSize + transform.position);
    }
}
