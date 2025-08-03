using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

#if UNITY_EDITOR

[CustomEditor(typeof(GridExportImportSystem))]
public class GridExportImportSystemEditor : Editor
{
    private string fileName = "GridData";
    private string selectedExportPath = "";
    private string selectedImportFilePath = "";
    private GridExportData previewData;

    // Tabs
    private int selectedTab = 0;
    private readonly string[] tabNames =
    {
        "Export", "Import"
    };

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridExportImportSystem system = (GridExportImportSystem)target;

        EditorGUILayout.Space(10);

        // Tab selection
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

        EditorGUILayout.Space(5);

        switch (selectedTab)
        {
            case 0:
                DrawExportTab(system);
                break;
            case 1:
                DrawImportTab(system);
                break;
        }

        EditorGUILayout.Space(10);
        DrawValidationSection(system);
    }

    private void DrawExportTab(GridExportImportSystem system)
    {
        EditorGUILayout.LabelField("Grid Export", EditorStyles.boldLabel);

        // File name input
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("File Name:", GUILayout.Width(70));
        fileName = EditorGUILayout.TextField(fileName);
        EditorGUILayout.EndHorizontal();

        // Path selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save Path:", GUILayout.Width(70));
        EditorGUILayout.LabelField(string.IsNullOrEmpty(selectedExportPath) ? "Assets/" : selectedExportPath, EditorStyles.helpBox);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.SaveFolderPanel("Select Save Location", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert absolute path to relative path
                if (path.StartsWith(Application.dataPath))
                {
                    selectedExportPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    selectedExportPath = path;
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Export button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Export Grid Data", GUILayout.Height(30)))
        {
            ExportGrid(system);
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(5);

        // Quick export buttons
        EditorGUILayout.LabelField("Quick Export Options:", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Export to Desktop"))
        {
            string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            ExportToPath(system, desktopPath);
        }

        if (GUILayout.Button("Export to StreamingAssets"))
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath);
            if (!Directory.Exists(streamingPath))
            {
                Directory.CreateDirectory(streamingPath);
            }
            ExportToPath(system, streamingPath);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawImportTab(GridExportImportSystem system)
    {
        EditorGUILayout.LabelField("Grid Import", EditorStyles.boldLabel);

        // File selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Import File:", GUILayout.Width(70));
        EditorGUILayout.LabelField(string.IsNullOrEmpty(selectedImportFilePath) ? "No file selected" : Path.GetFileName(selectedImportFilePath), EditorStyles.helpBox);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("Select Grid Data File", "Assets", "json");
            if (!string.IsNullOrEmpty(path))
            {
                selectedImportFilePath = path;
                LoadPreviewData(system);
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
        GUI.enabled = !string.IsNullOrEmpty(selectedImportFilePath) && previewData != null;
        if (GUILayout.Button("Import Grid Data", GUILayout.Height(30)))
        {
            ImportGrid(system);
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
            QuickImportFromPath(system, desktopPath);
        }

        if (GUILayout.Button("Import from StreamingAssets"))
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath);
            QuickImportFromPath(system, streamingPath);
        }

        EditorGUILayout.EndHorizontal();

        // Warning about clearing existing data
        if (previewData != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("⚠️ WARNING: Importing will clear all existing grid data!", MessageType.Warning);
        }
    }
    
    private void DrawValidationSection(GridExportImportSystem system)
    {
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

        bool hasErrors = false;

        if (system.gridManager == null)
        {
            EditorGUILayout.HelpBox("GridManager reference is missing!", MessageType.Error);
            hasErrors = true;
        }

        if (system.database == null)
        {
            EditorGUILayout.HelpBox("BuildPieceDatabaseSO reference is missing!", MessageType.Error);
            hasErrors = true;
        }

        if (system.buildingManager == null)
        {
            EditorGUILayout.HelpBox("BuildingManager reference is missing!", MessageType.Error);
            hasErrors = true;
        }

        if (!hasErrors)
        {
            EditorGUILayout.HelpBox("All references are properly assigned!", MessageType.Info);
        }
    }

    #region Export Methods

    private void ExportGrid(GridExportImportSystem system)
    {
        string savePath = string.IsNullOrEmpty(selectedExportPath) ? "Assets" : selectedExportPath;
        ExportToPath(system, savePath);
    }

    private void ExportToPath(GridExportImportSystem system, string path)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Export Error", "Grid export only works in Play Mode!\nPlease enter Play Mode and try again.", "OK");
            return;
        }

        GridExportData data = system.ExportGrid();
        if (data != null)
        {
            string fullPath = Path.Combine(path, fileName + ".json");

            if (system.SaveToFile(data, fullPath))
            {
                EditorUtility.DisplayDialog("Export Successful",
                    $"Grid data exported successfully!\n\nFile: {fileName}.json\nLocation: {path}\nPieces exported: {data.buildPieces.Count}",
                    "OK");

                // Refresh the asset database if saving to Assets folder
                if (path.StartsWith("Assets"))
                {
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Export Failed", "Failed to save grid data to file.", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Export Failed", "Failed to export grid data.", "OK");
        }
    }

    #endregion

    #region Import Methods

    private void LoadPreviewData(GridExportImportSystem system)
    {
        try
        {
            if (!File.Exists(selectedImportFilePath))
            {
                Debug.LogError($"File does not exist: {selectedImportFilePath}");
                previewData = null;
                return;
            }

            string json = File.ReadAllText(selectedImportFilePath);
            previewData = JsonUtility.FromJson<GridExportData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load preview data: {e.Message}");
            previewData = null;
        }
    }

    private void ImportGrid(GridExportImportSystem system)
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
            if (system.LoadGrid(previewData))
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

    private void QuickImportFromPath(GridExportImportSystem system, string basePath)
    {
        if (!Directory.Exists(basePath))
        {
            EditorUtility.DisplayDialog("Path Not Found", $"Directory does not exist: {basePath}", "OK");
            return;
        }

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
            menu.AddItem(new GUIContent(fileNames[i]), false, () =>
            {
                selectedImportFilePath = filePath;
                LoadPreviewData(system);
            });
        }
        menu.ShowAsContext();
    }

    #endregion
}
#endif