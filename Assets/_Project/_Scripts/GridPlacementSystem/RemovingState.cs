using UnityEngine;
public class RemovingState : IBuildingState
{
    private int _gameObjectIndex = -1;
    private Grid _grid;
    private PreviewSystem _previewSystem;
    private GridData _gridData;
    private ObjectPlacer _objectPlacer;
    private SoundFeedback _soundFeedback;

    public RemovingState(Grid grid, PreviewSystem previewSystem, ObjectPlacer objectPlacer, SoundFeedback soundFeedback)
    {
        _grid = grid;
        _previewSystem = previewSystem;
        _gridData = GridData.Instance;
        _objectPlacer = objectPlacer;
        _soundFeedback = soundFeedback;

        previewSystem.StartShowingRemovePreview();
    }

    public void EndState()
    {
        _previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        GridData selectedData = _gridData;

        if (selectedData == null)
        {
            _soundFeedback.PlaySound(SoundType.wrongPlacement);
        }
        else
        {
            _gameObjectIndex = selectedData.GetRepresentationIndex(gridPosition);
            if (_gameObjectIndex > -1)
            {
                _soundFeedback.PlaySound(SoundType.Remove);
                selectedData.RemoveObjectAt(gridPosition);
                _objectPlacer.RemoveObjectAt(_gameObjectIndex);
            }
        }

        Vector3 cellPosition = _grid.CellToWorld(gridPosition);
        _previewSystem.UpdatePosition(cellPosition, CheckIfSelectionIsValid(gridPosition));
    }

    private bool CheckIfSelectionIsValid(Vector3Int gridPosition)
    {
        return !(_gridData.CanPlaceObjectAt(gridPosition, Vector2Int.one) && _gridData.CanPlaceObjectAt(gridPosition, Vector2Int.one));
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool isValid = CheckIfSelectionIsValid(gridPosition);
        _previewSystem.UpdatePosition(_grid.CellToWorld(gridPosition), isValid);

        // Highlight the object at the current position if there is one
        GameObject objectToHighlight = GetObjectAt(gridPosition);
        _previewSystem.HighlightObjectAt(objectToHighlight);
    }

    private GameObject GetObjectAt(Vector3Int gridPosition)
    {
        // Check furniture first, then floor
        if (!_gridData.CanPlaceObjectAt(gridPosition, Vector2Int.one))
        {
            int index = _gridData.GetRepresentationIndex(gridPosition);
            if (index > -1)
            {
                return _objectPlacer.GetPlacedObjectAt(index);
            }
        }
        // else if (!_gridData.CanPlaceObjectAt(gridPosition, Vector2Int.one))
        // {
        //     int index = _gridData.GetRepresentationIndex(gridPosition);
        //     if (index > -1)
        //     {
        //         return _objectPlacer.GetPlacedObjectAt(index);
        //     }
        // }

        return null;
    }
}