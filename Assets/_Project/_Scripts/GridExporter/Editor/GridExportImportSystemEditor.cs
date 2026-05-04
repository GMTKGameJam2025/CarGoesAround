using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Inspector for <see cref="GridExportImportSystem"/>.
/// Renders the default fields then delegates each tab to a dedicated helper class:
/// <list type="bullet">
///   <item><see cref="GridExporterEditorTab"/> — export controls</item>
///   <item><see cref="GridImporterEditorTab"/> — import controls</item>
///   <item><see cref="GridSOManagerEditorTab"/> — SO management and validation</item>
/// </list>
/// </summary>
[CustomEditor(typeof(GridExportImportSystem))]
public class GridExportImportSystemEditor : Editor
{
    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private int _selectedTab;
    private static readonly string[] TabNames = { "Export", "Import", "Manage" };

    private GridExporterEditorTab  _exportTab;
    private GridImporterEditorTab  _importTab;
    private GridSOManagerEditorTab _manageTab;

    // -------------------------------------------------------------------------
    // Unity callbacks
    // -------------------------------------------------------------------------

    private void OnEnable()
    {
        var system = (GridExportImportSystem)target;
        _exportTab  = new GridExporterEditorTab(system);
        _importTab  = new GridImporterEditorTab(system);
        _manageTab  = new GridSOManagerEditorTab(system);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        _selectedTab = GUILayout.Toolbar(_selectedTab, TabNames);
        EditorGUILayout.Space(5);

        var system = (GridExportImportSystem)target;

        switch (_selectedTab)
        {
            case 0: _exportTab.Draw();  break;
            case 1: _importTab.Draw();  break;
            case 2: _manageTab.Draw();  break;
        }

        EditorGUILayout.Space(10);
        _manageTab.DrawValidationSection();
    }
}
