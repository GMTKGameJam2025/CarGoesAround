using UnityEngine;

public class PlacementState : IBuildingState
{
    private int _selectedObjectIndex = -1;
    private int _id;
    private Grid _grid;
    private PreviewSystem _previewSystem;
    private ObjectsDatabaseSO _database;
    private GridData _gridData;
    private ObjectPlacer _objectPlacer;
    private SoundFeedback _soundFeedback;
    private int _currentRotation = 0; // Track rotation in 90-degree increments (0, 1, 2, 3)

    public PlacementState(int id, Grid grid, PreviewSystem previewSystem, ObjectsDatabaseSO database, ObjectPlacer objectPlacer, SoundFeedback soundFeedback)
    {
        _id = id;
        _grid = grid;
        _previewSystem = previewSystem;
        _gridData = GridData.Instance;
        _database = database;
        _objectPlacer = objectPlacer;
        _soundFeedback = soundFeedback;

        _selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == id);
        if (_selectedObjectIndex > -1)
        {
            previewSystem.StartShowingPlacementPreview(database.objectsData[_selectedObjectIndex].Prefab, database.objectsData[_selectedObjectIndex].Size);
        }
        else
            throw new System.Exception($"There is no object with ID: {id}");

    }

    public void EndState()
    {
        _previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, _selectedObjectIndex);
        if (!placementValidity)
        {
            _soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        _soundFeedback.PlaySound(SoundType.Place);

        float rotationAngle = _currentRotation * 90f;

        int index = _objectPlacer.PlaceObject(_database.objectsData[_selectedObjectIndex].Prefab, _grid.CellToWorld(gridPosition), rotationAngle);

        _gridData.AddObjectAt(gridPosition, _database.objectsData[_selectedObjectIndex].Size, _database.objectsData[_selectedObjectIndex].ID, index);
        _previewSystem.UpdatePosition(_grid.CellToWorld(gridPosition), false);
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        return _gridData.CanPlaceObjectAt(gridPosition, _database.objectsData[selectedObjectIndex].Size);
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, _selectedObjectIndex);

        _previewSystem.UpdatePosition(_grid.CellToWorld(gridPosition), placementValidity);
    }

    public void OnRotate()
    {
        _currentRotation = (_currentRotation + 1) % 4; // Cycle through 0, 1, 2, 3
        float rotationAngle = _currentRotation * 90f;
        _previewSystem.SetRotation(rotationAngle);
        _soundFeedback.PlaySound(SoundType.Click); // Optional: play sound on rotation
    }
}
