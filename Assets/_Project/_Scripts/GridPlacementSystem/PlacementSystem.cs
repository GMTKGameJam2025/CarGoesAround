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

    // [SerializeField]
    // private AudioSource placementSound;

    private GridData floorData, furnitureData;


    [SerializeField]
    private PreviewSystem preview;

    private Vector3Int lastDetectedPosition = Vector3Int.zero;

    [SerializeField]
    private ObjectPlacer objectPlacer;

    IBuildingState buildingState;

    private void Start()
    {
        StopPlacement();
        floorData = new();
        furnitureData = new();

    }
    public void StartPlacement(int ID)
    {
        StopPlacement();
        gridVisualization.SetActive(true);
        buildingState = new PlacementState(ID, grid, preview, database, floorData, furnitureData, objectPlacer);
        inputManager.OnClick += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void StartRemoving()
    {
        StopPlacement();
        gridVisualization.SetActive(true);
        buildingState = new RemovingState(grid, preview, floorData, furnitureData, objectPlacer);
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

        buildingState.OnAction(gridPosition);
    }

    // private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    // {
    //     GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData;

    //     return selectedData.CanPlaceOjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size);
    // }

    private void StopPlacement()
    {
        if (buildingState == null)
            return;

        gridVisualization.SetActive(false);
        buildingState.EndState();
        inputManager.OnClick -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
        lastDetectedPosition = Vector3Int.zero;
        buildingState = null;
    }

    private void Update()
    {
        if (buildingState == null)
            return;

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        if (lastDetectedPosition != gridPosition)
        {
            buildingState.UpdateState(gridPosition);
            lastDetectedPosition = gridPosition;
        }
    }
}
