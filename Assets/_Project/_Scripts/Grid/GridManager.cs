using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Vector2Int gridSize = new Vector2Int(10, 10);
    [SerializeField] private float cellSize = 1f;
    
    public GridXZ<GridCell> Grid;
    
    public bool showDebugGrid;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Grid = new GridXZ<GridCell>(
            gridSize.x, gridSize.y,
            cellSize,
            transform.position,
            (g, x, y) => new GridCell(g, new Vector2Int(x, y)));
    }

    // Update is called once per frame
    public void AddObjectToGrid(GridBuildPiece obj, Vector2Int originPosition)
    {
        GridCell cell = Grid.GetGridObject(originPosition.x, originPosition.y);
        obj.occupiedPositions.Add(originPosition);
        cell.AddGridBuildPiece(obj);
    }
    public void AddObjectToGrid(GridBuildPiece obj, List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            AddObjectToGrid(obj, position);
        }
    }
    public void AddObjectToGrid(GridBuildPiece obj, Vector2Int originPosition, Vector2Int size, Direction direction = Direction.Down)
    {
        List<Vector2Int> positions = originPosition.GetGridPositionList(size, direction);
        foreach (Vector2Int position in positions)
        {
            AddObjectToGrid(obj, position);
        }
    }

    public GridBuildPiece RemoveObjectFromGrid(Vector2Int originPosition)
    {
        GridCell cell = Grid.GetGridObject(originPosition.x, originPosition.y);
        GridBuildPiece piece = cell.RemoveTopGridBuildPiece();
        
        if (piece != null)
            foreach (Vector2Int pos in piece.occupiedPositions)
            {
                //According to logic, this should only remove the top piece from each cell if there is nothing else placed on top
                if (pos == originPosition) continue;
                GridCell occupiedCell = Grid.GetGridObject(pos.x, pos.y);
                occupiedCell.RemoveTopGridBuildPiece();
            }

        return piece;
    }

    public bool CanBuildOnCell(Vector2Int originPosition, BuildLayer layer)
    {
        if (Grid.IsGridObjectInGrid(originPosition))
            return false;
        
        GridCell cell = Grid.GetGridObject(originPosition.x, originPosition.y);
        return cell.CanBuild() && cell.CompareCurrentTopLayer(layer);
    }
    
    public bool CanBuildOnCell(Vector2Int originPosition, Vector2Int size, BuildLayer layer, Direction direction = Direction.Down)
    {
        List<Vector2Int> positions = originPosition.GetGridPositionList(size, direction);
        return positions.All(
            pos => Grid.IsGridObjectInGrid(pos) && 
            Grid.GetGridObject(pos.x, pos.y).CanBuild() && 
            Grid.GetGridObject(pos.x, pos.y).CompareCurrentTopLayer(layer));
    }

    public bool CanRemoveOnCell(Vector2Int position)
    {
        GridCell cell = Grid.GetGridObject(position.x, position.y);
        return cell != null && cell.CanRemove();
    }

    public GridBuildPiece GetTopLevelObject(Vector2Int position)
    {
        GridCell cell = Grid.GetGridObject(position.x, position.y);
        return cell?.GetTopGridObject();
    }

    private void OnDrawGizmos()
    {
        if (showDebugGrid) {
            Gizmos.color = Color.white;
            for (int x = 0; x < gridSize.x; x++) {
                for (int z = 0; z < gridSize.y; z++) {
                    Vector3 originPosition = new Vector3(x, 0, z) * cellSize + transform.position;
                    Vector3 upPosition = new Vector3(x, 0, z + 1) * cellSize + transform.position;
                    Vector3 rightPosition = new Vector3(x + 1, 0, z) * cellSize + transform.position;
                    Gizmos.DrawLine(originPosition, upPosition);
                    Gizmos.DrawLine(originPosition, rightPosition);
                }
            }
            
            Gizmos.DrawLine(
                new Vector3(0, 0,gridSize.y) * cellSize + transform.position, 
                new Vector3(gridSize.x, 0,gridSize.y) * cellSize + transform.position);
            Gizmos.DrawLine(
                new Vector3(gridSize.x, 0,0) * cellSize + transform.position,  
                new Vector3(gridSize.x, 0,gridSize.y) * cellSize + transform.position);
        }
    }
}

public enum Direction
{
    Down, 
    Left,
    Up,
    Right,
}

public static class GridHelper
{
    public static bool CanBuildOnLayer(BuildLayer baseLayer, BuildLayer newLayer)
    {
        return (baseLayer & newLayer) != 0;
    }
    
    public static bool CanBuildOnLayer(GridBuildPiece basePiece, GridBuildPiece newPiece)
    {
        // Check if the base piece's layer is allowed by the new piece's build-on layers
        return (newPiece.canBeBuiltOnLayers & basePiece.layer) != 0;
    }
    
    public static List<Vector2Int> GetGridPositionList(this Vector2Int startPosition, Vector2Int size, Direction dir) {
        List<Vector2Int> gridPositionList = new();
        switch (dir) {
            default:
            case Direction.Down:
            case Direction.Up:
                for (int x = 0; x < size.x; x++) {
                    for (int y = 0; y < size.y; y++) {
                        gridPositionList.Add(startPosition + new Vector2Int(x, y));
                    }
                }
                break;
            case Direction.Left:
            case Direction.Right:
                for (int x = 0; x < size.y; x++) {
                    for (int y = 0; y < size.x; y++) {
                        gridPositionList.Add(startPosition + new Vector2Int(x, y));
                    }
                }
                break;
        }
        return gridPositionList;
    }
}

public static class DirectionHelper
{
    public static Direction GetNextDirection(this Direction dir) {
        switch (dir) {
            default:
            case Direction.Down:      return Direction.Left;
            case Direction.Left:      return Direction.Up;
            case Direction.Up:        return Direction.Right;
            case Direction.Right:     return Direction.Down;
        }
    }

    
    public static int GetDirectionRotation(this Direction dir) {
        switch (dir){
            default:
            case Direction.Down:     return 0;
            case Direction.Left:     return 90;
            case Direction.Up:       return 180;
            case Direction.Right:    return 270;
        }
    }
    
    public static Vector2Int GetRotationOffset(this Direction dir, Vector2Int size) {
        switch (dir) {
            default:
            case Direction.Down:  return new Vector2Int(0, 0);
            case Direction.Left:  return new Vector2Int(0, size.x);
            case Direction.Up:    return new Vector2Int(size.x, size.y);
            case Direction.Right: return new Vector2Int(size.y, 0);
        }
    }
}