using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum BuildModeType
{
    None, 
    Add, 
    Remove
}

public class BuildingManager : MonoBehaviour
{
    public GridManager gridManager;
    public GridBuildPiece selectedBuildPiece;

    public BuildModeType currentMode = BuildModeType.None;
    
    public PreviewSystem previewSystem;
    
    private void Update()
    {
        
    }
}