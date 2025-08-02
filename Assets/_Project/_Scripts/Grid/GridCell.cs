using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
    public GridXZ<GridCell> Grid;
    public Vector2Int Position;
    private Stack<GridBuildPiece> _buildPieces;

    public GridCell(GridXZ<GridCell> grid, Vector2Int position)
    {
        this.Grid = grid;
        this.Position = position;
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
    
    public GridBuildPiece RemoveTopGridBuildPiece()
    {
        if (_buildPieces.Count <= 0)
        {
            return null;
        }
        
        GridBuildPiece topPiece = _buildPieces.Pop();

        if (_buildPieces.TryPeek(out GridBuildPiece nextTopPiece))
        {
            nextTopPiece.gridObjectsOnTop.Remove(topPiece);
        } 
        
        return topPiece;
    }

    public bool CompareCurrentTopLayer(BuildLayer layer)
    {
        return _buildPieces.Count <= 0 || GridHelper.CanBuildOnLayer(layer, GetTopGridObject().layer);
    }
    
    public bool CanBuild()
    {
        return _buildPieces.Count <= 0 || GetTopGridObject().canBuildOnTop;
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