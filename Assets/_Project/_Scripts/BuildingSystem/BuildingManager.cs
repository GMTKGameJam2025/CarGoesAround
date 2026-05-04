using UnityEngine;

/// <summary>
/// Central controller for the building system.
/// Owns the active <see cref="IBuildingState"/>, routes input events to it,
/// and provides factory helpers for spawning / destroying build pieces.
/// </summary>
public class BuildingManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [SerializeField] private InputManager inputManager;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private BuildPieceDatabaseSO database;

    [Header("Feedback")]
    [SerializeField] private GameObject gridVisualization;
    [SerializeField] private PreviewSystem preview;
    [SerializeField] private SoundFeedback soundFeedback;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Vector3Int _lastDetectedPosition = Vector3Int.zero;
    private IBuildingState _buildingState;
    private Transform _objectsParent;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        _objectsParent = new GameObject("Build Pieces").transform;
        Exit();
    }

    private void Update()
    {
        if (_buildingState == null) return;

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        gridManager.Grid.GetXZ(mousePosition, out int x, out int z);
        Vector3Int gridPosition = new(x, z, 0);

        if (_lastDetectedPosition == gridPosition) return;

        _buildingState.UpdateState(gridPosition);
        _lastDetectedPosition = gridPosition;
    }

    // -------------------------------------------------------------------------
    // State transitions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Exits any active state, then enters <see cref="BuildState"/> for the piece
    /// identified by <paramref name="id"/>.
    /// </summary>
    public void StartPlacement(int id)
    {
        Exit();
        gridVisualization.SetActive(true);
        _buildingState = new BuildState(id, database, this, inventoryManager, gridManager, preview, soundFeedback);

        EventBus.Fire(new BuildModeChangedEvent(BuildMode.Build));

        inputManager.OnClick  += Click;
        inputManager.OnExit   += Exit;
        inputManager.OnRotate += Rotate;
    }

    /// <summary>
    /// Exits any active state, then enters <see cref="RemoveState"/>.
    /// </summary>
    public void StartRemoving()
    {
        Exit();
        gridVisualization.SetActive(true);
        _buildingState = new RemoveState(this, gridManager, inventoryManager, preview, soundFeedback);

        EventBus.Fire(new BuildModeChangedEvent(BuildMode.Remove));

        inputManager.OnClick += Click;
        inputManager.OnExit  += Exit;
    }

    /// <summary>
    /// Ends the active state and resets the building system to an idle state.
    /// Safe to call when no state is active.
    /// </summary>
    public void Exit()
    {
        gridVisualization.SetActive(false);

        if (_buildingState != null)
        {
            _buildingState.EndState();
            _buildingState = null;

            if (inputManager != null)
            {
                inputManager.OnClick  -= Click;
                inputManager.OnExit   -= Exit;
                inputManager.OnRotate -= Rotate;
            }
        }

        _lastDetectedPosition = Vector3Int.zero;

        EventBus.Fire(new BuildModeChangedEvent(BuildMode.None));
    }

    // -------------------------------------------------------------------------
    // Input handlers
    // -------------------------------------------------------------------------

    private void Click()
    {
        if (inputManager.IsPointerOverUI()) return;

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        gridManager.Grid.GetXZ(mousePosition, out int x, out int z);
        _buildingState.OnAction(new Vector3Int(x, z, 0));
    }

    private void Rotate()
    {
        if (_buildingState is BuildState buildState)
            buildState.OnRotate(_lastDetectedPosition);
    }

    // -------------------------------------------------------------------------
    // Factory helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Instantiates the prefab from <paramref name="pieceData"/>, attaches a
    /// <see cref="GridBuildPiece"/> component if one is not already present,
    /// initialises it, and parents it under the shared "Build Pieces" transform.
    /// </summary>
    /// <param name="pieceData">Database entry that describes the piece.</param>
    /// <param name="position">World-space spawn position.</param>
    /// <param name="rotation">Spawn rotation.</param>
    /// <param name="source">Optional tag identifying the spawn source (e.g. "LevelLoad").</param>
    /// <returns>The initialised <see cref="GridBuildPiece"/> component.</returns>
    public GridBuildPiece CreateBuildPiece(BuildPieceData pieceData, Vector3 position, Quaternion rotation,
                                           string source = "")
    {
        GameObject pieceObj = Instantiate(pieceData.Prefab, position, rotation);
        pieceObj.transform.parent = _objectsParent;

        if (!pieceObj.TryGetComponent(out GridBuildPiece pieceComponent))
            pieceComponent = pieceObj.AddComponent<GridBuildPiece>();

        pieceComponent.Init(pieceData, source);
        return pieceComponent;
    }

    /// <summary>Destroys the GameObject that hosts <paramref name="piece"/>.</summary>
    public void DestroyBuildPiece(GridBuildPiece piece)
        => Destroy(piece.gameObject);
}
