using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridData
{
    private readonly Dictionary<Vector3Int, PlacementData> _placedObjects = new();

    public void AddObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int id, int placedObjectIndex)
    {
        List<Vector3Int> positionToOccupy = CalculatePosition(gridPosition, objectSize);
        PlacementData data = new PlacementData(positionToOccupy, id, placedObjectIndex);
        foreach (var pos in positionToOccupy)
        {
            if (!_placedObjects.TryAdd(pos, data))
                throw new Exception($"Position {pos} is already occupied");

        }
    }

    private List<Vector3Int> CalculatePosition(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> returnVal = new();
        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                returnVal.Add(gridPosition + new Vector3Int(x, 0, y));
            }
        }
        return returnVal;
    }

    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> positionToOccupy = CalculatePosition(gridPosition, objectSize);
        return positionToOccupy.All(pos => !_placedObjects.ContainsKey(pos));
    }

    internal int GetRepresentationIndex(Vector3Int gridPosition)
    {
        if (_placedObjects.TryGetValue(gridPosition, out PlacementData o))
            return o.PlacedObjectIndex;
        return -1;
    }

    internal void RemoveObjectAt(Vector3Int gridPosition)
    {
        foreach (var pos in _placedObjects[gridPosition].OccupiedPositions)
        {
            _placedObjects.Remove(pos);
        }
    }
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