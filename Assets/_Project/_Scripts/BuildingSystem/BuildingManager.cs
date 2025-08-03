using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum BuildMode
{
    None,
    Build,
    Remove
}

public struct BuildModeChangedEvent : IGameEvent
{
    public BuildMode currentBuildMode;

    public BuildModeChangedEvent(BuildMode mode)
    {
        currentBuildMode = mode;
    }
}

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

    private Transform _objectsParent;

    private void Start()
    {
        _objectsParent = new GameObject("Build Pieces").transform;
        Exit();
    }
    
    public void StartPlacement(int id)
    {
        Exit();
        gridVisualization.SetActive(true);
        _buildingState = new BuildState(id, database, this, inventoryManager, gridManager, preview, soundFeedback);
        
        EventBus.Fire(new BuildModeChangedEvent(BuildMode.Build));
        
        inputManager.OnClick += Click;
        inputManager.OnExit += Exit;
        inputManager.OnRotate += Rotate;
    }

    public void StartRemoving()
    {
        Exit();
        gridVisualization.SetActive(true);
        _buildingState = new RemoveState(this, gridManager, inventoryManager, preview, soundFeedback);
        
        EventBus.Fire(new BuildModeChangedEvent(BuildMode.Remove));
        
        inputManager.OnClick += Click;
        inputManager.OnExit += Exit;
    }

    private void Click()
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

    private void Rotate()
    {
        if (_buildingState is BuildState state)
        {
            state.OnRotate(_lastDetectedPosition);
        }
    }

    public void Exit()
    {
        gridVisualization.SetActive(false);
        
        if (_buildingState == null)
            return;
        
        _buildingState.EndState();
        
        inputManager.OnClick -= Click;
        inputManager.OnExit -= Exit;
        inputManager.OnRotate -= Rotate;
        
        _lastDetectedPosition = Vector3Int.zero;
        _buildingState = null;
        
        EventBus.Fire(new BuildModeChangedEvent(BuildMode.None));
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

    public GridBuildPiece CreateBuildPiece(BuildPieceData pieceData, Vector3 position, Quaternion rotation, string source = "")
    {
        GameObject pieceObj = Instantiate(pieceData.Prefab, position, rotation);
        pieceObj.transform.parent = _objectsParent;
        if (!pieceObj.TryGetComponent(out GridBuildPiece pieceComponent))
        {
            pieceComponent = pieceObj.AddComponent<GridBuildPiece>();
        }

        pieceComponent.Init(pieceData, source);
        return pieceComponent;
    }

    public void DestroyBuildPiece(GridBuildPiece piece)
    {
        Destroy(piece.gameObject);
    }
}
