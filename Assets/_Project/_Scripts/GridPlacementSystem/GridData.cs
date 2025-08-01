using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridData : MonoBehaviour
{
    [SerializeField] private Vector2Int gridRadius = new(5, 5);
    [SerializeField] private int cellSize = 1;
    
    [SerializeField] private Grid grid;
    [SerializeField] private Transform gridVisualization;
        
    private Dictionary<Vector3Int, PlacementData> _placedObjects;

    private void Awake()
    {
        _placedObjects = new();
        ConstructGrid();
    }

    private void ConstructGrid()
    {
        if (grid)
        {
            grid.cellSize = new Vector3(cellSize, cellSize, cellSize);
        }

        if (gridVisualization)
        {
            gridVisualization.localScale = new Vector3(gridRadius.x * 2 / 10f, 1, gridRadius.y * 2 / 10f);
        }
    }
    
    public void AddObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int id, int placedObjectIndex)
    {
        List<Vector3Int> positionToOccupy = GetOccupiedPosition(gridPosition, objectSize);
        PlacementData data = new PlacementData(positionToOccupy, id, placedObjectIndex);
        foreach (var pos in positionToOccupy)
        {
            if (_placedObjects.ContainsKey(pos))
                throw new Exception($"Position {pos} is already occupied");
            
            _placedObjects[pos] = data;
        }
    }

    private List<Vector3Int> GetOccupiedPosition(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> returnVal = new();
        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                returnVal.Add(gridPosition + new Vector3Int(x, y, 0));
            }
        }
        return returnVal;
    }

    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> positionToOccupy = GetOccupiedPosition(gridPosition, objectSize);
        return positionToOccupy.All(pos =>
        {
            bool inGrid = pos.x < gridRadius.x && pos.y < gridRadius.y  && pos.x >= -gridRadius.x && pos.y >= -gridRadius.y;
            bool occupied = _placedObjects.ContainsKey(pos);

            return inGrid && !occupied;
        });
    }

    public int GetRepresentationIndex(Vector3Int gridPosition)
    {
        if (_placedObjects.TryGetValue(gridPosition, out PlacementData o))
            return o.PlacedObjectIndex;
        return -1;
    }

    public void RemoveObjectAt(Vector3Int gridPosition)
    {
        PlacementData data = _placedObjects[gridPosition];
        foreach (var pos in data.OccupiedPositions)
        {
            _placedObjects.Remove(pos);
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        ConstructGrid();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            for (int x = -gridRadius.x; x <= gridRadius.x - 1; x++)
            {
                for (int y = -gridRadius.y; y <= gridRadius.y - 1; y++)
                {
                    if (_placedObjects.ContainsKey(new Vector3Int(x, y, 0)))
                    {
                        Vector3 center = new Vector3(x + 0.5f, 1, y + 0.5f);
                        Gizmos.DrawSphere(center, 0.25f);
                    }
                }
            }
        }
    }
#endif
}

public class PlacementData
{
    public readonly List<Vector3Int> OccupiedPositions;
    public int ID { get; private set; }
    public int PlacedObjectIndex { get; private set; }

    public PlacementData(List<Vector3Int> occupiedPositions, int id, int placedObjectIndex)
    {
        this.OccupiedPositions = occupiedPositions;
        this.ID = id;
        this.PlacedObjectIndex = placedObjectIndex;
    }

}