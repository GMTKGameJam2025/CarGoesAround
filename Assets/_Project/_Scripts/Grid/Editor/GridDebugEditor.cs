```
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GridManager))]
public class GridDebugEditor : Editor
{
    private bool showGridCells = true;
    private bool showStackInfo = true;
    private bool showOccupiedPositions = true;
    private bool showRelationships = true;
    private bool showCellNumbers = false;
    private bool showWorldPositions = false;
    
    private Color emptyCellColor = new Color(0.8f, 0.8f, 0.8f, 0.3f);
    private Color occupiedCellColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
    private Color multiStackColor = new Color(0.8f, 0.2f, 0.2f, 0.7f);
    private Color relationshipColor = Color.yellow;
    
    private Vector2 scrollPosition;
    private GridManager gridManager;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        gridManager = (GridManager)target;
        
        if (gridManager == null || gridManager.Grid == null)
        {
            EditorGUILayout.HelpBox("Grid not initialized", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Grid Debug Tools", EditorStyles.boldLabel);
        
        // Visual options
        showGridCells = EditorGUILayout.Toggle("Show Grid Cells", showGridCells);
        showStackInfo = EditorGUILayout.Toggle("Show Stack Info", showStackInfo);
        showOccupiedPositions = EditorGUILayout.Toggle("Show Occupied Positions", showOccupiedPositions);
        showRelationships = EditorGUILayout.Toggle("Show Relationships", showRelationships);
        showCellNumbers = EditorGUILayout.Toggle("Show Cell Numbers", showCellNumbers);
        showWorldPositions = EditorGUILayout.Toggle("Show World Positions", showWorldPositions);
        
        EditorGUILayout.Space(5);
        
        // Color settings
        EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
        emptyCellColor = EditorGUILayout.ColorField("Empty Cell", emptyCellColor);
        occupiedCellColor = EditorGUILayout.ColorField("Occupied Cell", occupiedCellColor);
        multiStackColor = EditorGUILayout.ColorField("Multi-Stack Cell", multiStackColor);
        relationshipColor = EditorGUILayout.ColorField("Relationships", relationshipColor);
        
        EditorGUILayout.Space(10);
        
        // Grid info
        DrawGridInfo();
        
        // Force scene view repaint
        if (GUI.changed)
        {
            SceneView.RepaintAll();
        }
    }
    
    private void DrawGridInfo()
    {
        EditorGUILayout.LabelField("Grid Information", EditorStyles.boldLabel);
        
        int width = gridManager.Grid.Width;
        int height = gridManager.Grid.Height;
        float cellSize = gridManager.Grid.CellSize;
        
        EditorGUILayout.LabelField($"Dimensions: {width} x {height}");
        EditorGUILayout.LabelField($"Cell Size: {cellSize}");
        
        // Count statistics
        int totalPieces = 0;
        int occupiedCells = 0;
        int stackedCells = 0;
        
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridCell cell = gridManager.Grid.GetGridObject(x, z);
                if (cell != null && cell.GetTopGridObject() != null)
                {
                    occupiedCells++;
                    int stackCount = GetStackCount(cell);
                    totalPieces += stackCount;
                    if (stackCount > 1)
                    {
                        stackedCells++;
                    }
                }
            }
        }
        
        EditorGUILayout.LabelField($"Occupied Cells: {occupiedCells}/{width * height}");
        EditorGUILayout.LabelField($"Stacked Cells: {stackedCells}");
        EditorGUILayout.LabelField($"Total Pieces: {totalPieces}");
        
        EditorGUILayout.Space(10);
        
        // Detailed cell information
        if (GUILayout.Button("Show Detailed Cell Info"))
        {
            ShowDetailedCellInfo();
        }
    }
    
    private void ShowDetailedCellInfo()
    {
        GridDebugWindow.ShowWindow(gridManager);
    }
    
    private int GetStackCount(GridCell cell)
    {
        int count = 0;
        List<GridBuildPiece> pieces = new List<GridBuildPiece>();
        System.Collections.Generic.Stack<GridBuildPiece> tempStack = new System.Collections.Generic.Stack<GridBuildPiece>();
        
        // Extract all pieces
        while (cell.GetTopGridObject() != null)
        {
            GridBuildPiece piece = cell.RemoveTopGridBuildPiece();
            if (piece != null)
            {
                tempStack.Push(piece);
                pieces.Add(piece);
                count++;
            }
        }
        
        // Restore stack
        while (tempStack.Count > 0)
        {
            cell.AddGridBuildPiece(tempStack.Pop());
        }
        
        return count;
    }
    
    private void OnSceneGUI()
    {
        if (!showGridCells || gridManager == null || gridManager.Grid == null)
            return;
            
        DrawGridVisualization();
    }
    
    private void DrawGridVisualization()
    {
        int width = gridManager.Grid.Width;
        int height = gridManager.Grid.Height;
        float cellSize = gridManager.Grid.CellSize;
        
        Handles.BeginGUI();
        
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector2Int gridPos = new Vector2Int(x, z);
                Vector3 worldPos = gridManager.Grid.GetWorldPosition(x, z);
                Vector3 cellCenter = worldPos + new Vector3(cellSize * 0.5f, 0, cellSize * 0.5f);
                
                GridCell cell = gridManager.Grid.GetGridObject(x, z);
                
                // Draw cell background
                DrawCellBackground(worldPos, cellSize, cell);
                
                // Draw cell content info
                if (showStackInfo && cell != null)
                {
                    DrawCellStackInfo(cellCenter, cell, gridPos);
                }
                
                // Draw cell numbers
                if (showCellNumbers)
                {
                    DrawCellNumbers(cellCenter, gridPos);
                }
                
                // Draw world positions
                if (showWorldPositions)
                {
                    DrawWorldPosition(cellCenter, worldPos);
                }
                
                // Draw occupied positions
                if (showOccupiedPositions && cell != null && cell.GetTopGridObject() != null)
                {
                    DrawOccupiedPositions(cell.GetTopGridObject(), gridPos);
                }
                
                // Draw relationships
                if (showRelationships && cell != null && cell.GetTopGridObject() != null)
                {
                    DrawRelationships(cell.GetTopGridObject(), cellCenter);
                }
            }
        }
        
        Handles.EndGUI();
    }
    
    private void DrawCellBackground(Vector3 worldPos, float cellSize, GridCell cell)
    {
        Color cellColor = emptyCellColor;
        
        if (cell != null && cell.GetTopGridObject() != null)
        {
            int stackCount = GetStackCount(cell);
            cellColor = stackCount > 1 ? multiStackColor : occupiedCellColor;
        }
        
        Handles.color = cellColor;
        Vector3[] corners = new Vector3[4]
        {
            worldPos,
            worldPos + new Vector3(cellSize, 0, 0),
            worldPos + new Vector3(cellSize, 0, cellSize),
            worldPos + new Vector3(0, 0, cellSize)
        };
        
        Handles.DrawSolidRectangleWithOutline(corners, cellColor, Color.black);
    }
    
    private void DrawCellStackInfo(Vector3 cellCenter, GridCell cell, Vector2Int gridPos)
    {
        if (cell.GetTopGridObject() == null) return;
        
        int stackCount = GetStackCount(cell);
        GridBuildPiece topPiece = cell.GetTopGridObject();
        
        Vector3 screenPos = HandleUtility.WorldToGUIPoint(cellCenter);
        
        GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        
        string info = $"Stack: {stackCount}\nID: {topPiece.id}\nLayer: {topPiece.Layer}";
        
        Vector2 size = style.CalcSize(new GUIContent(info));
        GUI.Label(new Rect(screenPos.x - size.x * 0.5f, screenPos.y - size.y * 0.5f, size.x, size.y), info, style);
    }
    
    private void DrawCellNumbers(Vector3 cellCenter, Vector2Int gridPos)
    {
        Vector3 screenPos = HandleUtility.WorldToGUIPoint(cellCenter + Vector3.up * 0.1f);
        
        GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
        style.normal.textColor = Color.blue;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        
        string text = $"({gridPos.x},{gridPos.y})";
        Vector2 size = style.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - size.x * 0.5f, screenPos.y - size.y * 0.5f, size.x, size.y), text, style);
    }
    
    private void DrawWorldPosition(Vector3 cellCenter, Vector3 worldPos)
    {
        Vector3 screenPos = HandleUtility.WorldToGUIPoint(cellCenter - Vector3.up * 0.1f);
        
        GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
        style.normal.textColor = Color.magenta;
        style.fontSize = 8;
        style.alignment = TextAnchor.MiddleCenter;
        
        string text = $"W:({worldPos.x:F1},{worldPos.z:F1})";
        Vector2 size = style.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - size.x * 0.5f, screenPos.y - size.y * 0.5f, size.x, size.y), text, style);
    }
    
    private void DrawOccupiedPositions(GridBuildPiece piece, Vector2Int currentPos)
    {
        if (piece.OccupiedPositions == null) return;
        
        Handles.color = Color.cyan;
        
        foreach (Vector2Int pos in piece.OccupiedPositions)
        {
            if (pos != currentPos) // Don't draw line to self
            {
                Vector3 fromWorld = gridManager.Grid.GetWorldPosition(currentPos.x, currentPos.y);
                Vector3 toWorld = gridManager.Grid.GetWorldPosition(pos.x, pos.y);
                
                fromWorld += new Vector3(gridManager.Grid.CellSize * 0.5f, 0.05f, gridManager.Grid.CellSize * 0.5f);
                toWorld += new Vector3(gridManager.Grid.CellSize * 0.5f, 0.05f, gridManager.Grid.CellSize * 0.5f);
                
                Handles.DrawLine(fromWorld, toWorld);
                Handles.DrawWireCube(toWorld, Vector3.one * 0.2f);
            }
        }
    }
    
    private void DrawRelationships(GridBuildPiece piece, Vector3 cellCenter)
    {
        if (piece.GridObjectsOnTop == null) return;
        
        Handles.color = relationshipColor;
        
        foreach (GridBuildPiece topPiece in piece.GridObjectsOnTop)
        {
            if (topPiece != null)
            {
                Vector3 topPos = topPiece.transform.position;
                Vector3 fromPos = cellCenter + Vector3.up * 0.1f;
                Vector3 toPos = topPos + Vector3.up * 0.2f;
                
                // Draw arrow from bottom piece to top piece
                Handles.DrawLine(fromPos, toPos);
                Handles.DrawWireCube(toPos, Vector3.one * 0.15f);
            }
        }
    }
}

// Detailed debug window
public class GridDebugWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private GridManager gridManager;
    
    public static void ShowWindow(GridManager manager)
    {
        GridDebugWindow window = GetWindow<GridDebugWindow>("Grid Debug Details");
        window.gridManager = manager;
        window.Show();
    }
    
    private void OnGUI()
    {
        if (gridManager == null || gridManager.Grid == null)
        {
            EditorGUILayout.HelpBox("No grid manager assigned", MessageType.Info);
            return;
        }
        
        EditorGUILayout.LabelField("Detailed Grid Cell Information", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        int width = gridManager.Grid.Width;
        int height = gridManager.Grid.Height;
        
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridCell cell = gridManager.Grid.GetGridObject(x, z);
                if (cell != null && cell.GetTopGridObject() != null)
                {
                    DrawDetailedCellInfo(x, z, cell);
                }
            }
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawDetailedCellInfo(int x, int z, GridCell cell)
    {
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.LabelField($"Cell ({x}, {z})", EditorStyles.boldLabel);
        
        // Get all pieces in stack
        List<GridBuildPiece> pieces = GetAllPiecesInStack(cell);
        
        EditorGUILayout.LabelField($"Stack Count: {pieces.Count}");
        
        for (int i = pieces.Count - 1; i >= 0; i--) // Top to bottom
        {
            GridBuildPiece piece = pieces[i];
            string level = i == pieces.Count - 1 ? "TOP" : $"Level {pieces.Count - 1 - i}";
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"{level} - ID: {piece.id}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Name: {piece.name}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Layer: {piece.Layer}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Size: {piece.size}",