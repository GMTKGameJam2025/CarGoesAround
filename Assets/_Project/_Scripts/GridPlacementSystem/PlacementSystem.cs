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

    private GridData _gridData;


    [SerializeField]
    private PreviewSystem preview;

    private Vector3Int _lastDetectedPosition = Vector3Int.zero;

    [SerializeField]
    private ObjectPlacer objectPlacer;

    private IBuildingState _buildingState;

    [SerializeField]
    private SoundFeedback soundFeedback;

    private void Start()
    {
        StopPlacement();
        _gridData = new();

    }
    public void StartPlacement(int id)
    {
        StopPlacement();
        gridVisualization.SetActive(true);
        _buildingState = new PlacementState(id, grid, preview, database, _gridData, objectPlacer, soundFeedback);
        inputManager.OnClick += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void StartRemoving()
    {
        StopPlacement();
        gridVisualization.SetActive(true);
        _buildingState = new RemovingState(grid, preview, _gridData, objectPlacer, soundFeedback);
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

    private void StopPlacement()
    {
        soundFeedback.PlaySound(SoundType.Click);
        if (_buildingState == null)
            return;

        gridVisualization.SetActive(false);
        _buildingState.EndState();
        inputManager.OnClick -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
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
