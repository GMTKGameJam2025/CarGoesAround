using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws the "Import" tab inside the <see cref="GridExportImportSystemEditor"/> inspector.
/// Lets the user pick a <see cref="GridLevelDataSO"/>, preview its contents, and load it.
/// </summary>
public class GridImporterEditorTab
{
    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private readonly GridExportImportSystem _system;
    private GridLevelDataSO _selectedLevel;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public GridImporterEditorTab(GridExportImportSystem system)
    {
        _system = system;
    }

    // -------------------------------------------------------------------------
    // Drawing
    // -------------------------------------------------------------------------

    public void Draw()
    {
        EditorGUILayout.LabelField("Import Grid ← ScriptableObject", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        DrawLevelPicker();
        EditorGUILayout.Space(4);

        if (_selectedLevel != null)
        {
            DrawLevelPreview();
            EditorGUILayout.Space(4);
        }

        DrawImportButton();
        EditorGUILayout.Space(4);
        DrawQuickImportButtons();

        if (_selectedLevel != null)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox("WARNING: Importing will clear all existing grid data.", MessageType.Warning);
        }
    }

    // -------------------------------------------------------------------------
    // Field helpers
    // -------------------------------------------------------------------------

    private void DrawLevelPicker()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Import Level", GUILayout.Width(85));
        _selectedLevel = (GridLevelDataSO)EditorGUILayout.ObjectField(
            _selectedLevel, typeof(GridLevelDataSO), false);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLevelPreview()
    {
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Name:     {_selectedLevel.levelName}");
        EditorGUILayout.LabelField($"Grid:     {_selectedLevel.gridSize.x} x {_selectedLevel.gridSize.y}  |  Cell size: {_selectedLevel.cellSize}");
        EditorGUILayout.LabelField($"Pieces:   {_selectedLevel.buildPieces.Count}");
        EditorGUILayout.LabelField($"Origin:   {_selectedLevel.gridOrigin}");
        EditorGUILayout.LabelField($"Created:  {_selectedLevel.creationDate}");
        EditorGUILayout.LabelField($"Modified: {_selectedLevel.lastModified}");

        if (!string.IsNullOrEmpty(_selectedLevel.description))
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(_selectedLevel.description, EditorStyles.wordWrappedLabel);
        }

        DrawPieceBreakdown();
    }

    private void DrawPieceBreakdown()
    {
        if (_selectedLevel.buildPieces.Count == 0) return;

        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Piece breakdown", EditorStyles.miniLabel);

        var groups = _selectedLevel.buildPieces.GroupBy(p => p.pieceId).OrderBy(g => g.Key);
        foreach (var group in groups)
            EditorGUILayout.LabelField($"  ID {group.Key}: {group.Count()} piece(s)", EditorStyles.miniLabel);
    }

    // -------------------------------------------------------------------------
    // Button helpers
    // -------------------------------------------------------------------------

    private void DrawImportButton()
    {
        GUI.backgroundColor = Color.cyan;
        GUI.enabled         = _selectedLevel != null;

        if (GUILayout.Button("Import Grid Data", GUILayout.Height(30)))
            ImportSelected();

        GUI.enabled         = true;
        GUI.backgroundColor = Color.white;
    }

    private void DrawQuickImportButtons()
    {
        EditorGUILayout.LabelField("Quick Import", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Browse GridLevels Folder"))
            BrowseAndSelect();

        if (GUILayout.Button("Load Current Level"))
            ImportCurrentLevel();

        EditorGUILayout.EndHorizontal();
    }

    // -------------------------------------------------------------------------
    // Actions
    // -------------------------------------------------------------------------

    private void ImportSelected()
    {
        if (_selectedLevel == null) return;

        if (EditorUtility.DisplayDialog(
            "Import Grid",
            $"This will clear the current grid and load '{_selectedLevel.levelName}'.\n\nContinue?",
            "Import", "Cancel"))
        {
            _system.LoadFromScriptableObject(_selectedLevel);
        }
    }

    private void ImportCurrentLevel()
    {
        GridLevelDataSO current = _system.GetCurrentLevel();
        if (current == null)
        {
            EditorUtility.DisplayDialog("Import", "No current level is assigned on the component.", "OK");
            return;
        }

        _selectedLevel = current;
        ImportSelected();
    }

    private void BrowseAndSelect()
    {
        string path = EditorUtility.OpenFilePanel("Select GridLevelDataSO", "Assets/GridLevels", "asset");
        if (string.IsNullOrEmpty(path)) return;

        // Convert to project-relative path.
        if (path.StartsWith(Application.dataPath))
            path = "Assets" + path.Substring(Application.dataPath.Length);

        var asset = AssetDatabase.LoadAssetAtPath<GridLevelDataSO>(path);
        if (asset != null)
            _selectedLevel = asset;
        else
            EditorUtility.DisplayDialog("Import", "Selected file is not a GridLevelDataSO asset.", "OK");
    }
}
