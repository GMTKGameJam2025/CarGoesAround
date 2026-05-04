using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws the "Manage" tab and the shared validation section inside
/// <see cref="GridExportImportSystemEditor"/>.
/// Handles ScriptableObject browsing, duplication, deletion, and grid validation.
/// </summary>
public class GridSOManagerEditorTab
{
    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private readonly GridExportImportSystem _system;
    private Vector2 _scrollPosition;
    private List<GridLevelDataSO> _cachedLevels;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public GridSOManagerEditorTab(GridExportImportSystem system)
    {
        _system = system;
    }

    // -------------------------------------------------------------------------
    // Manage tab
    // -------------------------------------------------------------------------

    public void Draw()
    {
        EditorGUILayout.LabelField("ScriptableObject Management", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        DrawCurrentLevelInfo();
        EditorGUILayout.Space(6);
        DrawLevelListActions();
        EditorGUILayout.Space(4);
        DrawLevelList();
    }

    // -------------------------------------------------------------------------
    // Validation section (called from editor after all tabs)
    // -------------------------------------------------------------------------

    public void DrawValidationSection()
    {
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

        GUI.backgroundColor = new Color(1f, 0.85f, 0.3f);
        if (GUILayout.Button("Validate Grid State"))
            RunValidation();
        GUI.backgroundColor = Color.white;
    }

    // -------------------------------------------------------------------------
    // Current level info
    // -------------------------------------------------------------------------

    private void DrawCurrentLevelInfo()
    {
        EditorGUILayout.LabelField("Current Level", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Asset", GUILayout.Width(40));
        EditorGUILayout.ObjectField(_system.GetCurrentLevel(), typeof(GridLevelDataSO), false);
        EditorGUILayout.EndHorizontal();

        GridLevelDataSO current = _system.GetCurrentLevel();
        if (current != null)
        {
            EditorGUILayout.LabelField($"Name: {current.levelName}  |  Pieces: {current.pieceCount}  |  Modified: {current.lastModified}",
                EditorStyles.miniLabel);
        }
    }

    // -------------------------------------------------------------------------
    // Level list
    // -------------------------------------------------------------------------

    private void DrawLevelListActions()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Refresh List"))
            RefreshLevelCache();

        if (GUILayout.Button("Open Levels Folder"))
            OpenLevelsFolder();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawLevelList()
    {
        if (_cachedLevels == null)
            RefreshLevelCache();

        if (_cachedLevels.Count == 0)
        {
            EditorGUILayout.HelpBox("No GridLevelDataSO assets found in the save folder.", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField($"Found {_cachedLevels.Count} level(s)", EditorStyles.miniLabel);
        EditorGUILayout.Space(2);

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(160));

        foreach (GridLevelDataSO level in _cachedLevels)
            DrawLevelRow(level);

        EditorGUILayout.EndScrollView();
    }

    private void DrawLevelRow(GridLevelDataSO level)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"{level.levelName}  ({level.pieceCount} pieces)", GUILayout.ExpandWidth(true));

        if (GUILayout.Button("Set", GUILayout.Width(34)))
        {
            _system.SetCurrentLevel(level);
            EditorUtility.SetDirty(_system);
        }

        if (GUILayout.Button("Load", GUILayout.Width(40)))
            _system.LoadFromScriptableObject(level);

        if (GUILayout.Button("Dupe", GUILayout.Width(40)))
            DuplicateLevel(level);

        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("Del", GUILayout.Width(34)))
            DeleteLevel(level);
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();
    }

    // -------------------------------------------------------------------------
    // Actions
    // -------------------------------------------------------------------------

    private void RefreshLevelCache()
    {
        _cachedLevels = new List<GridLevelDataSO>();
        string folder = _system.defaultSaveFolder;

        if (!Directory.Exists(folder)) return;

        string[] guids = AssetDatabase.FindAssets("t:GridLevelDataSO", new[] { folder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<GridLevelDataSO>(path);
            if (asset != null) _cachedLevels.Add(asset);
        }

        _cachedLevels = _cachedLevels.OrderBy(l => l.levelName).ToList();
    }

    private void OpenLevelsFolder()
    {
        string folder = _system.defaultSaveFolder;
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        EditorUtility.RevealInFinder(folder);
    }

    private void DuplicateLevel(GridLevelDataSO level)
    {
        string srcPath  = AssetDatabase.GetAssetPath(level);
        string destPath = srcPath.Replace(".asset", "_Copy.asset");

        if (AssetDatabase.CopyAsset(srcPath, destPath))
        {
            AssetDatabase.Refresh();
            RefreshLevelCache();
            Debug.Log($"[GridSOManager] Duplicated '{level.levelName}' → '{destPath}'.");
        }
    }

    private void DeleteLevel(GridLevelDataSO level)
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Delete Level",
            $"Permanently delete '{level.levelName}'?\nThis cannot be undone.",
            "Delete", "Cancel");

        if (!confirmed) return;

        string path = AssetDatabase.GetAssetPath(level);
        AssetDatabase.DeleteAsset(path);
        RefreshLevelCache();
        Debug.Log($"[GridSOManager] Deleted '{path}'.");
    }

    // -------------------------------------------------------------------------
    // Validation
    // -------------------------------------------------------------------------

    private void RunValidation()
    {
        if (_system.gridManager?.Grid == null)
        {
            EditorUtility.DisplayDialog("Validation", "Grid is not initialised — enter Play mode first.", "OK");
            return;
        }

        int width   = _system.gridManager.Grid.Width;
        int height  = _system.gridManager.Grid.Height;
        int total   = 0;
        int empties = 0;
        int stacked = 0;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridCell cell = _system.gridManager.Grid.GetGridObject(x, z);
                if (cell == null) continue;

                GridBuildPiece top = cell.GetTopGridObject();
                if (top == null) { empties++; continue; }

                total++;
                if (top.GridObjectsOnTop != null && top.GridObjectsOnTop.Count > 0)
                    stacked++;
            }
        }

        string report =
            $"Grid: {width} x {height}\n" +
            $"Occupied cells: {total}\n" +
            $"Empty cells:    {empties}\n" +
            $"Cells with stack: {stacked}";

        EditorUtility.DisplayDialog("Grid Validation", report, "OK");
        Debug.Log($"[GridSOManager] Validation:\n{report}");
    }
}
