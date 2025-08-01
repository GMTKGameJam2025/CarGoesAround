using System;
using UnityEngine;

public class RemovingState : IBuildingState
{
    private int gameObjectIndex = -1;
    Grid grid;
    PreviewSystem previewSystem;
    GridData floorData;
    GridData furnitureData;
    ObjectPlacer objectPlacer;
    SoundFeedback soundFeedback;

    public RemovingState(Grid grid, PreviewSystem previewSystem, GridData floorData, GridData furnitureData, ObjectPlacer objectPlacer, SoundFeedback soundFeedback)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.floorData = floorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;
        this.soundFeedback = soundFeedback;

        previewSystem.StartShowingRemovePreview();
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        GridData selectedData = null;
        if (!furnitureData.CanPlaceOjectAt(gridPosition, Vector2Int.one))
        {
            selectedData = furnitureData;
        }
        else if (!floorData.CanPlaceOjectAt(gridPosition, Vector2Int.one))
        {
            selectedData = floorData;
        }

        if (selectedData == null)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
        }
        else
        {
            gameObjectIndex = selectedData.GetRepresentationIndex(gridPosition);
            if (gameObjectIndex > -1)
            {
                soundFeedback.PlaySound(SoundType.Remove);
                selectedData.RemoveObjectAt(gridPosition);
                objectPlacer.RemoveObjecAt(gameObjectIndex);
            }
        }

        Vector3 cellPosition = grid.CellToWorld(gridPosition);
        previewSystem.UpdatePosition(cellPosition, CheckIfSelectionIsValid(gridPosition));
    }

    private bool CheckIfSelectionIsValid(Vector3Int gridPosition)
    {
        return !(furnitureData.CanPlaceOjectAt(gridPosition, Vector2Int.one) && floorData.CanPlaceOjectAt(gridPosition, Vector2Int.one));
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool isValid = CheckIfSelectionIsValid(gridPosition);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), isValid);

        // Highlight the object at the current position if there is one
        GameObject objectToHighlight = GetObjectAt(gridPosition);
        previewSystem.HighlightObjectAt(objectToHighlight);
    }

    private GameObject GetObjectAt(Vector3Int gridPosition)
    {
        // Check furniture first, then floor
        if (!furnitureData.CanPlaceOjectAt(gridPosition, Vector2Int.one))
        {
            int index = furnitureData.GetRepresentationIndex(gridPosition);
            if (index > -1)
            {
                return objectPlacer.GetPlacedObjectAt(index);
            }
        }
        else if (!floorData.CanPlaceOjectAt(gridPosition, Vector2Int.one))
        {
            int index = floorData.GetRepresentationIndex(gridPosition);
            if (index > -1)
            {
                return objectPlacer.GetPlacedObjectAt(index);
            }
        }

        return null;
    }
}
