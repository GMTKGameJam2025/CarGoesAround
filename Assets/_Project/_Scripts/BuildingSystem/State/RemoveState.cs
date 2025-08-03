using UnityEngine;
public class RemoveState : IBuildingState
{
    private BuildingManager _buildingManager;
    private GridManager _gridManager;
    private InventoryManager _inventoryManager;
    private PreviewSystem _previewSystem;
    private SoundFeedback _soundFeedback;

    public RemoveState(BuildingManager buildingManager, GridManager gridManager, InventoryManager inventoryManager, PreviewSystem previewSystem, SoundFeedback soundFeedback)
    {
        _buildingManager = buildingManager;
        _gridManager = gridManager;
        _inventoryManager = inventoryManager;
        _previewSystem = previewSystem;
        _soundFeedback = soundFeedback;

        previewSystem.StartShowingRemovePreview();
    }

    public void EndState()
    {
        _previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        Vector2Int gridPos = new Vector2Int(gridPosition.x, gridPosition.y);

        if (!_gridManager.CanRemoveOnCell(gridPos))
        {
            _soundFeedback.PlaySound(SoundType.wrongPlacement);
        }
        else
        {
            GridBuildPiece piece = _gridManager.RemoveObjectFromGrid(gridPos);

            if (_inventoryManager)
            {
                _inventoryManager.AddItems(piece.id);
            }
            _buildingManager.DestroyBuildPiece(piece);
            
            _soundFeedback.PlaySound(SoundType.Remove);
        }

        Vector3 cellPosition = _gridManager.Grid.GetWorldPosition(gridPosition.x, gridPosition.y);
        _previewSystem.UpdatePosition(cellPosition, Vector3.zero, CheckIfSelectionIsValid(gridPosition));
    }

    private bool CheckIfSelectionIsValid(Vector3Int gridPosition)
    {
        Vector2Int gridPos = new Vector2Int(gridPosition.x, gridPosition.y);
        return (_gridManager.CanRemoveOnCell(gridPos));
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool isValid = CheckIfSelectionIsValid(gridPosition);
        Vector3 cellPosition = _gridManager.Grid.GetWorldPosition(gridPosition.x, gridPosition.y);
        _previewSystem.UpdatePosition(cellPosition, Vector3.zero, isValid);

        GridBuildPiece piece = _gridManager.GetTopLevelObject(new Vector2Int(gridPosition.x, gridPosition.y));
        // Highlight the object at the current position if there is one
        GameObject objectToHighlight = piece? piece.gameObject : null;
        _previewSystem.HighlightObjectAt(objectToHighlight);
    }
    
}