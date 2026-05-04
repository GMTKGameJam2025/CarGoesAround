using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single cell in the game grid.
/// Internally maintains a stack of <see cref="GridBuildPiece"/> objects placed on this cell,
/// where the top of the stack is the most recently placed (highest-layer) piece.
/// </summary>
public class GridCell
{
    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>The <see cref="GridXZ{GridCell}"/> that owns this cell.</summary>
    public GridXZ<GridCell> Grid { get; private set; }

    /// <summary>The (column, row) index of this cell within the grid.</summary>
    public Vector2Int Position { get; private set; }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private readonly Stack<GridBuildPiece> _buildPieces = new();

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="grid">The owning grid.</param>
    /// <param name="position">The (column, row) index of this cell.</param>
    public GridCell(GridXZ<GridCell> grid, Vector2Int position)
    {
        Grid     = grid;
        Position = position;
    }

    // -------------------------------------------------------------------------
    // Mutators
    // -------------------------------------------------------------------------

    /// <summary>
    /// Pushes <paramref name="piece"/> onto this cell's stack and registers it as a dependent
    /// of the piece that was previously on top (if any).
    /// </summary>
    public void AddGridBuildPiece(GridBuildPiece piece)
    {
        if (piece == null)
        {
            Debug.LogWarning($"[GridCell] Attempted to add a null GridBuildPiece at {Position}. Skipping.");
            return;
        }

        if (_buildPieces.Count > 0)
        {
            GridBuildPiece topPiece = GetTopGridObject();
            topPiece.AddObjectOnTop(piece);
        }

        _buildPieces.Push(piece);
    }

    /// <summary>
    /// Removes and returns the top <see cref="GridBuildPiece"/> from this cell's stack.
    /// Also un-registers it from the new top piece's dependency list.
    /// Returns <c>null</c> when the cell is empty.
    /// </summary>
    public GridBuildPiece RemoveTopGridBuildPiece()
    {
        if (_buildPieces.Count == 0) return null;

        GridBuildPiece topPiece = _buildPieces.Pop();

        if (_buildPieces.TryPeek(out GridBuildPiece nextTopPiece))
            nextTopPiece.RemoveObjectOnTop(topPiece);

        return topPiece;
    }

    // -------------------------------------------------------------------------
    // Queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns <c>true</c> when the current top piece's layer is compatible with
    /// <paramref name="layer"/> (or the cell is empty).
    /// </summary>
    public bool CompareCurrentTopLayer(BuildLayer layer)
        => _buildPieces.Count == 0 || GridHelper.CanBuildOnLayer(layer, GetTopGridObject().Layer);

    /// <summary>
    /// Returns <c>true</c> when there are no pieces on this cell, or the top piece
    /// explicitly allows building on top of it.
    /// </summary>
    public bool CanBuild()
        => _buildPieces.Count == 0 || GetTopGridObject().CanBuildOnTop;

    /// <summary>
    /// Returns <c>true</c> when the top piece exists, has nothing placed on top of it,
    /// and is flagged as removable.
    /// </summary>
    public bool CanRemove()
    {
        if (_buildPieces.TryPeek(out GridBuildPiece piece))
            return piece.GridObjectsOnTop.Count == 0 && piece.CanBeRemovedFromGrid;

        return false;
    }

    /// <summary>
    /// Returns the top <see cref="GridBuildPiece"/> without removing it,
    /// or <c>null</c> when the cell is empty.
    /// </summary>
    public GridBuildPiece GetTopGridObject()
        => _buildPieces.Count == 0 ? null : _buildPieces.Peek();
}
