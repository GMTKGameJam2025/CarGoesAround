using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ScriptableObject version of GridExportData

[System.Serializable]
public class GridBuildPieceData
{
    public string uniqueId;
    public int pieceId;
    public Vector3 worldPosition;
    public Vector3 rotation;
    public Vector2Int sizeOnGrid;
    public bool canBuildOnTop;
    public bool storeThisToGrid;
    public bool canBeRemovedFromGrid;
    public BuildLayer layer;
    public BuildLayer canBeBuiltOnLayers;
    public List<Vector2Int> occupiedPositions = new List<Vector2Int>();
    public List<string> objectsOnTopIds = new List<string>();
    public Direction placementDirection;
    public Vector2Int originGridPosition;
}

public class GridExportImportSystem : MonoBehaviour
{
    [Header("Core References")]
    public GridManager gridManager;
    public BuildPieceDatabaseSO database;
    public BuildingManager buildingManager;
    
    [Header("ScriptableObject Settings")]
    [SerializeField] private GridLevelDataSO currentLevelData;
    [SerializeField] private string defaultSaveFolder = "Assets/GridLevels/";
    [SerializeField] private bool autoLoadOnStart = true;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent<GridLevelDataSO> OnGridLoaded;
    public UnityEngine.Events.UnityEvent OnGridLoadFailed;
    public UnityEngine.Events.UnityEvent<GridLevelDataSO> OnGridExported;

    private void Start()
    {
        if (autoLoadOnStart && currentLevelData != null)
        {
            StartCoroutine(InitializeAndLoad());
        }
    }

    private System.Collections.IEnumerator InitializeAndLoad()
    {
        // Wait for grid initialization
        while (gridManager?.Grid == null)
        {
            yield return new WaitForEndOfFrame();
        }
        
        LoadFromScriptableObject(currentLevelData);
    }

    #region Export Methods
    
    public GridLevelDataSO ExportGridToScriptableObject(string fileName = null)
    {
        if (gridManager?.Grid == null)
        {
            Debug.LogError("GridManager or Grid is null!");
            return null;
        }

        // Create new ScriptableObject instance
        GridLevelDataSO levelData = ScriptableObject.CreateInstance<GridLevelDataSO>();
        
        // Set basic data
        levelData.levelName = fileName ?? $"GridLevel_{DateTime.Now:yyyyMMdd_HHmmss}";
        levelData.gridSize = new Vector2Int(gridManager.Grid.GetWidth(), gridManager.Grid.GetHeight());
        levelData.cellSize = gridManager.Grid.GetCellSize();
        levelData.gridOrigin = gridManager.transform.position;
        levelData.creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Export grid pieces
        ExportGridPieces(levelData);

        Debug.Log($"Exported {levelData.buildPieces.Count} build pieces to ScriptableObject.");
        OnGridExported?.Invoke(levelData);
        
        return levelData;
    }

    private void ExportGridPieces(GridLevelDataSO levelData)
    {
        Dictionary<GridBuildPiece, string> pieceToIdMap = new Dictionary<GridBuildPiece, string>();
        HashSet<GridBuildPiece> processedPieces = new HashSet<GridBuildPiece>();

        // First pass: Collect all unique build pieces and assign IDs
        for (int x = 0; x < levelData.gridSize.x; x++)
        {
            for (int z = 0; z < levelData.gridSize.y; z++)
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
            levelData.buildPieces.Add(pieceData);
        }
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

        if (diff.x == 1 && diff.y == 0) return Direction.Down;
        if (diff.x == 0 && diff.y == 1) return Direction.Left;
        return Direction.Down;
    }

    #if UNITY_EDITOR
    public bool SaveScriptableObjectToFile(GridLevelDataSO levelData, string fileName = null)
    {
        try
        {
            if (levelData == null)
            {
                Debug.LogError("LevelData is null!");
                return false;
            }

            // Ensure the directory exists
            if (!System.IO.Directory.Exists(defaultSaveFolder))
            {
                System.IO.Directory.CreateDirectory(defaultSaveFolder);
            }

            // Generate filename if not provided
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = $"{levelData.levelName}.asset";
            }
            else if (!fileName.EndsWith(".asset"))
            {
                fileName += ".asset";
            }

            string fullPath = System.IO.Path.Combine(defaultSaveFolder, fileName);

            // Create the asset
            AssetDatabase.CreateAsset(levelData, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Grid level saved to: {fullPath}");
            
            // Select the created asset in the Project window
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = levelData;

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save ScriptableObject: {e.Message}");
            return false;
        }
    }
    #endif

    #endregion

    #region Import Methods

    public bool LoadFromScriptableObject(GridLevelDataSO levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("Level data is null!");
            OnGridLoadFailed?.Invoke();
            return false;
        }

        if (!levelData.IsValid())
        {
            Debug.LogError($"Level data '{levelData.name}' is invalid!");
            OnGridLoadFailed?.Invoke();
            return false;
        }

        if (gridManager?.Grid == null)
        {
            Debug.LogError("GridManager or Grid is null!");
            OnGridLoadFailed?.Invoke();
            return false;
        }

        Debug.Log($"Loading level: {levelData.levelName}");

        // Clear existing grid
        ClearGrid();

        // Set current level
        currentLevelData = levelData;

        // Verify grid dimensions match
        if (gridManager.Grid.GetWidth() != levelData.gridSize.x || gridManager.Grid.GetHeight() != levelData.gridSize.y)
        {
            Debug.LogWarning("Grid size mismatch! Current grid size may not match saved data.");
        }

