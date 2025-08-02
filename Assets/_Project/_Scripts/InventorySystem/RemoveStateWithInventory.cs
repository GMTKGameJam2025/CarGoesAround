using UnityEngine;

public class RemovingStateWithInventory : IBuildingState
{
    private int gameObjectIndex = -1;
    private Grid grid;
    private PreviewSystem previewSystem;
    private GridData gridData;
    private ObjectPlacer objectPlacer;
    private SoundFeedback soundFeedback;
    private InventoryManager inventoryManager; // Add inventory manager reference
    private ObjectsDatabaseSO database; // Add database reference to get item data

    public RemovingStateWithInventory(Grid grid, PreviewSystem previewSystem, ObjectPlacer objectPlacer,
        SoundFeedback soundFeedback, InventoryManager inventoryManager, ObjectsDatabaseSO database)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.gridData = GridData.Instance;
        this.objectPlacer = objectPlacer;
        this.soundFeedback = soundFeedback;
        this.inventoryManager = inventoryManager; // Store inventory manager reference
        this.database = database; // Store database reference

        previewSystem.StartShowingRemovePreview();
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        if (!CheckIfSelectionIsValid(gridPosition))
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }

        gameObjectIndex = gridData.GetRepresentationIndex(gridPosition);
        if (gameObjectIndex > -1)
        {
            // Get the item data to know which item to add back to inventory
            PlacementData placementData = GetPlacementDataAt(gridPosition);
            if (placementData != null)
            {
                // Add item back to inventory
                inventoryManager.AddItems(placementData.ID);
            }

            soundFeedback.PlaySound(SoundType.Remove);
            gridData.RemoveObjectAt(gridPosition);
            objectPlacer.RemoveObjectAt(gameObjectIndex);
        }

        Vector3 cellPosition = grid.CellToWorld(gridPosition);
        previewSystem.UpdatePosition(cellPosition, CheckIfSelectionIsValid(gridPosition));
    }

    private bool CheckIfSelectionIsValid(Vector3Int gridPosition)
    {
        return !(gridData.CanPlaceObjectAt(gridPosition, Vector2Int.one));
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool isValid = CheckIfSelectionIsValid(gridPosition);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), isValid);

        GameObject objectToHighlight = GetObjectAt(gridPosition);
        previewSystem.HighlightObjectAt(objectToHighlight);
    }

    private GameObject GetObjectAt(Vector3Int gridPosition)
    {
        if (!gridData.CanPlaceObjectAt(gridPosition, Vector2Int.one))
        {
            int index = gridData.GetRepresentationIndex(gridPosition);
            if (index > -1)
            {
                return objectPlacer.GetPlacedObjectAt(index);
            }
        }
        return null;
    }

    // Helper method to get placement data - you'll need to add this to GridData
    private PlacementData GetPlacementDataAt(Vector3Int gridPosition)
    {
        return gridData.GetPlacementDataAt(gridPosition);
    }
}