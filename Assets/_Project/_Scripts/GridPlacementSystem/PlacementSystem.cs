using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private Grid grid;

    [SerializeField]
    private ObjectsDatabaseSO database;
    [SerializeField]
    private GameObject gridVisualization;
    
    [SerializeField]
    private PreviewSystem preview;

    private Vector3Int _lastDetectedPosition = Vector3Int.zero;

    [SerializeField]
    private ObjectPlacer objectPlacer;

    private IBuildingState _buildingState;

    [SerializeField]
    private SoundFeedback soundFeedback;

    [SerializeField]
    private InventoryManager inventoryManager;

    private void Start()
    {
        StopPlacement();
    }
    public void StartPlacement(int id)
    {
        StopPlacement();
        gridVisualization.SetActive(true);
        _buildingState = new PlacementStateWithInventory(id, grid, preview, database, objectPlacer, soundFeedback, inventoryManager);
        inputManager.OnClick += PlaceStructure;
        inputManager.OnExit += StopPlacement;
        inputManager.OnRotate += RotateObject;
    }

    public void StartRemoving()
    {
        StopPlacement();
        gridVisualization.SetActive(true);
        _buildingState = new RemovingStateWithInventory(grid, preview, objectPlacer, soundFeedback, inventoryManager, database);
        inputManager.OnClick += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    private void PlaceStructure()
    {
        if (inputManager.IsPointerOverUI())
        {
            return;
        }

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        _buildingState.OnAction(gridPosition);
    }

    private void RotateObject()
    {
        if (_buildingState != null)
        {
            ((PlacementState)_buildingState).OnRotate();
        }
    }

    private void StopPlacement()
    {
        soundFeedback.PlaySound(SoundType.Click);
        if (_buildingState == null)
            return;

        gridVisualization.SetActive(false);
        _buildingState.EndState();
        inputManager.OnClick -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
        inputManager.OnRotate -= RotateObject;
        _lastDetectedPosition = Vector3Int.zero;
        _buildingState = null;
    }

    private void Update()
    {
        if (_buildingState == null)
            return;

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        if (_lastDetectedPosition == gridPosition)
            return;
        _buildingState.UpdateState(gridPosition);
        _lastDetectedPosition = gridPosition;
    }
}
