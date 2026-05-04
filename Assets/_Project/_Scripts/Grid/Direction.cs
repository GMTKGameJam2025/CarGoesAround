using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cardinal directions used for grid placement and rotation.
/// </summary>
public enum Direction
{
    Down,
    Left,
    Up,
    Right,
}

/// <summary>
/// Extension methods for rotating and offsetting grid objects by <see cref="Direction"/>.
/// </summary>
public static class DirectionHelper
{
    /// <summary>Returns the next clockwise direction in the cycle Down → Left → Up → Right → Down.</summary>
    public static Direction GetNextDirection(this Direction dir)
    {
        return dir switch
        {
            Direction.Down  => Direction.Left,
            Direction.Left  => Direction.Up,
            Direction.Up    => Direction.Right,
            Direction.Right => Direction.Down,
            _               => Direction.Down,
        };
    }

    /// <summary>Returns the Y-axis rotation angle (degrees) that corresponds to <paramref name="dir"/>.</summary>
    public static int GetDirectionRotation(this Direction dir)
    {
        return dir switch
        {
            Direction.Down  => 0,
            Direction.Left  => 90,
            Direction.Up    => 180,
            Direction.Right => 270,
            _               => 0,
        };
    }

    /// <summary>
    /// Returns the world-space offset that must be added to a placed object's position
    /// so that it lines up correctly when rotated to <paramref name="dir"/>.
    /// </summary>
    public static Vector2Int GetRotationOffset(this Direction dir, Vector2Int size)
    {
        return dir switch
        {
            Direction.Down  => new Vector2Int(0,       0),
            Direction.Left  => new Vector2Int(0,       size.x),
            Direction.Up    => new Vector2Int(size.x,  size.y),
            Direction.Right => new Vector2Int(size.y,  0),
            _               => Vector2Int.zero,
        };
    }
}

/// <summary>
/// Grid-related utility and extension methods.
/// </summary>
public static class GridHelper
{
    /// <summary>
    /// Returns <c>true</c> when <paramref name="newLayer"/> is compatible with
    /// <paramref name="baseLayer"/> (i.e. at least one flag is shared).
    /// </summary>
    public static bool CanBuildOnLayer(BuildLayer baseLayer, BuildLayer newLayer)
        => (baseLayer & newLayer) != 0;

    /// <summary>
    /// Returns <c>true</c> when <paramref name="newPiece"/> is allowed to be placed
    /// on top of <paramref name="basePiece"/> according to layer flags.
    /// </summary>
    public static bool CanBuildOnLayer(GridBuildPiece basePiece, GridBuildPiece newPiece)
        => (newPiece.CanBeBuiltOnLayers & basePiece.Layer) != 0;

    /// <summary>
    /// Enumerates every grid cell covered by an object of <paramref name="size"/> placed at
    /// <paramref name="startPosition"/> facing <paramref name="dir"/>.
    /// </summary>
    public static List<Vector2Int> GetGridPositionList(this Vector2Int startPosition, Vector2Int size, Direction dir)
    {
        List<Vector2Int> list = new();

        // Swap axes when the piece is rotated 90 / 270 degrees.
        int xSteps = (dir == Direction.Left || dir == Direction.Right) ? size.y : size.x;
        int ySteps = (dir == Direction.Left || dir == Direction.Right) ? size.x : size.y;

        for (int x = 0; x < xSteps; x++)
            for (int y = 0; y < ySteps; y++)
                list.Add(startPosition + new Vector2Int(x, y));

        return list;
    }
}
