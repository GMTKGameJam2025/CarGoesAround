using UnityEngine;

public class PlacementStateWithInventory : IBuildingState
{
    private int selectedObjectIndex = -1;
    private int id;
    private Grid grid;
    private PreviewSystem previewSystem;
    private ObjectsDatabaseSO database;
    private GridData gridData;
    private ObjectPlacer objectPlacer;
    private SoundFeedback soundFeedback;
    private InventoryManager inventoryManager; // Add inventory manager reference
    private int currentRotation = 0;

    public PlacementStateWithInventory(int id, Grid grid, PreviewSystem previewSystem, ObjectsDatabaseSO database, 
        ObjectPlacer objectPlacer, SoundFeedback soundFeedback, InventoryManager inventoryManager)
    {
        this.id = id;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.gridData = GridData.Instance;
        this.database = database;
        this.objectPlacer = objectPlacer;
        this.soundFeedback = soundFeedback;
        this.inventoryManager = inventoryManager; // Store inventory manager reference
        
        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == id);
        
        if (selectedObjectIndex > -1)
        {
            previewSystem.StartShowingPlacementPreview(
                database.objectsData[selectedObjectIndex].Prefab, 
                database.objectsData[selectedObjectIndex].Size);
        }
        else 
        {
            throw new System.Exception($"There is no object with ID: {id}");
        }
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        if (!placementValidity)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        
        // Check inventory before placing
        if (!inventoryManager.HasEnoughItems(id))
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            Debug.Log($"Not enough items in inventory to place object with ID: {id}");
            return;
        }
        
        // Consume inventory item
        if (!inventoryManager.ConsumeItems(id))
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        
        soundFeedback.PlaySound(SoundType.Place);
        float rotationAngle = currentRotation * 90f;
        int index = objectPlacer.PlaceObject(
            database.objectsData[selectedObjectIndex].Prefab, 
            grid.CellToWorld(gridPosition), 
            rotationAngle);
            
        gridData.AddObjectAt(gridPosition, 
            database.objectsData[selectedObjectIndex].Size, 
            database.objectsData[selectedObjectIndex].ID, 
            index);
            
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), false);
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        return gridData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size);
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
    }

    public void OnRotate()
    {
        currentRotation = (currentRotation + 1) % 4;
        float rotationAngle = currentRotation * 90f;
        previewSystem.SetRotation(rotationAngle);
        soundFeedback.PlaySound(SoundType.Click);
    }
}