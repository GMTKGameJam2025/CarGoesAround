/// <summary>
/// Editor-only helper that converts legacy JSON grid exports into
/// <see cref="GridLevelDataSO"/> ScriptableObject assets.
/// Not compiled into builds — all code is wrapped in UNITY_EDITOR.
/// </summary>
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public static class GridLevelConverter
{
    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Opens a file-picker, reads the selected JSON file, converts it to a
    /// <see cref="GridLevelDataSO"/> and saves it alongside the source JSON.
    /// </summary>
    /// <param name="system">The system whose save folder and exporter are used.</param>
    public static void ConvertJsonToScriptableObject(GridExportImportSystem system)
    {
        string jsonPath = EditorUtility.OpenFilePanel("Select legacy JSON export", Application.dataPath, "json");
        if (string.IsNullOrEmpty(jsonPath)) return;

        try
        {
            GridLevelDataSO levelData = ReadJsonIntoScriptableObject(jsonPath);
            string assetName = Path.GetFileNameWithoutExtension(jsonPath) + ".asset";
            system.SaveScriptableObjectToFile(levelData, assetName);
            Debug.Log($"[GridLevelConverter] Converted '{jsonPath}' → '{assetName}'.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[GridLevelConverter] Conversion failed: {e.Message}");
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static GridLevelDataSO ReadJsonIntoScriptableObject(string jsonPath)
    {
        string json = File.ReadAllText(jsonPath);
        GridExportDataLegacy legacy = JsonUtility.FromJson<GridExportDataLegacy>(json);

        GridLevelDataSO levelData = ScriptableObject.CreateInstance<GridLevelDataSO>();
        levelData.levelName   = Path.GetFileNameWithoutExtension(jsonPath);
        levelData.gridSize    = legacy.gridSize;
        levelData.cellSize    = legacy.cellSize;
        levelData.gridOrigin  = legacy.gridOrigin;
        levelData.buildPieces = legacy.buildPieces;
        return levelData;
    }
}
#endif
