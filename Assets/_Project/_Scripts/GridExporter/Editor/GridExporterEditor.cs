using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(GridExporter))]
public class GridExporterEditor : Editor
{
    private string fileName = "GridData";
    private string selectedPath = "";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridExporter exporter = (GridExporter)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Grid Export Tools", EditorStyles.boldLabel);
        
        // File name input
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("File Name:", GUILayout.Width(70));
        fileName = EditorGUILayout.TextField(fileName);
        EditorGUILayout.EndHorizontal();
        
        // Path selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save Path:", GUILayout.Width(70));
        EditorGUILayout.LabelField(string.IsNullOrEmpty(selectedPath) ? "Assets/" : selectedPath, EditorStyles.helpBox);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.SaveFolderPanel("Select Save Location", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert absolute path to relative path
                if (path.StartsWith(Application.dataPath))
                {
                    selectedPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    selectedPath = path;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Export button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Export Grid Data", GUILayout.Height(30)))
        {
            ExportGrid(exporter);
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        
        // Quick export buttons
        EditorGUILayout.LabelField("Quick Export Options:", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Export to Desktop"))
        {
            string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            ExportToPath(exporter, desktopPath);
        }
        
        if (GUILayout.Button("Export to StreamingAssets"))
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath);
            if (!Directory.Exists(streamingPath))
            {
                Directory.CreateDirectory(streamingPath);
            }
            ExportToPath(exporter, streamingPath);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Validation
        if (!exporter.GetComponent<GridExporter>().gridManager)
        {
            EditorGUILayout.HelpBox("GridManager component not found! Please assign it in the inspector.", MessageType.Warning);
        }
        
        if (!exporter.GetComponent<GridExporter>().database)
        {
            EditorGUILayout.HelpBox("BuildPieceDatabaseSO not assigned! Please assign it in the inspector.", MessageType.Warning);
        }
    }
    
    private void ExportGrid(GridExporter exporter)
    {
        string savePath = string.IsNullOrEmpty(selectedPath) ? "Assets" : selectedPath;
        string fullPath = Path.Combine(savePath, fileName + ".json");
        
        ExportToPath(exporter, savePath);
    }
    
    private void ExportToPath(GridExporter exporter, string path)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Export Error", "Grid export only works in Play Mode!\nPlease enter Play Mode and try again.", "OK");
            return;
        }
        
        GridExportData data = exporter.ExportGrid();
        if (data != null)
        {
            string fullPath = Path.Combine(path, fileName + ".json");
            
            if (exporter.SaveToFile(data, fullPath))
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
}

// Combined Editor for when both components are on the same GameObject

// Optional: Combined component for easier management