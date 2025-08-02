using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class GridExportData
{
    public List<GridBuildPieceData> buildPieces = new List<GridBuildPieceData>();
    public Vector2Int gridSize;
    public float cellSize;
    public Vector3 gridOrigin;
}

[System.Serializable]
public class GridBuildPieceData
{
    public string uniqueId; // Generated unique ID for this piece
    public int pieceId; // The original ID from BuildPieceData
    public Vector3 worldPosition;
    public Vector3 rotation;
    public Vector2Int sizeOnGrid;
    public bool canBuildOnTop;
    public bool storeThisToGrid;
    public bool canBeRemovedFromGrid;
    public BuildLayer layer;
    public BuildLayer canBeBuiltOnLayers;
    public List<Vector2Int> occupiedPositions = new List<Vector2Int>();
    public List<string> objectsOnTopIds = new List<string>(); // References to other pieces on top
    
    // Additional data for reconstruction
    public Direction placementDirection;
    public Vector2Int originGridPosition;
}

public class GridExporter : MonoBehaviour
{
    public GridManager gridManager;
    public BuildPieceDatabaseSO database;
    
    public GridExportData ExportGrid()
    {
        if (gridManager?.Grid == null)
        {
            Debug.LogError("GridManager or Grid is null!");
            return null;
        }

        GridExportData exportData = new GridExportData
        {
            gridSize = new Vector2Int(gridManager.Grid.GetWidth(), gridManager.Grid.GetHeight()),
            cellSize = gridManager.Grid.GetCellSize(),
            gridOrigin = gridManager.transform.position
        };

        // Dictionary to map GridBuildPiece instances to their unique IDs
        Dictionary<GridBuildPiece, string> pieceToIdMap = new Dictionary<GridBuildPiece, string>();
        HashSet<GridBuildPiece> processedPieces = new HashSet<GridBuildPiece>();

        // First pass: Collect all unique build pieces and assign IDs
        for (int x = 0; x < exportData.gridSize.x; x++)
        {
            for (int z = 0; z < exportData.gridSize.y; z++)
            {
                GridCell cell = gridManager.Grid.GetGridObject(x, z);
                if (cell != null)
                {
                    GridBuildPiece topPiece = cell.GetTopGridObject();
                    if (topPiece != null && !processedPieces.Contains(topPiece))
                    {
                        CollectAllPiecesInStack(cell, processedPieces, pieceToIdMap);
                    }
                }
            }
        }

        // Second pass: Create export data for each piece
        foreach (var kvp in pieceToIdMap)
        {
            GridBuildPiece piece = kvp.Key;
            string uniqueId = kvp.Value;

            GridBuildPieceData pieceData = CreatePieceData(piece, uniqueId, pieceToIdMap);
            exportData.buildPieces.Add(pieceData);
        }

        Debug.Log($"Exported {exportData.buildPieces.Count} build pieces from grid.");
        return exportData;
    }

    private void CollectAllPiecesInStack(GridCell cell, HashSet<GridBuildPiece> processedPieces, Dictionary<GridBuildPiece, string> pieceToIdMap)
    {
        // We need to access the internal stack to get all pieces in the cell
        // Since the stack is private, we'll use the available methods to reconstruct the stack
        List<GridBuildPiece> stackPieces = new List<GridBuildPiece>();
        
        // Get all pieces by temporarily removing them and putting them back
        Stack<GridBuildPiece> tempStack = new Stack<GridBuildPiece>();
        
        // Remove all pieces from the cell
        while (cell.GetTopGridObject() != null)
        {
            GridBuildPiece piece = cell.RemoveTopGridBuildPiece();
            if (piece != null)
            {
                tempStack.Push(piece);
                stackPieces.Add(piece);
            }
        }
        
        // Put them back in reverse order to maintain the original stack
        while (tempStack.Count > 0)
        {
            GridBuildPiece piece = tempStack.Pop();
            cell.AddGridBuildPiece(piece);
        }

        // Assign unique IDs to all pieces in the stack
        foreach (GridBuildPiece piece in stackPieces)
        {
            if (!processedPieces.Contains(piece))
            {
                processedPieces.Add(piece);
                pieceToIdMap[piece] = System.Guid.NewGuid().ToString();
            }
        }
    }

    private GridBuildPieceData CreatePieceData(GridBuildPiece piece, string uniqueId, Dictionary<GridBuildPiece, string> pieceToIdMap)
    {
        GridBuildPieceData data = new GridBuildPieceData
        {
            uniqueId = uniqueId,
            pieceId = piece.id,
            worldPosition = piece.transform.position,
            rotation = piece.transform.eulerAngles,
            sizeOnGrid = piece.sizeOnGrid,
            canBuildOnTop = piece.canBuildOnTop,
            storeThisToGrid = piece.storeThisToGrid,
            canBeRemovedFromGrid = piece.canBeRemovedFromGrid,
            layer = piece.layer,
            canBeBuiltOnLayers = piece.canBeBuiltOnLayers,
            occupiedPositions = new List<Vector2Int>(piece.occupiedPositions)
        };

        // Get references to objects on top
        if (piece.gridObjectsOnTop != null)
        {
            foreach (GridBuildPiece topPiece in piece.gridObjectsOnTop)
            {
                if (pieceToIdMap.ContainsKey(topPiece))
                {
                    data.objectsOnTopIds.Add(pieceToIdMap[topPiece]);
                }
            }
        }

        // Calculate origin grid position and direction
        if (data.occupiedPositions.Count > 0)
        {
            data.originGridPosition = data.occupiedPositions[0];
            data.placementDirection = CalculateDirection(data.occupiedPositions, data.sizeOnGrid);
        }

        return data;
    }

    private Direction CalculateDirection(List<Vector2Int> occupiedPositions, Vector2Int size)
    {
        if (occupiedPositions.Count <= 1) return Direction.Down;

        Vector2Int origin = occupiedPositions[0];
        Vector2Int second = occupiedPositions[1];
        Vector2Int diff = second - origin;

        // Determine direction based on how positions are laid out
        if (diff.x == 1 && diff.y == 0) return Direction.Down;
        if (diff.x == 0 && diff.y == 1) return Direction.Left;
        // Add more logic for Up and Right if needed based on your grid layout
        
        return Direction.Down; // Default
    }

    public bool SaveToFile(GridExportData data, string filePath)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(filePath, json);
            Debug.Log($"Grid data saved to: {filePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save grid data: {e.Message}");
            return false;
        }
    }

    public GridExportData LoadFromFile(string filePath)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError($"File does not exist: {filePath}");
                return null;
            }

            string json = System.IO.File.ReadAllText(filePath);
            GridExportData data = JsonUtility.FromJson<GridExportData>(json);
            Debug.Log($"Grid data loaded from: {filePath}");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load grid data: {e.Message}");
            return null;
        }
    }
}

// Extension methods to help with the export/import process
public static class GridExportExtensions
{
    public static void ExportToFile(this GridExporter exporter, string filePath)
    {
        GridExportData data = exporter.ExportGrid();
        if (data != null)
        {
            exporter.SaveToFile(data, filePath);
        }
    }

    public static bool ImportFromFile(this GridLoader loader, string filePath)
    {
        GridExporter exporter = loader.GetComponent<GridExporter>();
        if (exporter == null)
        {
            exporter = loader.gameObject.AddComponent<GridExporter>();
        }

        GridExportData data = exporter.LoadFromFile(filePath);
        return data != null && loader.LoadGrid(data);
    }
}