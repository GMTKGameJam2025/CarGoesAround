using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LevelDataSO : ScriptableObject
{
    public List<LevelData> allLevelData = new List<LevelData>();
}

[Serializable]
public class LevelData
{
    public int levelNumber;
    public string subtitle;
    public GridLevelDataSO gridData;
    public InventoryPreset inventoryPreset;
    public string sceneName;
}