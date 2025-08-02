using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
    public GridXZ<GridCell> grid;
    public Vector2Int position;
    private Stack<GridBuildPiece> _buildPieces;

    public GridCell(GridXZ<GridCell> grid, Vector2Int position)
    {
        this.grid = grid;
        this.position = position;
        _buildPieces = new Stack<GridBuildPiece>();
    }

    public void AddGridBuildPiece(GridBuildPiece piece)
    {
        if (_buildPieces.Count > 0)
        {
            GridBuildPiece topPiece = GetTopGridObject();
            topPiece.gridObjectsOnTop.Add(piece);
        }
        
        _buildPieces.Push(piece);
    }
    
    public bool CanBuild()
    {
        return _buildPieces.Count <= 0 || _buildPieces.Peek().canBuildOnTop;
    }

    public bool CanRemove()
    {
        if (_buildPieces.TryPeek(out GridBuildPiece piece))
        {
            return piece.gridObjectsOnTop.Count <= 0;
        }

        return false;
    }

    public GridBuildPiece GetTopGridObject()
    {
        return _buildPieces.Count <= 0 ? null : _buildPieces.Peek();
    }
}