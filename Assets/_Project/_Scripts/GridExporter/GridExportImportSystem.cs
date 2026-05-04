using System.Collections;
using UnityEngine;

/// <summary>
/// MonoBehaviour facade for the grid export/import system.
/// Holds all scene references and Unity events, then delegates the heavy lifting
/// to <see cref="GridExporter"/> (writing) and <see cref="GridImporter"/> (loading).
/// </summary>
public class GridExportImportSystem : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("Core References")]
    public GridManager gridManager;
    public BuildPieceDatabaseSO database;
    public BuildingManager buildingManager;

    [Header("Level Settings")]
    [SerializeField] private GridLevelDataSO currentLevelData;
    public string defaultSaveFolder = "Assets/GridLevels/";
    [SerializeField] private bool autoLoadOnStart = true;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent<GridLevelDataSO> OnGridLoaded;
    public UnityEngine.Events.UnityEvent OnGridLoadFailed;
    public UnityEngine.Events.UnityEvent<GridLevelDataSO> OnGridExported;

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private GridExporter _exporter;
    private GridImporter _importer;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _exporter = new GridExporter(this);
        _importer = new GridImporter(this);
    }

    private void Start()
    {
        if (autoLoadOnStart && currentLevelData != null)
            StartCoroutine(WaitForGridThenLoad());
    }

    /// <summary>Waits until the grid is initialised before triggering a load.</summary>
    private IEnumerator WaitForGridThenLoad()
    {
        while (gridManager?.Grid == null)
            yield return new WaitForEndOfFrame();

        LoadFromScriptableObject(currentLevelData);
    }

    // -------------------------------------------------------------------------
    // Export API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Snapshots the current grid state into a new <see cref="GridLevelDataSO"/> instance.
    /// </summary>
    /// <param name="fileName">Optional asset name; defaults to a timestamped name.</param>
    public GridLevelDataSO ExportGridToScriptableObject(string fileName = null)
        => _exporter.ExportGrid(fileName);

#if UNITY_EDITOR
    /// <summary>Saves <paramref name="levelData"/> to disk as a Unity asset.</summary>
    public bool SaveScriptableObjectToFile(GridLevelDataSO levelData, string fileName = null)
        => _exporter.SaveToFile(levelData, fileName);
#endif

    // -------------------------------------------------------------------------
    // Import API
    // -------------------------------------------------------------------------

    /// <summary>Clears the current grid and rebuilds it from <paramref name="levelData"/>.</summary>
    public bool LoadFromScriptableObject(GridLevelDataSO levelData)
        => _importer.LoadGrid(levelData);

    // -------------------------------------------------------------------------
    // Context-menu shortcuts
    // -------------------------------------------------------------------------

    [ContextMenu("Export Grid to ScriptableObject")]
    public void QuickExportToScriptableObject()
    {
        GridLevelDataSO levelData = ExportGridToScriptableObject();
        if (levelData == null) return;

#if UNITY_EDITOR
        SaveScriptableObjectToFile(levelData);
#else
        Debug.Log("[GridExportImportSystem] ScriptableObject created but cannot be saved outside the Editor.");
#endif
    }

    [ContextMenu("Load Current Level")]
    public void LoadCurrentLevel()
    {
        if (currentLevelData != null)
            LoadFromScriptableObject(currentLevelData);
        else
            Debug.LogWarning("[GridExportImportSystem] No current level data assigned.");
    }

    // -------------------------------------------------------------------------
    // Current level accessors
    // -------------------------------------------------------------------------

    public GridLevelDataSO GetCurrentLevel()              => currentLevelData;
    public void             SetCurrentLevel(GridLevelDataSO levelData) => currentLevelData = levelData;
}
