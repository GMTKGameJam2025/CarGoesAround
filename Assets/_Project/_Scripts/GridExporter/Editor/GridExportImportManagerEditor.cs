using System.IO;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(GridExportImportManager))]
public class GridExportImportManagerEditor : Editor
{
    private string fileName = "GridData";
    private string exportPath = "";
    private string importFilePath = "";
    private GridExportData previewData;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridExportImportManager manager = (GridExportImportManager)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Grid Export/Import Manager", EditorStyles.boldLabel);
        
        // Export Section
        EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("File Name:", GUILayout.Width(70));
        fileName = EditorGUILayout.TextField(fileName);
        EditorGUILayout.EndHorizontal();
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Export Grid", GUILayout.Height(25)))
        {
            string path = EditorUtility.SaveFilePanel("Save Grid Data", "Assets", fileName, "json");
            if (!string.IsNullOrEmpty(path))
            {
                ExportGrid(manager, path);
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        
        // Import Section
        EditorGUILayout.LabelField("Import", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("File:", GUILayout.Width(35));
        EditorGUILayout.LabelField(string.IsNullOrEmpty(importFilePath) ? "No file selected" : Path.GetFileName(importFilePath), EditorStyles.helpBox);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("Select Grid Data", "Assets", "json");
            if (!string.IsNullOrEmpty(path))
            {
                importFilePath = path;
                LoadPreviewData(manager);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        if (previewData != null)
        {
            EditorGUILayout.LabelField($"Preview: {previewData.buildPieces.Count} pieces, Grid: {previewData.gridSize}", EditorStyles.miniLabel);
        }
        
        GUI.backgroundColor = Color.cyan;
        GUI.enabled = !string.IsNullOrEmpty(importFilePath);
        if (GUILayout.Button("Import Grid", GUILayout.Height(25)))
        {
            ImportGrid(manager);
        }
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
        
        if (previewData != null)
        {
            EditorGUILayout.HelpBox("⚠️ Import will clear existing grid data!", MessageType.Warning);
        }
    }
    
    private void ExportGrid(GridExportImportManager manager, string path)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Export Error", "Export only works in Play Mode!", "OK");
            return;
        }
        
        GridExportData data = manager.exporter.ExportGrid();
        if (data != null && manager.exporter.SaveToFile(data, path))
        {
            EditorUtility.DisplayDialog("Export Successful", $"Exported {data.buildPieces.Count} pieces!", "OK");
        }
    }
    
    private void LoadPreviewData(GridExportImportManager manager)
    {
        previewData = manager.exporter.LoadFromFile(importFilePath);
    }
    
    private void ImportGrid(GridExportImportManager manager)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Import Error", "Import only works in Play Mode!", "OK");
            return;
        }
        
        if (EditorUtility.DisplayDialog("Confirm Import", $"Import {previewData.buildPieces.Count} pieces?", "Import", "Cancel"))
        {
            if (manager.loader.LoadGrid(previewData))
            {
                EditorUtility.DisplayDialog("Import Successful", $"Imported {previewData.buildPieces.Count} pieces!", "OK");
            }
        }
    }
}