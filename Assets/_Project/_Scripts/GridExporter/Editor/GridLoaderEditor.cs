using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(GridLoader))]
public class GridLoaderEditor : Editor
{
    private string selectedFilePath = "";
    private GridExportData previewData;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridLoader loader = (GridLoader)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Grid Import Tools", EditorStyles.boldLabel);
        
        // File selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Import File:", GUILayout.Width(70));
        EditorGUILayout.LabelField(string.IsNullOrEmpty(selectedFilePath) ? "No file selected" : Path.GetFileName(selectedFilePath), EditorStyles.helpBox);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("Select Grid Data File", "Assets", "json");
            if (!string.IsNullOrEmpty(path))
            {
                selectedFilePath = path;
                LoadPreviewData(loader);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Preview information
        if (previewData != null)
        {
            EditorGUILayout.LabelField("File Preview:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Grid Size: {previewData.gridSize.x} x {previewData.gridSize.y}");
            EditorGUILayout.LabelField($"Cell Size: {previewData.cellSize}");
            EditorGUILayout.LabelField($"Build Pieces: {previewData.buildPieces.Count}");
            EditorGUILayout.LabelField($"Grid Origin: {previewData.gridOrigin}");
            
            EditorGUILayout.Space(5);
            
            // Show piece breakdown
            if (previewData.buildPieces.Count > 0)
            {
                EditorGUILayout.LabelField("Piece Breakdown:", EditorStyles.miniLabel);
                var pieceGroups = previewData.buildPieces.GroupBy(p => p.pieceId);
                foreach (var group in pieceGroups)
                {
                    EditorGUILayout.LabelField($"  ID {group.Key}: {group.Count()} pieces", EditorStyles.miniLabel);
                }
            }
        }
        
        EditorGUILayout.Space(5);
        
        // Import button
        GUI.backgroundColor = Color.cyan;
        GUI.enabled = !string.IsNullOrEmpty(selectedFilePath) && previewData != null;
        if (GUILayout.Button("Import Grid Data", GUILayout.Height(30)))
        {
            ImportGrid(loader);
        }
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        
        // Quick import buttons
        EditorGUILayout.LabelField("Quick Import Options:", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Import from Desktop"))
        {
            string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            QuickImportFromPath(loader, desktopPath);
        }
        
        if (GUILayout.Button("Import from StreamingAssets"))
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath);
            QuickImportFromPath(loader, streamingPath);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Warning about clearing existing data
        if (previewData != null)
        {
            EditorGUILayout.HelpBox("⚠️ WARNING: Importing will clear all existing grid data!", MessageType.Warning);
        }
        
        // Validation
        if (!loader.GetComponent<GridLoader>().gridManager)
        {
            EditorGUILayout.HelpBox("GridManager component not found! Please assign it in the inspector.", MessageType.Warning);
        }
        
        if (!loader.GetComponent<GridLoader>().database)
        {
            EditorGUILayout.HelpBox("BuildPieceDatabaseSO not assigned! Please assign it in the inspector.", MessageType.Warning);
        }
        
        if (!loader.GetComponent<GridLoader>().buildingManager)
        {
            EditorGUILayout.HelpBox("BuildingManager not assigned! Please assign it in the inspector.", MessageType.Warning);
        }
    }
    
    private void LoadPreviewData(GridLoader loader)
    {
        try
        {
            GridExporter exporter = loader.GetComponent<GridExporter>();
            if (exporter == null)
            {
                exporter = loader.gameObject.AddComponent<GridExporter>();
            }
            
            previewData = exporter.LoadFromFile(selectedFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load preview data: {e.Message}");
            previewData = null;
        }
    }
    
    private void ImportGrid(GridLoader loader)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Import Error", "Grid import only works in Play Mode!\nPlease enter Play Mode and try again.", "OK");
            return;
        }
        
        if (EditorUtility.DisplayDialog("Confirm Import", 
            $"This will clear all existing grid data and import {previewData.buildPieces.Count} build pieces.\n\nAre you sure you want to continue?", 
            "Import", "Cancel"))
        {
            if (loader.LoadGrid(previewData))
            {
                EditorUtility.DisplayDialog("Import Successful", 
                    $"Grid data imported successfully!\n\nPieces imported: {previewData.buildPieces.Count}", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Import Failed", "Failed to import grid data.", "OK");
            }
        }
    }
    
    private void QuickImportFromPath(GridLoader loader, string basePath)
    {
        string[] jsonFiles = Directory.GetFiles(basePath, "*.json");
        if (jsonFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("No Files Found", $"No JSON files found in {basePath}", "OK");
            return;
        }
        
        // Show file selection dialog
        string[] fileNames = new string[jsonFiles.Length];
        for (int i = 0; i < jsonFiles.Length; i++)
        {
            fileNames[i] = Path.GetFileName(jsonFiles[i]);
        }
        
        // Create a simple popup menu (using GenericMenu as a workaround)
        GenericMenu menu = new GenericMenu();
        for (int i = 0; i < jsonFiles.Length; i++)
        {
            string filePath = jsonFiles[i];
            menu.AddItem(new GUIContent(fileNames[i]), false, () => {
                selectedFilePath = filePath;
                LoadPreviewData(loader);
            });
        }
        menu.ShowAsContext();
    }
}