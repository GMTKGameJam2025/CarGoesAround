Using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
[CustomEditor(typeof(GridExportImportSystem))]
public class GridExportImportSystemEditor : Editor
{
    private string levelName = "GridLevel";
    private string selectedExportPath = "";
    private GridLevelDataSO selectedImportLevel;
    private GridLevelDataSO previewData;
    
    // Tabs
    private int selectedTab = 0;
    private readonly string[] tabNames = { "Export", "Import", "ScriptableObjects" };
    
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
            case 2:
                DrawScriptableObjectTab(system);
                break;
        }
        
        EditorGUILayout.Space(10);
        DrawValidationSection(system);
    }
    
    private void DrawExportTab(GridExportImportSystem system)
    {
        EditorGUILayout.LabelField("Grid Export to ScriptableObject", EditorStyles.boldLabel);
        
        // Level name input
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Level Name:", GUILayout.Width(80));
        levelName = EditorGUILayout.TextField(levelName);
        EditorGUILayout.EndHorizontal();
        
        // Export folder path
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save Folder:", GUILayout.Width(80));
        string currentPath = string.IsNullOrEmpty(selectedExportPath) ? "Assets/GridLevels/" : selectedExportPath;
        EditorGUILayout.LabelField(currentPath, EditorStyles.helpBox);
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
        if (GUILayout.Button("Export Grid to ScriptableObject", GUILayout.Height(30)))
        {
            ExportToScriptableObject(system);
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        
        // Quick export buttons
        EditorGUILayout.LabelField("Quick Export Options:", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Export to Default Folder"))
        {
            ExportToDefaultFolder(system);
        }
        
        if (GUILayout.Button("Export & Set as Current"))
        {
            ExportAndSetAsCurrent(system);
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawImportTab(GridExportImportSystem system)
    {
        EditorGUILayout.LabelField("Grid Import from ScriptableObject", EditorStyles.boldLabel);
        
        // ScriptableObject selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Import Level:", GUILayout.Width(80));
        selectedImportLevel = (GridLevelDataSO)EditorGUILayout.ObjectField(
            selectedImportLevel, 
            typeof(GridLevelDataSO), 
            false
        );
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Preview information
        if (selectedImportLevel != null)
        {
            EditorGUILayout.LabelField("Level Preview:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Level Name: {selectedImportLevel.levelName}");
            EditorGUILayout.LabelField($"Grid Size: {selectedImportLevel.gridSize.x} x {selectedImportLevel.gridSize.y}");
            EditorGUILayout.LabelField($"Cell Size: {selectedImportLevel.cellSize}");
            EditorGUILayout.LabelField($"Build Pieces: {selectedImportLevel.buildPieces.Count}");
            EditorGUILayout.LabelField($"Grid Origin: {selectedImportLevel.gridOrigin}");
            EditorGUILayout.LabelField($"Created: {selectedImportLevel.creationDate}");
            EditorGUILayout.LabelField($"Modified: {selectedImportLevel.lastModified}");
            
            if (!string.IsNullOrEmpty(selectedImportLevel.description))
            {
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(selectedImportLevel.description, EditorStyles.wordWrappedLabel);
            }
            
            EditorGUILayout.Space(5);
            
            // Show piece breakdown
            if (selectedImportLevel.buildPieces.Count > 0)
            {
                EditorGUILayout.LabelField("Piece Breakdown:", EditorStyles.miniLabel);
                var pieceGroups = selectedImportLevel.buildPieces.GroupBy(p => p.pieceId);
                foreach (var group in pieceGroups)
                {
                    EditorGUILayout.LabelField($" ID {group.Key}: {group.Count()} pieces", EditorStyles.miniLabel);
                }
            }
        }
        
        EditorGUILayout.Space(5);
        
        // Import button
        GUI.backgroundColor = Color.cyan;
        GUI.enabled = selectedImportLevel != null;
        if (GUILayout.Button("Import Grid Data", GUILayout.Height(30)))
        {
            ImportFromScriptableObject(system);
        }
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        
        // Quick import buttons
        EditorGUILayout.LabelField("Quick Import Options:", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Browse GridLevels Folder"))
        {
            BrowseGridLevelsFolder(system);
        }
        
        if (GUILayout.Button("Import from Current Level"))
        {
            ImportCurrentLevel(system);
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Warning about clearing existing data
        if (selectedImportLevel != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("⚠️ WARNING: Importing will clear all existing grid data!", MessageType.Warning);
        }
    }
    
    private void DrawScriptableObjectTab(GridExportImportSystem system)
    {
        EditorGUILayout.LabelField("ScriptableObject Management", EditorStyles.boldLabel);
        
        // Current level display
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Current Level:", GUILayout.Width(90));
        GridLevelDataSO currentLevel = system.GetCurrentLevel();
        EditorGUILayout.ObjectField(currentLevel, typeof(GridLevelDataSO), false);
        EditorGUILayout.EndHorizontal();
        
        if (currentLevel != null)
        {
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField($"Level: {currentLevel.levelName}");
            EditorGUILayout.LabelField($"Pieces: {currentLevel.pieceCount}");
            EditorGUILayout.LabelField($"Modified: {currentLevel.lastModified}");
        }
        
        EditorGUILayout.Space(10);
        
        // Level management buttons
        EditorGUILayout.LabelField("Level Management:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create New Level"))
        {
            CreateNewLevel();
        }
        
        if (GUILayout.Button("Load Current Level"))
        {
            if (currentLevel != null)
            {
                ImportFromScriptableObject(system, currentLevel);
            }
            else
            {
                EditorUtility.DisplayDialog("No Level", "No current level assigned!", "OK");
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Utility buttons
        EditorGUILayout.LabelField("Utilities:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Find All Grid Levels"))
        {
            FindAllGridLevels();
        }
        
        if (GUILayout.Button("Convert JSON to SO"))
        {
            system.ConvertJSONToScriptableObject();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Auto-load settings
        SerializedProperty autoLoadProp = serializedObject.FindProperty("autoLoadOnStart");
        EditorGUILayout.PropertyField(autoLoadProp, new GUIContent("Auto Load on Start"));
        
        SerializedProperty defaultFolderProp = serializedObject.FindProperty("defaultSaveFolder");
        EditorGUILayout.PropertyField(defaultFolderProp, new GUIContent("Default Save Folder"));
        
        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
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
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Grid operations only work in Play Mode!", MessageType.Warning);
        }
        
        if (!hasErrors)
        {
            EditorGUILayout.HelpBox("All references are properly assigned!", MessageType.Info);
        }
    }
    
    #region Export Methods
    
    private void ExportToScriptableObject(GridExportImportSystem system)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Export Error", 
                "Grid export only works in Play Mode!\nPlease enter Play Mode and try again.", "OK");
            return;
        }
        
        GridLevelDataSO levelData = system.ExportGridToScriptableObject(levelName);
        if (levelData != null)
        {
            string savePath = string.IsNullOrEmpty(selectedExportPath) ? "Assets/GridLevels/" : selectedExportPath;
            string fileName = levelName + ".asset";
            
            // Ensure directory exists
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            
            string fullPath = Path.Combine(savePath, fileName);
            
            // Create the asset
            AssetDatabase.CreateAsset(levelData, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Export Successful", 
                $"Grid level exported successfully!\n\nFile: {fileName}\nLocation: {savePath}\nPieces exported: {levelData.buildPieces.Count}", "OK");
            
            // Select the created asset
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = levelData;
        }
        else
        {
            EditorUtility.DisplayDialog("Export Failed", "Failed to export grid data.", "OK");
        }
    }
    
    private void ExportToDefaultFolder(GridExportImportSystem system)
    {
        selectedExportPath = "Assets/GridLevels/";
        ExportToScriptableObject(system);
    }
    
    private void ExportAndSetAsCurrent(GridExportImportSystem system)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Export Error", 
                "Grid export only works in Play Mode!\nPlease enter Play Mode and try again.", "OK");
            return;
        }
        
        GridLevelDataSO levelData = system.ExportGridToScriptableObject(levelName);
        if (levelData != null)
        {
            // Save to default folder
            if (system.SaveScriptableObjectToFile(levelData, levelName + ".asset"))
            {
                // Set as current level
                SerializedProperty currentLevelProp = serializedObject.FindProperty("currentLevelData");
                currentLevelProp.objectReferenceValue = levelData;
                serializedObject.ApplyModifiedProperties();
                
                EditorUtility.DisplayDialog("Export & Set Successful", 
                    $"Grid level exported and set as current level!\n\nLevel: {levelData.levelName}\nPieces: {levelData.buildPieces.Count}", "OK");
            }
        }
    }
    
    #endregion
    
    #region Import Methods
    
    private void ImportFromScriptableObject(GridExportImportSystem system, GridLevelDataSO levelToImport = null)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Import Error", 
                "Grid import only works in Play Mode!\nPlease enter Play Mode and try again.", "OK");
            return;
        }
        
        GridLevelDataSO levelData = levelToImport ?? selectedImportLevel;
        if (levelData == null)
        {
            EditorUtility.DisplayDialog("Import Error", "No level selected for import!", "OK");
            return;
        }
        
        if (EditorUtility.DisplayDialog("Confirm Import", 
            $"This will clear all existing grid data and import level '{levelData.levelName}' with {levelData.buildPieces.Count} build pieces.\n\nAre you sure you want to continue?", 
            "Import", "Cancel"))
        {
            if (system.LoadFromScriptableObject(levelData))
            {
                EditorUtility.DisplayDialog("Import Successful", 
                    $"Grid level '{levelData.levelName}' imported successfully!\n\nPieces imported: {levelData.buildPieces.Count}", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Import Failed", "Failed to import grid data.", "OK");
            }
        }
    }
    
    private void BrowseGridLev