using System.Collections.Generic;
using UnityEngine;

public class GridLoader : MonoBehaviour
{
    public GridManager gridManager;
    public BuildPieceDatabaseSO database;
    public BuildingManager buildingManager;

    public bool LoadGrid(GridExportData exportData)
    {
        if (exportData == null || gridManager?.Grid == null)
        {
            Debug.LogError("Export data or GridManager is null!");
            return false;
        }

        // Clear existing grid
        ClearGrid();

        // Verify grid dimensions match
        if (gridManager.Grid.GetWidth() != exportData.gridSize.x || 
            gridManager.Grid.GetHeight() != exportData.gridSize.y)
        {
            Debug.LogWarning("Grid size mismatch! Current grid size may not match saved data.");
        }

        // Dictionary to map unique IDs back to GridBuildPiece instances
        Dictionary<string, GridBuildPiece> idToPieceMap = new Dictionary<string, GridBuildPiece>();

        // First pass: Create all GameObjects and GridBuildPiece components
        foreach (GridBuildPieceData pieceData in exportData.buildPieces)
        {
            BuildPieceData buildData = database.objectsData.Find(p => p.ID == pieceData.pieceId);
            if (buildData == null)
            {
                Debug.LogError($"Build piece with ID {pieceData.pieceId} not found in database!");
                continue;
            }

            // Create the piece at the saved position and rotation
            Vector3 position = pieceData.worldPosition;
            Quaternion rotation = Quaternion.Euler(pieceData.rotation);
            GridBuildPiece piece = buildingManager.CreateBuildPiece(buildData, position, rotation);

            // Apply saved data BEFORE calling Init so OnBuild uses the saved state
            ApplySavedDataBeforeInit(piece, pieceData);

            // Initialize the piece - this will call OnBuild with the saved data already applied
            piece.Init(buildData, "GridLoader");

            idToPieceMap[pieceData.uniqueId] = piece;
        }

        // Second pass: Rebuild relationships and add to grid
        foreach (GridBuildPieceData pieceData in exportData.buildPieces)
        {
            if (!idToPieceMap.TryGetValue(pieceData.uniqueId, out GridBuildPiece piece))
                continue;

            // Rebuild objects on top relationships
            foreach (string topId in pieceData.objectsOnTopIds)
            {
                if (idToPieceMap.TryGetValue(topId, out GridBuildPiece topPiece))
                {
                    piece.gridObjectsOnTop.Add(topPiece);
                }
            }
        }

        // Third pass: Add pieces to grid in the correct order (bottom to top)
        var sortedPieces = SortPiecesByStackOrder(exportData.buildPieces, idToPieceMap);

        foreach (var pieceData in sortedPieces)
        {
            if (!idToPieceMap.TryGetValue(pieceData.uniqueId, out GridBuildPiece piece))
                continue;

            if (piece.storeThisToGrid)
            {
                // Add to grid using the saved occupied positions
                foreach (Vector2Int pos in piece.occupiedPositions)
                {
                    if (gridManager.Grid.IsGridObjectInGrid(pos))
                    {
                        GridCell cell = gridManager.Grid.GetGridObject(pos.x, pos.y);
                        cell.AddGridBuildPiece(piece);
                    }
                }
            }
        }

        Debug.Log($"Successfully loaded {exportData.buildPieces.Count} build pieces to grid.");
        return true;
    }

    /// <summary>
    /// Apply saved data to piece BEFORE Init so OnBuild uses the saved state
    /// </summary>
    private void ApplySavedDataBeforeInit(GridBuildPiece piece, GridBuildPieceData savedData)
    {
        // Apply saved data BEFORE calling Init so OnBuild will use this data
        piece.id = savedData.pieceId;
        piece.sizeOnGrid = savedData.sizeOnGrid;
        piece.canBuildOnTop = savedData.canBuildOnTop;
        piece.storeThisToGrid = savedData.storeThisToGrid;
        piece.canBeRemovedFromGrid = savedData.canBeRemovedFromGrid;
        piece.layer = savedData.layer;
        piece.canBeBuiltOnLayers = savedData.canBeBuiltOnLayers;
        piece.occupiedPositions = new List<Vector2Int>(savedData.occupiedPositions);
        
        // Initialize the gridObjectsOnTop list (relationships will be rebuilt in second pass)
        piece.gridObjectsOnTop = new List<GridBuildPiece>();
    }

    private List<GridBuildPieceData> SortPiecesByStackOrder(List<GridBuildPieceData> pieces, Dictionary<string, GridBuildPiece> idToPieceMap)
    {
        // Create a dependency graph
        Dictionary<string, List<string>> dependencies = new Dictionary<string, List<string>>();
        Dictionary<string, int> inDegree = new Dictionary<string, int>();

        foreach (var piece in pieces)
        {
            dependencies[piece.uniqueId] = new List<string>(piece.objectsOnTopIds);
            inDegree[piece.uniqueId] = 0;
        }

        // Calculate in-degrees (how many pieces are below each piece)
        foreach (var piece in pieces)
        {
            foreach (string topId in piece.objectsOnTopIds)
            {
                if (inDegree.ContainsKey(topId))
                {
                    inDegree[topId]++;
                }
            }
        }

        // Topological sort to ensure bottom pieces are placed first
        Queue<string> queue = new Queue<string>();
        List<GridBuildPieceData> sorted = new List<GridBuildPieceData>();

        // Start with pieces that have no dependencies (bottom pieces)
        foreach (var kvp in inDegree)
        {
            if (kvp.Value == 0)
            {
                queue.Enqueue(kvp.Key);
            }
        }

        while (queue.Count > 0)
        {
            string currentId = queue.Dequeue();
            GridBuildPieceData currentPiece = pieces.Find(p => p.uniqueId == currentId);
            if (currentPiece != null)
            {
                sorted.Add(currentPiece);
            }

            // Reduce in-degree for dependent pieces
            if (dependencies.ContainsKey(currentId))
            {
                foreach (string dependentId in dependencies[currentId])
                {
                    inDegree[dependentId]--;
                    if (inDegree[dependentId] == 0)
                    {
                        queue.Enqueue(dependentId);
                    }
                }
            }
        }

        return sorted;
    }

    private void ClearGrid()
    {
        // Clear all existing build pieces from the grid
        for (int x = 0; x < gridManager.Grid.GetWidth(); x++)
        {
            for (int z = 0; z < gridManager.Grid.GetHeight(); z++)
            {
                GridCell cell = gridManager.Grid.GetGridObject(x, z);
                if (cell != null)
                {
                    while (cell.GetTopGridObject() != null)
                    {
                        GridBuildPiece piece = cell.RemoveTopGridBuildPiece();
                        if (piece != null)
                        {
                            buildingManager.DestroyBuildPiece(piece);
                        }
                    }
                }
            }
        }
    }
}