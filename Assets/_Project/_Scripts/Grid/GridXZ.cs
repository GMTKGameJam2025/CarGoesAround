using System;
using UnityEngine;

/// <summary>
/// A generic 2-D grid laid out on the XZ plane.
/// Each cell holds one <typeparamref name="TGridObject"/> instance.
/// </summary>
public class GridXZ<TGridObject>
{
    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>Raised whenever a cell's value is changed via <see cref="SetGridObject"/>.</summary>
    public event EventHandler<GridObjectChangedArgs> OnGridObjectChanged;

    /// <summary>Event arguments carrying the grid coordinates of the changed cell.</summary>
    public class GridObjectChangedArgs : EventArgs
    {
        /// <summary>Column (X axis) of the changed cell.</summary>
        public int x;
        /// <summary>Row (Z axis) of the changed cell.</summary>
        public int z;
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>Number of columns along the X axis.</summary>
    public int Width { get; }

    /// <summary>Number of rows along the Z axis.</summary>
    public int Height { get; }

    /// <summary>World-space size of one cell.</summary>
    public float CellSize { get; }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private readonly Vector3 _originPosition;
    private readonly TGridObject[,] _gridArray;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new grid and populates every cell using <paramref name="createGridObject"/>.
    /// </summary>
    /// <param name="width">Number of columns.</param>
    /// <param name="height">Number of rows.</param>
    /// <param name="cellSize">World-space size of a single cell.</param>
    /// <param name="originPosition">World-space position of the (0, 0) corner.</param>
    /// <param name="createGridObject">Factory called once per cell; receives this grid and the cell's (x, z) indices.</param>
    public GridXZ(int width, int height, float cellSize, Vector3 originPosition,
                  Func<GridXZ<TGridObject>, int, int, TGridObject> createGridObject)
    {
        Width          = width;
        Height         = height;
        CellSize       = cellSize;
        _originPosition = originPosition;
        _gridArray      = new TGridObject[width, height];

        for (int x = 0; x < width; x++)
            for (int z = 0; z < height; z++)
                _gridArray[x, z] = createGridObject(this, x, z);
    }

    // -------------------------------------------------------------------------
    // Coordinate helpers
    // -------------------------------------------------------------------------

    /// <summary>Returns the world-space origin (bottom-left corner) of the cell at (<paramref name="x"/>, <paramref name="z"/>).</summary>
    public Vector3 GetWorldPosition(int x, int z)
        => new Vector3(x, 0, z) * CellSize + _originPosition;

    /// <summary>Converts a world-space position to grid indices, written into <paramref name="x"/> and <paramref name="z"/>.</summary>
    public void GetXZ(Vector3 worldPosition, out int x, out int z)
    {
        Vector3 local = worldPosition - _originPosition;
        x = Mathf.FloorToInt(local.x / CellSize);
        z = Mathf.FloorToInt(local.z / CellSize);
    }

    /// <summary>
    /// Returns <c>true</c> when (<paramref name="x"/>, <paramref name="z"/>) is a valid cell inside the grid bounds.
    /// </summary>
    public bool IsInBounds(int x, int z)
        => x >= 0 && z >= 0 && x < Width && z < Height;

    /// <summary>
    /// Returns <c>true</c> when the 2-D position encoded in <paramref name="gridPosition"/> (x, y → x, z)
    /// is inside the grid bounds.
    /// </summary>
    public bool IsInBounds(Vector2Int gridPosition)
        => IsInBounds(gridPosition.x, gridPosition.y);

    // -------------------------------------------------------------------------
    // Cell accessors
    // -------------------------------------------------------------------------

    /// <summary>
    /// Overwrites the value at cell (<paramref name="x"/>, <paramref name="z"/>) and fires
    /// <see cref="OnGridObjectChanged"/>. Out-of-bounds writes are silently ignored.
    /// </summary>
    public void SetGridObject(int x, int z, TGridObject value)
    {
        if (!IsInBounds(x, z)) return;
        _gridArray[x, z] = value;
        TriggerGridObjectChanged(x, z);
    }

    /// <summary>Overload that accepts a world-space position instead of grid indices.</summary>
    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        GetXZ(worldPosition, out int x, out int z);
        SetGridObject(x, z, value);
    }

    /// <summary>
    /// Returns the value stored at cell (<paramref name="x"/>, <paramref name="z"/>),
    /// or <c>default</c> when the position is out of bounds.
    /// </summary>
    public TGridObject GetGridObject(int x, int z)
        => IsInBounds(x, z) ? _gridArray[x, z] : default;

    /// <summary>Overload that accepts a world-space position instead of grid indices.</summary>
    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        GetXZ(worldPosition, out int x, out int z);
        return GetGridObject(x, z);
    }

    /// <summary>
    /// Manually fires <see cref="OnGridObjectChanged"/> for the cell at (<paramref name="x"/>, <paramref name="z"/>).
    /// Useful when a cell's internal state changes without going through <see cref="SetGridObject"/>.
    /// </summary>
    public void TriggerGridObjectChanged(int x, int z)
        => OnGridObjectChanged?.Invoke(this, new GridObjectChangedArgs { x = x, z = z });

    // -------------------------------------------------------------------------
    // Validation helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Clamps <paramref name="gridPosition"/> so that both axes lie within valid grid bounds.
    /// </summary>
    public Vector2Int ValidateGridPosition(Vector2Int gridPosition)
        => new Vector2Int(
            Mathf.Clamp(gridPosition.x, 0, Width  - 1),
            Mathf.Clamp(gridPosition.y, 0, Height - 1));

    /// <summary>
    /// Returns <c>true</c> when the 2-D position encoded in <paramref name="gridPosition"/> (x, y → x, z)
    /// is inside the grid bounds.
    /// </summary>
    /// <remarks>Kept for backwards compatibility — prefer <see cref="IsInBounds(Vector2Int)"/>.</remarks>
    public bool IsGridObjectInGrid(Vector2Int gridPosition)
        => IsInBounds(gridPosition.x, gridPosition.y);
}
