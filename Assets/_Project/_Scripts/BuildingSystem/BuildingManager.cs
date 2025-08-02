using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private BuildPieceDatabaseSO database;
    
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
    
    public void StartPlacement(int id)
    {
        OnExit();
        gridVisualization.SetActive(true);
        _buildingState = new BuildState(id, database, this, inventoryManager, gridManager, preview, soundFeedback);
        inputManager.OnClick += OnClick;
        inputManager.OnExit += OnExit;
        inputManager.OnRotate += OnRotate;
    }

    public void StartRemoving()
    {
        OnExit();
        gridVisualization.SetActive(true);
        _buildingState = new RemoveState(this, gridManager, inventoryManager, preview, soundFeedback);
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
        gridManager.Grid.GetXZ(mousePosition, out int x, out int z);
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
        gridManager.Grid.GetXZ(mousePosition, out int x, out int z);
        Vector3Int gridPosition = new(x, z, 0);
        
        if (_lastDetectedPosition == gridPosition)
            return;
        
        _buildingState.UpdateState(gridPosition);
        _lastDetectedPosition = gridPosition;
    }

    public GridBuildPiece CreateBuildPiece(BuildPieceData pieceData, Vector3 position, Quaternion rotation)
    {
        GameObject pieceObj = Instantiate(pieceData.Prefab, position, rotation);
        if (!pieceObj.TryGetComponent(out GridBuildPiece pieceComponent))
        {
            pieceComponent = pieceObj.AddComponent<GridBuildPiece>();
        }

        pieceComponent.Init(gridManager, pieceData);
        return pieceComponent;
    }

    public void DestroyBuildPiece(GridBuildPiece piece)
    {
        Destroy(piece.gameObject);
    }
}
