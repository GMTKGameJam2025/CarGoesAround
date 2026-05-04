using UnityEngine;

/// <summary>
/// Active building state: shows a placement preview and places a build piece
/// on click after validating grid position, layer compatibility, and inventory.
/// </summary>
public class BuildState : IBuildingState
{
    private readonly int _id;
    private readonly BuildPieceData _piece;
    private readonly BuildingManager _buildingManager;
    private readonly GridManager _gridManager;
    private readonly InventoryManager _inventoryManager;
    private readonly PreviewSystem _previewSystem;
    private readonly SoundFeedback _soundFeedback;

    private Direction _currentDirection = Direction.Down;
    private float _currentRotation;

    /// <param name="id">Database ID of the piece to place.</param>
    /// <param name="pieceDatabase">Database to look up the piece definition.</param>
    /// <param name="buildingManager">Used to spawn / destroy pieces.</param>
    /// <param name="inventoryManager">Optional inventory check before placement.</param>
    /// <param name="gridManager">Grid to validate and record placement.</param>
    /// <param name="previewSystem">Handles ghost-preview rendering.</param>
    /// <param name="soundFeedback">Plays audio cues.</param>
    public BuildState(
        int id,
        BuildPieceDatabaseSO pieceDatabase,
        BuildingManager buildingManager,
        InventoryManager inventoryManager,
        GridManager gridManager,
        PreviewSystem previewSystem,
        SoundFeedback soundFeedback)
    {
        _id               = id;
        _piece            = pieceDatabase.objectsData.Find(p => p.ID == id);
        _buildingManager  = buildingManager;
        _gridManager      = gridManager;
        _inventoryManager = inventoryManager;
        _previewSystem    = previewSystem;
        _soundFeedback    = soundFeedback;

        previewSystem.StartShowingPlacementPreview(
            _piece.PreviewPrefab ? _piece.PreviewPrefab : _piece.Prefab,
            _piece.Size);
    }

    /// <inheritdoc/>
    public void EndState() => _previewSystem.StopShowingPreview();

    /// <inheritdoc/>
    public void OnAction(Vector3Int gridPosition)
    {
        if (!CheckPlacementValidity(gridPosition, _piece.Size, _currentDirection))
        {
            _soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }

        if (_inventoryManager != null)
        {
            if (!_inventoryManager.HasEnoughItems(_id) || !_inventoryManager.ConsumeItems(_id))
            {
                _soundFeedback.PlaySound(SoundType.wrongPlacement);
                return;
            }
        }

        Vector2Int offset        = _currentDirection.GetRotationOffset(_piece.Size);
        Vector3    worldPosition = _gridManager.Grid.GetWorldPosition(gridPosition.x, gridPosition.y)
                                   + new Vector3(offset.x, 0, offset.y);

        GridBuildPiece piece = _buildingManager.CreateBuildPiece(
            _piece,
            worldPosition,
            Quaternion.Euler(0, _currentRotation, 0));

        // Attach and start the drop-in animation.
        if (!piece.TryGetComponent(out GridPlacementAnimator animator))
            animator = piece.gameObject.AddComponent<GridPlacementAnimator>();

        animator.StartFallAnimation(worldPosition);

        if (piece.storeThisToGrid)
        {
            _gridManager.AddObjectToGrid(
                piece,
                new Vector2Int(gridPosition.x, gridPosition.y),
                piece.sizeOnGrid,
                _currentDirection);
        }

        _previewSystem.UpdatePosition(
            _gridManager.Grid.GetWorldPosition(gridPosition.x, gridPosition.y),
            new Vector3(offset.x, 0, offset.y),
            false);

        _soundFeedback.PlaySound(SoundType.Place);
    }

    /// <inheritdoc/>
    public void UpdateState(Vector3Int gridPosition)
    {
        bool valid        = CheckPlacementValidity(gridPosition, _piece.Size, _currentDirection);
        Vector2Int offset = _currentDirection.GetRotationOffset(_piece.Size);

        _previewSystem.UpdatePosition(
            _gridManager.Grid.GetWorldPosition(gridPosition.x, gridPosition.y),
            new Vector3(offset.x, 0, offset.y),
            valid);
    }

    /// <summary>Rotates the preview to the next direction and refreshes its position.</summary>
    public void OnRotate(Vector3Int gridPosition)
    {
        _currentDirection = _currentDirection.GetNextDirection();
        _currentRotation  = _currentDirection.GetDirectionRotation();

        Vector2Int offset = _currentDirection.GetRotationOffset(_piece.Size);
        bool valid        = CheckPlacementValidity(gridPosition, _piece.Size, _currentDirection);

        _previewSystem.UpdatePosition(
            _gridManager.Grid.GetWorldPosition(gridPosition.x, gridPosition.y),
            new Vector3(offset.x, 0, offset.y),
            valid);

        _previewSystem.SetRotation(_currentRotation);
        _soundFeedback.PlaySound(SoundType.Click);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private bool CheckPlacementValidity(Vector3Int gridPosition, Vector2Int objectSize, Direction direction)
        => _gridManager.CanBuildOnCell(
            new Vector2Int(gridPosition.x, gridPosition.y),
            objectSize,
            _piece.canBeBuiltOnLayers,
            direction);
}
