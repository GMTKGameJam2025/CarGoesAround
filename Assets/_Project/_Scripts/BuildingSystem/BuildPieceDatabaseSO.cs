using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject database that holds every <see cref="BuildPieceData"/> definition
/// available in the game. Assign entries in the Inspector.
/// </summary>
[CreateAssetMenu(fileName = "BuildPieceDatabase", menuName = "Building System/Build Piece Database")]
public class BuildPieceDatabaseSO : ScriptableObject
{
    /// <summary>All build piece definitions registered in this database.</summary>
    public List<BuildPieceData> objectsData;
}

/// <summary>
/// Designer-facing data definition for a single build piece type.
/// Referenced at runtime to initialise a <see cref="GridBuildPiece"/> component.
/// </summary>
[Serializable]
public class BuildPieceData
{
    [field: SerializeField, Tooltip("Human-readable display name.")]
    public string Name { get; private set; }

    [field: SerializeField, Tooltip("Unique numeric identifier used to look up this piece in code.")]
    public int ID { get; private set; }

    [field: SerializeField, Tooltip("Footprint of this piece on the grid (columns × rows).")]
    public Vector2Int Size { get; private set; } = Vector2Int.one;

    [field: SerializeField, Tooltip("Prefab instantiated when the piece is placed.")]
    public GameObject Prefab { get; private set; }

    [field: SerializeField, Tooltip("Optional alternate prefab shown while the player is previewing placement. Falls back to Prefab when null.")]
    public GameObject PreviewPrefab { get; private set; }

    [Tooltip("When true, other pieces may be placed on top of this one.")]
    public bool canBuildOnTop;

    [Tooltip("When false, the piece is purely cosmetic and is not tracked in the grid data structure.")]
    public bool storeThisToGrid = true;

    [Tooltip("When false, the player cannot remove this piece once placed.")]
    public bool canBeRemovedFromGrid = true;

    [Tooltip("The layer this piece occupies once placed.")]
    public BuildLayer layer = BuildLayer.Ground;

    [Tooltip("Which layers this piece is allowed to be placed on top of.")]
    public BuildLayer canBeBuiltOnLayers = BuildLayer.Ground;
}

/// <summary>
/// Bitmask enum that categorises the build layers pieces can occupy or require beneath them.
/// Multiple flags may be combined.
/// </summary>
[Flags]
public enum BuildLayer
{
    None        = 0,
    Ground      = 1 << 0,
    Pathway     = 1 << 1,
    Decoration  = 1 << 2,
    Obstacle    = 1 << 3,
    // Add more as needed (up to 32 bits total).
}
