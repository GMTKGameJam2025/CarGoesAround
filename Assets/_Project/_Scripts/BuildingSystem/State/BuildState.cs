using UnityEngine;

public class BuildState : IBuildingState
{
    private int _id;
    private BuildPieceData _piece;
    private BuildingManager _buildingManager;
    private GridManager _gridManager;
    private InventoryManager _inventoryManager;
    private PreviewSystem _previewSystem;
    private SoundFeedback _soundFeedback;
    
    private Direction _currentDirection = Direction.Down;
    private float _currentRotation = 0;
    
    public BuildState(
        int id,
        BuildPieceDatabaseSO pieceDatabase,
        BuildingManager buildingManager, 
        InventoryManager inventoryManager,
        GridManager gridManager, 
        PreviewSystem previewSystem, 
        SoundFeedback soundFeedback)
    {
        _id = id;
        _piece = pieceDatabase.objectsData.Find(piece => piece.ID == id);
        _buildingManager = buildingManager;
        _gridManager = gridManager;
        _inventoryManager = inventoryManager;
        _previewSystem = previewSystem;
        _soundFeedback = soundFeedback;
        
        previewSystem.StartShowingPlacementPreview(_piece.Prefab, _piece.Size);
    }
    
    public void EndState()
    {
        _previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, _piece.Size, _currentDirection);
        if (!placementValidity)
        {
            _soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        _soundFeedback.PlaySound(SoundType.Place);
        
        // Check inventory before placing
        if (!_inventoryManager.HasEnoughItems(_id))
        {
            _soundFeedback.PlaySound(SoundType.wrongPlacement);
            Debug.Log($"Not enough items in inventory to place object with ID: {_id}");
            return;
        }
        
        // Consume inventory item
        if (!_inventoryManager.ConsumeItems(_id))
        {
            _soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        
        float rotationAngle = _currentRotation;

        Vector2Int offset = _currentDirection.GetRotationOffset(_piece.Size);
        
        GridBuildPiece piece = _buildingManager.CreateBuildPiece(_piece, _gridManager.grid.GetWorldPosition(gridPosition.x, gridPosition.y) + new Vector3(offset.x, 0, offset.y), Quaternion.Euler(0, rotationAngle, 0));
        
        if (piece.storeThisToGrid)
        {
            _gridManager.AddObjectToGrid(piece, new Vector2Int(gridPosition.x, gridPosition.y), piece.sizeOnGrid, _currentDirection);
        }
        _previewSystem.UpdatePosition(_gridManager.grid.GetWorldPosition(gridPosition.x, gridPosition.y),new Vector3(offset.x, 0, offset.y), false);
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, Vector2Int objectSize, Direction direction)
    {
        //TODO: remember to change back to Vector2Int
        return _gridManager.CanBuildOnCell(new Vector2Int(gridPosition.x, gridPosition.y), objectSize, _piece.canBeBuiltOnLayers, direction);
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, _piece.Size, _currentDirection);

        Vector2Int offset = _currentDirection.GetRotationOffset(_piece.Size);
        
        _previewSystem.UpdatePosition(_gridManager.grid.GetWorldPosition(gridPosition.x, gridPosition.y), new Vector3(offset.x, 0, offset.y), placementValidity);
    }
    
    public void OnRotate(Vector3Int gridPosition)
    {
        _currentDirection = _currentDirection.GetNextDirection();// Cycle through 0, 1, 2, 3
        _currentRotation = _currentDirection.GetDirectionRotation();
        
        Vector2Int offset = _currentDirection.GetRotationOffset(_piece.Size);
        
        bool placementValidity = CheckPlacementValidity(gridPosition, _piece.Size, _currentDirection);
        
        _previewSystem.UpdatePosition(_gridManager.grid.GetWorldPosition(gridPosition.x, gridPosition.y), new Vector3(offset.x, 0, offset.y), placementValidity);
        
        _previewSystem.SetRotation(_currentRotation);
        _soundFeedback.PlaySound(SoundType.Click); // Optional: play sound on rotation
    }
}