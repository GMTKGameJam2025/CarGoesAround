using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Enhanced ScriptableObject version of GridExportData
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
    
    // New fields for better validation
    public int stackDepth; // How deep in the stack this piece is (0 = ground level)
    public string builtSource;
}

public class GridExportImportSystem : MonoBehaviour
{
    [Header("Core References")]
    public GridManager gridManager;
    public BuildPieceDatabaseSO database;
    public BuildingManager buildingManager;

    [Header("ScriptableObject Settings")]
    [SerializeField] private GridLevelDataSO currentLevelData;
    public string defaultSaveFolder = "Assets/LevelData/";
    [SerializeField] private bool autoLoadOnStart = true;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent<GridLevelDataSO> OnGridLoaded;
    public UnityEngine.Events.UnityEvent OnGridLoadFailed;
    public UnityEngine.Events.UnityEvent<GridLevelDataSO> OnGridExported;

    // Export system
    private GridExporter _exporter;
    // Import system
    private GridImporter _importer;

    private void Awake()
    {
        _exporter = new GridExporter(this);
        _importer = new GridImporter(this);
    }

    private void Start()
    {
        if (autoLoadOnStart && currentLevelData != null)
        {
            StartCoroutine(InitializeAndLoad());
        }
    }

    private IEnumerator InitializeAndLoad()
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
        return _exporter.ExportGrid(fileName);
    }

    #if UNITY_EDITOR
    public bool SaveScriptableObjectToFile(GridLevelDataSO levelData, string fileName = null)
    {
        return _exporter.SaveToFile(levelData, fileName);
    }
    #endif
    #endregion

    #region Import Methods
    public bool LoadFromScriptableObject(GridLevelDataSO levelData)
    {
        return _importer.LoadGrid(levelData);
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
                var jsonData = JsonUtility.FromJson<GridExportDataLegacy>(json);

                GridLevelDataSO levelData = ScriptableObject.CreateInstance<GridLevelDataSO>();
                levelData.levelName = System.IO.Path.GetFileNameWithoutExtension(jsonPath);
                levelData.gridSize = jsonData.gridSize;
                levelData.cellSize = jsonData.cellSize;
                levelData.gridOrigin = jsonData.gridOrigin;
                levelData.buildPieces = jsonData.buildPieces;

                string fileName = System.IO.Path.GetFileNameWithoutExtension(jsonPath) + ".asset";
                SaveScriptableObjectToFile(levelData, fileName);
                Debug.Log($"Successfully converted {jsonPath} to ScriptableObject!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to convert JSON: {e.Message}");
            }
        }
    }
    #endif

    public GridLevelDataSO GetCurrentLevel() => currentLevelData;
    public void SetCurrentLevel(GridLevelDataSO levelData) => currentLevelData = levelData;
    #endregion
}

// Handles the export logic
public class GridExporter
{
    private readonly GridExportImportSystem _system;
    
    public GridExporter(GridExportImportSystem system)
    {
        _system = system;
    }

    public GridLevelDataSO ExportGrid(string fileName = null)
    {
        if (_system.gridManager?.Grid == null)
        {
            Debug.LogError("GridManager or Grid is null!");
            return null;
        }

        // Create new ScriptableObject instance
        GridLevelDataSO levelData = ScriptableObject.CreateInstance<GridLevelDataSO>();
        
        // Set basic data
        levelData.levelName = fileName ?? $"GridLevel_{DateTime.Now:yyyyMMdd_HHmmss}";
        levelData.gridSize = new Vector2Int(_system.gridManager.Grid.GetWidth(), _system.gridManager.Grid.GetHeight());
        levelData.cellSize = _system.gridManager.Grid.GetCellSize();
        levelData.gridOrigin = _system.gridManager.transform.position;
        levelData.creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Export grid pieces
        ExportGridPieces(levelData);

        Debug.Log($"Exported {levelData.buildPieces.Count} build pieces to ScriptableObject.");
        _system.OnGridExported?.Invoke(levelData);
        return levelData;
    }

