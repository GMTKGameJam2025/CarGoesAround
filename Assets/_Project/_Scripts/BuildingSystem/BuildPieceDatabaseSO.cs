using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[CreateAssetMenu]
public class BuildPieceDatabaseSO : ScriptableObject
{
    public List<BuildPieceData> objectsData;
}

[Serializable]
public class BuildPieceData
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public Vector2Int Size { get; private set; } = Vector2Int.one;
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public GameObject PreviewPrefab { get; private set; }

    public bool canBuildOnTop = false;
    public bool storeThisToGrid = true;

    public BuildLayer layer = BuildLayer.Ground;
    public BuildLayer canBeBuiltOnLayers = BuildLayer.Ground;
}

[System.Flags]
public enum BuildLayer
{
    None = 0,
    Ground = 1 << 0,
    Pathway = 1 << 1,
    Decoration = 1 << 2,
    Obstacle = 1 << 3,
    // Add more as needed (up to 32 bits)
}