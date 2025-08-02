using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GridManager gridManager;
    
    public GridBuildPiece currentGridBuildPiece;
    
    [Header("Feedback")]
    [SerializeField] private GameObject gridVisualization;
    [SerializeField] private PreviewSystem preview;
    [SerializeField] private SoundFeedback soundFeedback;

    private Vector3Int _lastDetectedPosition = Vector3Int.zero;
    private IBuildingState _buildingState;

    private void Start()
    {
        OnExit();
    }
    public void StartPlacement()
    {
        OnExit();
        gridVisualization.SetActive(true);
        _buildingState = new BuildState(currentGridBuildPiece, this, gridManager, preview, soundFeedback);
        inputManager.OnClick += OnClick;
        inputManager.OnExit += OnExit;
        inputManager.OnRotate += OnRotate;
    }

    public void StartRemoving()
    {
        OnExit();
        gridVisualization.SetActive(true);
        //_buildingState = new RemovingState(grid, preview, soundFeedback);
        inputManager.OnClick += OnClick;
        inputManager.OnExit += OnExit;
    }

    private void OnClick()
    {
        if (inputManager.IsPointerOverUI())
        {
            return;
        }

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        gridManager.grid.GetXZ(mousePosition, out int x, out int z);
        Vector3Int gridPosition = new(x, z, 0);

        _buildingState.OnAction(gridPosition);
    }

    private void OnRotate()
    {
        if (_buildingState is BuildState state)
        {
            state.OnRotate(_lastDetectedPosition);
        }
    }

    private void OnExit()
    {
        soundFeedback.PlaySound(SoundType.Click);
        if (_buildingState == null)
            return;

        gridVisualization.SetActive(false);
        _buildingState.EndState();
        
        inputManager.OnClick -= OnClick;
        inputManager.OnExit -= OnExit;
        inputManager.OnRotate -= OnRotate;
        
        _lastDetectedPosition = Vector3Int.zero;
        _buildingState = null;
    }

    private void Update()
    {
        if (_buildingState == null)
            return;

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        gridManager.grid.GetXZ(mousePosition, out int x, out int z);
        Vector3Int gridPosition = new(x, z, 0);
        
        if (_lastDetectedPosition == gridPosition)
            return;
        
        _buildingState.UpdateState(gridPosition);
        _lastDetectedPosition = gridPosition;
    }

    public GridBuildPiece CreateBuildPiece(GridBuildPiece piece, Vector3 position, Quaternion rotation)
    {
        return Instantiate(piece, position, rotation);
    }
}
