using UnityEngine;

public class BuildState : IBuildingState
{
    private GridBuildPiece _piece;
    private PlacementSystem _placementSystem;
    private GridManager _gridManager;
    private PreviewSystem _previewSystem;
    private SoundFeedback _soundFeedback;
    
    private Direction _currentDirection = Direction.Down;
    private float _currentRotation = 0;
    
    public BuildState(GridBuildPiece currentPiece, PlacementSystem placementSystem, GridManager gridManager, PreviewSystem previewSystem, SoundFeedback soundFeedback)
    {
        _piece = currentPiece;
        _placementSystem = placementSystem;
        _gridManager = gridManager;
        _previewSystem = previewSystem;
        _soundFeedback = soundFeedback;
        
        previewSystem.StartShowingPlacementPreview(currentPiece.gameObject, currentPiece.sizeOnGrid);
    }
    
    public void EndState()
    {
        _previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, _piece.sizeOnGrid, _currentDirection);
        if (!placementValidity)
        {
            _soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        _soundFeedback.PlaySound(SoundType.Place);

        float rotationAngle = _currentRotation * 90f;

        Vector2Int offset = _currentDirection.GetRotationOffset(new Vector2Int(gridPosition.x, gridPosition.y));
        Vector3 actualPosition = _gridManager.grid.GetWorldPosition(gridPosition.x, gridPosition.y) + new Vector3(offset.x, offset.y);
        
        GridBuildPiece piece = _placementSystem.CreateBuildPiece(_piece, actualPosition, Quaternion.Euler(0, rotationAngle, 0));

        _gridManager.AddObjectToGrid(piece, new Vector2Int(gridPosition.x, gridPosition.y), piece.sizeOnGrid);
        _previewSystem.UpdatePosition(actualPosition, false);
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, Vector2Int objectSize, Direction direction)
    {
        //TODO: remember to change back to Vector2Int
        return _gridManager.CanBuildOnCell(new Vector2Int(gridPosition.x, gridPosition.y), objectSize, direction);
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, _piece.sizeOnGrid, _currentDirection);

        Vector2Int offset = _currentDirection.GetRotationOffset(new Vector2Int(gridPosition.x, gridPosition.y));
        Vector3 actualPosition = _gridManager.grid.GetWorldPosition(gridPosition.x, gridPosition.y) + new Vector3(offset.x, offset.y);
        
        _previewSystem.UpdatePosition(actualPosition, placementValidity);
    }
    
    public void OnRotate()
    {
        _currentDirection = _currentDirection.GetNextDirection();// Cycle through 0, 1, 2, 3
        _currentRotation = _currentDirection.GetDirectionRotation();
        _previewSystem.SetRotation(_currentRotation);
        _soundFeedback.PlaySound(SoundType.Click); // Optional: play sound on rotation
    }
}