        // Load the pieces
        return LoadGridPieces(levelData);
    }

    private bool LoadGridPieces(GridLevelDataSO levelData)
    {
        Dictionary<string, GridBuildPiece> idToPieceMap = new Dictionary<string, GridBuildPiece>();

        // First pass: Create all GameObjects and GridBuildPiece components
        foreach (GridBuildPieceData pieceData in levelData.buildPieces)
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

            // Apply saved data BEFORE calling Init
            ApplySavedDataBeforeInit(piece, pieceData);
            piece.Init(buildData);

            idToPieceMap[pieceData.uniqueId] = piece;
        }

        // Second pass: Rebuild relationships
        foreach (GridBuildPieceData pieceData in levelData.buildPieces)
        {
            if (!idToPieceMap.TryGetValue(pieceData.uniqueId, out GridBuildPiece piece)) continue;

            foreach (string topId in pieceData.objectsOnTopIds)
            {
                if (idToPieceMap.TryGetValue(topId, out GridBuildPiece topPiece))
                {
                    piece.gridObjectsOnTop.Add(topPiece);
                }
            }
        }

        // Third pass: Add pieces to grid in correct order
        var sortedPieces = SortPiecesByStackOrder(levelData.buildPieces, idToPieceMap);

        foreach (var pieceData in sortedPieces)
        {
            if (!idToPieceMap.TryGetValue(pieceData.uniqueId, out GridBuildPiece piece)) continue;

            if (piece.storeThisToGrid)
            {
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

        Debug.Log($"Successfully loaded level '{levelData.levelName}' with {levelData.buildPieces.Count} pieces");
        OnGridLoaded?.Invoke(levelData);
        return true;
    }

    private void ApplySavedDataBeforeInit(GridBuildPiece piece, GridBuildPieceData savedData)
    {
        piece.id = savedData.pieceId;
        piece.sizeOnGrid = savedData.sizeOnGrid;
        piece.canBuildOnTop = savedData.canBuildOnTop;
        piece.storeThisToGrid = savedData.storeThisToGrid;
        piece.canBeRemovedFromGrid = savedData.canBeRemovedFromGrid;
        piece.layer = savedData.layer;
        piece.canBeBuiltOnLayers = savedData.canBeBuiltOnLayers;
        piece.occupiedPositions = new List<Vector2Int>(savedData.occupiedPositions);
        piece.gridObjectsOnTop = new List<GridBuildPiece>();
    }

    private List<GridBuildPieceData> SortPiecesByStackOrder(List<GridBuildPieceData> pieces, Dictionary<string, GridBuildPiece> idToPieceMap)
    {
        Dictionary<string, List<string>> dependencies = new Dictionary<string, List<string>>();
        Dictionary<string, int> inDegree = new Dictionary<string, int>();

        foreach (var piece in pieces)
        {
            dependencies[piece.uniqueId] = new List<string>(piece.objectsOnTopIds);
            inDegree[piece.uniqueId] = 0;
        }

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

        Queue<string> queue = new Queue<string>();
        List<GridBuildPieceData> sorted = new List<GridBuildPieceData>();

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

    [ContextMenu("Export Grid to ScriptableObject")]
    public void QuickExportToScriptableObject()
    {
        GridLevelDataSO levelData = ExportGridToScriptableObject();
        if (levelData != null)
        {
            #if UNITY_EDITOR
            SaveScriptableObjectToFile(levelData);
            #else
            Debug.Log("ScriptableObject created but can't save to file outside editor.");
            #endif
        }
    }

    [ContextMenu("Load Current Level")]
    public void LoadCurrentLevel()
    {
        if (currentLevelData != null)
        {
            LoadFromScriptableObject(currentLevelData);
        }
        else
        {
            Debug.LogWarning("No current level data assigned!");
        }
    }

    #if UNITY_EDITOR
    [ContextMenu("Convert JSON to ScriptableObject")]
    public void ConvertJSONToScriptableObject()
    {
        string jsonPath = EditorUtility.OpenFilePanel("Select JSON file", Application.dataPath, "json");
        if (!string.IsNullOrEmpty(jsonPath))
        {
            try
            {
                string json = System.IO.File.ReadAllText(jsonPath);
                
                // Parse the old JSON format
                var jsonData = JsonUtility.FromJson<GridExportDataLegacy>(json);
                
                // Create new ScriptableObject
                GridLevelDataSO levelData = ScriptableObject.CreateInstance<GridLevelDataSO>();
                levelData.levelName = System.IO.Path.GetFileNameWithoutExtension(jsonPath);
                levelData.gridSize = jsonData.gridSize;
                levelData.cellSize = jsonData.cellSize;
                levelData.gridOrigin = jsonData.gridOrigin;
                levelData.buildPieces = jsonData.buildPieces;
                
                // Save it
                string fileName = System.IO.Path.GetFileNameWithoutExtension(jsonPath) + ".asset";
                SaveScriptableObjectToFile(levelData, fileName);
                
                Debug.Log($"Successfully converted {jsonPath} to ScriptableObject!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to convert JSON: {e.Message}");
            }
        }
    }
    #endif

    public GridLevelDataSO GetCurrentLevel() => currentLevelData;

    #endregion
}

// Legacy class for JSON conversion
[System.Serializable]
public class GridExportDataLegacy
{
    public List<GridBuildPieceData> buildPieces = new List<GridBuildPieceData>();
    public Vector2Int gridSize;
    public float cellSize;
    public Vector3 gridOrigin;
}