    private void ExportGridPieces(GridLevelDataSO levelData)
    {
        Dictionary<GridBuildPiece, string> pieceToIdMap = new Dictionary<GridBuildPiece, string>();
        HashSet<GridBuildPiece> processedPieces = new HashSet<GridBuildPiece>();

        // First pass: Collect all unique build pieces and assign IDs using GridManager
        for (int x = 0; x < levelData.gridSize.x; x++)
        {
            for (int z = 0; z < levelData.gridSize.y; z++)
            {
                Vector2Int position = new Vector2Int(x, z);
                GridBuildPiece topPiece = _system.gridManager.GetTopLevelObject(position);
                
                if (topPiece != null && !processedPieces.Contains(topPiece))
                {
                    CollectAllPiecesFromPosition(position, processedPieces, pieceToIdMap);
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

    private void CollectAllPiecesFromPosition(Vector2Int position, HashSet<GridBuildPiece> processedPieces, Dictionary<GridBuildPiece, string> pieceToIdMap)
    {
        // Get all pieces in the stack at this position using GridManager
        GridCell cell = _system.gridManager.Grid.GetGridObject(position.x, position.y);
        if (cell == null) return;

        // Collect all pieces in the stack without modifying the grid
        List<GridBuildPiece> stackPieces = GetAllPiecesInStackNonDestructive(cell);

        // Assign unique IDs to all pieces in the stack
        foreach (GridBuildPiece piece in stackPieces)
        {
            if (!processedPieces.Contains(piece))
            {
                processedPieces.Add(piece);
                pieceToIdMap[piece] = Guid.NewGuid().ToString();
            }
        }
    }

    private List<GridBuildPiece> GetAllPiecesInStackNonDestructive(GridCell cell)
    {
        List<GridBuildPiece> pieces = new List<GridBuildPiece>();
        Stack<GridBuildPiece> tempStack = new Stack<GridBuildPiece>();

        // Safely extract all pieces
        while (cell.GetTopGridObject() != null)
        {
            GridBuildPiece piece = cell.RemoveTopGridBuildPiece();
            if (piece != null)
            {
                tempStack.Push(piece);
                pieces.Add(piece);
            }
        }

        // Restore the stack in correct order
        while (tempStack.Count > 0)
        {
            cell.AddGridBuildPiece(tempStack.Pop());
        }

        return pieces;
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
            occupiedPositions = new List<Vector2Int>(piece.occupiedPositions),
            builtSource = piece.builtSource,
            stackDepth = CalculateStackDepth(piece)
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

    private int CalculateStackDepth(GridBuildPiece piece)
    {
        // Find how many pieces are below this one in any of its occupied positions
        int maxDepth = 0;
        foreach (Vector2Int pos in piece.occupiedPositions)
        {
            if (_system.gridManager.Grid.IsGridObjectInGrid(pos))
            {
                GridBuildPiece topPiece = _system.gridManager.GetTopLevelObject(pos);
                if (topPiece != null)
                {
                    int depth = GetDepthInStackUsingGridManager(pos, piece);
                    maxDepth = Mathf.Max(maxDepth, depth);
                }
            }
        }
        return maxDepth;
    }

    private int GetDepthInStackUsingGridManager(Vector2Int position, GridBuildPiece targetPiece)
    {
        GridCell cell = _system.gridManager.Grid.GetGridObject(position.x, position.y);
        if (cell == null) return 0;

        List<GridBuildPiece> stackPieces = GetAllPiecesInStackNonDestructive(cell);
        for (int i = 0; i < stackPieces.Count; i++)
        {
            if (stackPieces[i] == targetPiece)
            {
                return stackPieces.Count - 1 - i; // Bottom piece has highest depth
            }
        }
        return 0;
    }

    private Direction CalculateDirection(List<Vector2Int> occupiedPositions, Vector2Int size)
    {
        if (occupiedPositions.Count <= 1) return Direction.Down;

        Vector2Int origin = occupiedPositions[0];
        
        // Test each direction to see which one matches the occupied positions
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            List<Vector2Int> expectedPositions = origin.GetGridPositionList(size, dir);
            if (ListsEqual(occupiedPositions, expectedPositions))
            {
                return dir;
            }
        }
        
        return Direction.Down; // Default fallback
    }

    private bool ListsEqual(List<Vector2Int> list1, List<Vector2Int> list2)
    {
        if (list1.Count != list2.Count) return false;
        var sorted1 = list1.OrderBy(p => p.x).ThenBy(p => p.y).ToList();
        var sorted2 = list2.OrderBy(p => p.x).ThenBy(p => p.y).ToList();
        
        for (int i = 0; i < sorted1.Count; i++)
        {
            if (sorted1[i] != sorted2[i]) return false;
        }
        return true;
    }

    #if UNITY_EDITOR
    public bool SaveToFile(GridLevelDataSO levelData, string fileName = null)
    {
        try
        {
            if (levelData == null)
            {
                Debug.LogError("LevelData is null!");
                return false;
            }

            // Ensure the directory exists
            if (!System.IO.Directory.Exists(_system.defaultSaveFolder))
            {
                System.IO.Directory.CreateDirectory(_system.defaultSaveFolder);
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

            string fullPath = System.IO.Path.Combine(_system.defaultSaveFolder, fileName);

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
        catch (Exception e)
        {
            Debug.LogError($"Failed to save ScriptableObject: {e.Message}");
            return false;
        }
    }
    #endif
}

// Handles the import logic
public class GridImporter
{
    private readonly GridExportImportSystem _system;
    
    public GridImporter(GridExportImportSystem system)
    {
        _system = system;
    }

    public bool LoadGrid(GridLevelDataSO levelData)
    {
        if (!ValidateLevelData(levelData))
        {
            _system.OnGridLoadFailed?.Invoke();
            return false;
        }

        Debug.Log($"Loading level: {levelData.levelName}");

        // Clear existing grid
        ClearGrid();

        // Set current level
        _system.SetCurrentLevel(levelData);

        // Validate grid dimensions
        ValidateGridDimensions(levelData);

        // Load the pieces
        bool success = LoadGridPieces(levelData);
        
        if (success)
        {
            _system.OnGridLoaded?.Invoke(levelData);
        }
        else
        {
            _system.OnGridLoadFailed?.Invoke();
        }
        
        return success;
    }

    private bool ValidateLevelData(GridLevelDataSO levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("Level data is null!");
            return false;
        }

        if (!levelData.IsValid())
        {
            Debug.LogError($"Level data '{levelData.name}' is invalid!");
            return false;
        }

        if (_system.gridManager?.Grid == null)
        {
            Debug.LogError("GridManager or Grid is null!");
            return false;
        }

        return true;
    }

    private void ValidateGridDimensions(GridLevelDataSO levelData)
    {
        if (_system.gridManager.Grid.GetWidth() != levelData.gridSize.x || 
            _system.gridManager.Grid.GetHeight() != levelData.gridSize.y)
        {
            Debug.LogWarning("Grid size mismatch! Current grid size may not match saved data.");
        }
    }

    private bool LoadGridPieces(GridLevelDataSO levelData)
    {
        Dictionary<string, GridBuildPiece> idToPieceMap = new Dictionary<string, GridBuildPiece>();

        // First pass: Create all GameObjects and GridBuildPiece components
        if (!CreateAllPieces(levelData, idToPieceMap))
        {
            return false;
        }

        // Second pass: Rebuild relationships
        RestoreRelationships(levelData, idToPieceMap);

        // Third pass: Add pieces to grid in correct order (bottom to top)
        AddPiecesToGrid(levelData, idToPieceMap);

        Debug.Log($"Successfully loaded level '{levelData.levelName}' with {levelData.buildPieces.Count} pieces");
        return true;
    }

    private bool CreateAllPieces(GridLevelDataSO levelData, Dictionary<string, GridBuildPiece> idToPieceMap)
    {
        foreach (GridBuildPieceData pieceData in levelData.buildPieces)
        {
            BuildPieceData buildData = _system.database.objectsData.Find(p => p.ID == pieceData.pieceId);
            if (buildData == null)
            {
                Debug.LogError($"Build piece with ID {pieceData.pieceId} not found in database!");
                continue;
            }

            // Use BuildingManager to create the piece properly
            Vector3 position = pieceData.worldPosition;
            Quaternion rotation = Quaternion.Euler(pieceData.rotation);
            GridBuildPiece piece = _system.buildingManager.CreateBuildPiece(buildData, position, rotation);

            // Apply saved data BEFORE calling Init
            ApplySavedDataBeforeInit(piece, pieceData);
            piece.Init(buildData, pieceData.builtSource);

            idToPieceMap[pieceData.uniqueId] = piece;
        }

        return true;
    }

    private void RestoreRelationships(GridLevelDataSO levelData, Dictionary<string, GridBuildPiece> idToPieceMap)
    {
        foreach (GridBuildPieceData pieceData in levelData.buildPieces)
        {
            if (!idToPieceMap.TryGetValue(pieceData.uniqueId, out GridBuildPiece piece)) 
                continue;

            // Restore objects on top relationships
            foreach (string topId in pieceData.objectsOnTopIds)
            {
                if (idToPieceMap.TryGetValue(topId, out GridBuildPiece topPiece))
                {
                    piece.gridObjectsOnTop.Add(topPiece);
                }
            }
        }
    }

    private void AddPiecesToGrid(GridLevelDataSO levelData, Dictionary<string, GridBuildPiece> idToPieceMap)
    {
        // Sort pieces by stack depth (bottom pieces first)
        var sortedPieces = levelData.buildPieces
            .Where(p => p.storeThisToGrid) // Only add pieces that should be stored to grid
            .OrderBy(p => p.stackDepth)
            .ThenBy(p => p.uniqueId); // Secondary sort for consistency

        foreach (var pieceData in sortedPieces)
        {
            if (!idToPieceMap.TryGetValue(pieceData.uniqueId, out GridBuildPiece piece)) 
                continue;

            // Use GridManager's AddObjectToGrid method with the proper parameters
            Vector2Int originPosition = pieceData.originGridPosition;
            Vector2Int size = pieceData.sizeOnGrid;
            Direction direction = pieceData.placementDirection;

            // Add to grid using GridManager's method
            _system.gridManager.AddObjectToGrid(piece, originPosition, size, direction);
        }
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

    private void ClearGrid()
    {
        // Get all occupied positions first to avoid issues with iteration
        List<Vector2Int> occupiedPositions = new List<Vector2Int>();
        
        for (int x = 0; x < _system.gridManager.Grid.GetWidth(); x++)
        {
            for (int z = 0; z < _system.gridManager.Grid.GetHeight(); z++)
            {
                Vector2Int position = new Vector2Int(x, z);
                GridBuildPiece topPiece = _system.gridManager.GetTopLevelObject(position);
                
                if (topPiece != null)
                {
                    occupiedPositions.Add(position);
                }
            }
        }

        // Remove all pieces using GridManager's removal method
        HashSet<GridBuildPiece> processedPieces = new HashSet<GridBuildPiece>();
        
        foreach (Vector2Int position in occupiedPositions)
        {
            GridBuildPiece topPiece = _system.gridManager.GetTopLevelObject(position);
            
            // Skip if already processed or no piece
            if (topPiece == null || processedPieces.Contains(topPiece))
                continue;

            // Mark as processed
            processedPieces.Add(topPiece);
            
            // Find the origin position for this piece (should be the first occupied position)
            Vector2Int originPos = position;
            if (topPiece.occupiedPositions != null && topPiece.occupiedPositions.Count > 0)
            {
                originPos = topPiece.occupiedPositions[0];
            }
            
            // Use GridManager's removal method which handles multi-cell pieces properly
            GridBuildPiece removedPiece = _system.gridManager.RemoveObjectFromGrid(originPos);
            
            // Destroy using BuildingManager
            if (removedPiece != null)
            {
                _system.buildingManager.DestroyBuildPiece(removedPiece);
            }
        }
        
        // Safety check to ensure grid is completely clear
        VerifyGridIsEmpty();
    }
    
    private void VerifyGridIsEmpty()
    {
        for (int x = 0; x < _system.gridManager.Grid.GetWidth(); x++)
        {
            for (int z = 0; z < _system.gridManager.Grid.GetHeight(); z++)
            {
                Vector2Int position = new Vector2Int(x, z);
                GridBuildPiece remainingPiece = _system.gridManager.GetTopLevelObject(position);
                
                if (remainingPiece != null)
                {
                    Debug.LogWarning($"Found remaining piece at {position}: {remainingPiece.name}. Force removing...");
                    
                    // Force removal using grid manager
                    GridBuildPiece forcedRemoval = _system.gridManager.RemoveObjectFromGrid(position);
                    if (forcedRemoval != null)
                    {
                        _system.buildingManager.DestroyBuildPiece(forcedRemoval);
                    }
                }
            }
        }
    }
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