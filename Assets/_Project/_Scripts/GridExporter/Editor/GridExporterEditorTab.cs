using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws the "Export" tab inside the <see cref="GridExportImportSystemEditor"/> inspector.
/// Provides controls for naming, choosing a destination folder, and triggering an export.
/// </summary>
public class GridExporterEditorTab
{
    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private readonly GridExportImportSystem _system;
    private string _levelName        = "GridLevel";
    private string _selectedFolder   = "";

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public GridExporterEditorTab(GridExportImportSystem system)
    {
        _system = system;
    }

    // -------------------------------------------------------------------------
    // Drawing
    // -------------------------------------------------------------------------

    public void Draw()
    {
        EditorGUILayout.LabelField("Export Grid → ScriptableObject", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        DrawLevelNameField();
        DrawFolderPicker();
        EditorGUILayout.Space(6);
        DrawPrimaryExportButton();
        EditorGUILayout.Space(4);
        DrawQuickExportButtons();
        EditorGUILayout.Space(6);
        DrawConvertJsonButton();
    }

    // -------------------------------------------------------------------------
    // Field helpers
    // -------------------------------------------------------------------------

    private void DrawLevelNameField()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Level Name", GUILayout.Width(80));
        _levelName = EditorGUILayout.TextField(_levelName);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawFolderPicker()
    {
        string display = string.IsNullOrEmpty(_selectedFolder)
            ? _system.defaultSaveFolder
            : _selectedFolder;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save Folder", GUILayout.Width(80));
        EditorGUILayout.LabelField(display, EditorStyles.helpBox);

        if (GUILayout.Button("Browse", GUILayout.Width(60)))
            _selectedFolder = BrowseForFolder();

        EditorGUILayout.EndHorizontal();
    }

    // -------------------------------------------------------------------------
    // Button helpers
    // -------------------------------------------------------------------------

    private void DrawPrimaryExportButton()
    {
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Export Grid to ScriptableObject", GUILayout.Height(30)))
            ExportToScriptableObject();
        GUI.backgroundColor = Color.white;
    }

    private void DrawQuickExportButtons()
    {
        EditorGUILayout.LabelField("Quick Export", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Export to Default Folder"))
            ExportToDefaultFolder();

        if (GUILayout.Button("Export & Set as Current"))
            ExportAndSetAsCurrent();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawConvertJsonButton()
    {
        EditorGUILayout.LabelField("Legacy", EditorStyles.miniLabel);
        if (GUILayout.Button("Convert JSON → ScriptableObject"))
            GridLevelConverter.ConvertJsonToScriptableObject(_system);
    }

    // -------------------------------------------------------------------------
    // Actions
    // -------------------------------------------------------------------------

    private void ExportToScriptableObject()
    {
        GridLevelDataSO levelData = _system.ExportGridToScriptableObject(_levelName);
        if (levelData == null) return;

        string folder = string.IsNullOrEmpty(_selectedFolder) ? null : _selectedFolder;
        string file   = _levelName + ".asset";

        // Temporarily override the save folder if a custom one was chosen.
        string originalFolder = _system.defaultSaveFolder;
        if (!string.IsNullOrEmpty(_selectedFolder))
            _system.defaultSaveFolder = _selectedFolder;

        _system.SaveScriptableObjectToFile(levelData, file);
        _system.defaultSaveFolder = originalFolder;
    }

    private void ExportToDefaultFolder()
    {
        GridLevelDataSO levelData = _system.ExportGridToScriptableObject(_levelName);
        if (levelData != null)
            _system.SaveScriptableObjectToFile(levelData, _levelName + ".asset");
    }

    private void ExportAndSetAsCurrent()
    {
        GridLevelDataSO levelData = _system.ExportGridToScriptableObject(_levelName);
        if (levelData == null) return;

        _system.SaveScriptableObjectToFile(levelData, _levelName + ".asset");
        _system.SetCurrentLevel(levelData);
        EditorUtility.SetDirty(_system);
    }

    // -------------------------------------------------------------------------
    // Utilities
    // -------------------------------------------------------------------------

    private static string BrowseForFolder()
    {
        string raw = EditorUtility.SaveFolderPanel("Select Save Folder", "Assets", "");
        if (string.IsNullOrEmpty(raw)) return "";

        // Convert absolute path to project-relative.
        return raw.StartsWith(UnityEngine.Application.dataPath)
            ? "Assets" + raw.Substring(UnityEngine.Application.dataPath.Length)
            : raw;
    }
}
