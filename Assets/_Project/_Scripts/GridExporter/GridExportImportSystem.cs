using System;
using System.Collections.Generic;
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

public class GridExportImportSystem : MonoBehaviour
{
    [Header("Core References")]
    public GridManager gridManager;
    public BuildPieceDatabaseSO database;
    public BuildingManager buildingManager;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent<GridExportData> OnGridLoaded;
    public UnityEngine.Events.UnityEvent OnGridLoadFailed;
    public UnityEngine.Events.UnityEvent<GridExportData> OnGridExported;
    
    #region Export Methods
    
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
        OnGridExported?.Invoke(exportData);
        return exportData;
    }

    private void CollectAllPiecesInStack(GridCell cell, HashSet<GridBuildPiece> processedPieces, Dictionary<GridBuildPiece, string> pieceToIdMap)
    {
        List<GridBuildPiece> stackPieces = new List<GridBuildPiece>();
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

    #endregion

    #region Import Methods
    // Load from file path
    public bool LoadFromFile(string filePath)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError($"File does not exist: {filePath}");
                return false;
            }

            string json = System.IO.File.ReadAllText(filePath);
            GridExportData data = JsonUtility.FromJson<GridExportData>(json);
            return LoadGrid(data);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load grid data: {e.Message}");
            OnGridLoadFailed?.Invoke();
            return false;
        }
    }
    
    public bool LoadFromTextAsset(TextAsset asset)
    {
        try
        {
            string json = asset.text;
            GridExportData data = JsonUtility.FromJson<GridExportData>(json);
            return LoadGrid(data);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load grid data: {e.Message}");
            OnGridLoadFailed?.Invoke();
            return false;
        }
    }

    // Core load method
    public bool LoadGrid(GridExportData exportData)
    {
        if (exportData == null || gridManager?.Grid == null)
        {
            Debug.LogError("Export data or GridManager is null!");
            OnGridLoadFailed?.Invoke();
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
        OnGridLoaded?.Invoke(exportData);
        return true;
    }

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

    #endregion

    #region Utility Methods
    // Quick export with timestamp
    [ContextMenu("Quick Export")]
    public void QuickExport()
    {
        GridExportData data = ExportGrid();
        if (data != null)
        {
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, 
                "GridData_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");
            SaveToFile(data, path);
        }
    }

    #endregion
}

