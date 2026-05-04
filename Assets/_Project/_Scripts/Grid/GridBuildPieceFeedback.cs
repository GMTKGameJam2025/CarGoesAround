using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Swaps the materials on all <c>Previewable</c>-tagged renderers of a placed piece
/// to give visual feedback when the building system switches to Remove mode and the
/// piece is locked (cannot be removed).
/// </summary>
public class GridBuildPieceFeedback : MonoBehaviour, IBuildable
{
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material lockMaterial;

    private GridBuildPiece _piece;
    private List<Renderer> _renderers;

    private void Awake()
    {
        // Only drive renderers that are explicitly tagged as Previewable.
        _renderers = GetComponentsInChildren<Renderer>()
            .Where(r => r.CompareTag("Previewable"))
            .ToList();
    }

    /// <inheritdoc/>
    public void OnBuild(GridBuildPiece piece)
    {
        _piece = piece;
    }

    private void OnEnable()  => EventBus.Subscribe<BuildModeChangedEvent>(OnBuildModeChanged);
    private void OnDisable() => EventBus.Unsubscribe<BuildModeChangedEvent>(OnBuildModeChanged);

    private void OnBuildModeChanged(BuildModeChangedEvent @event)
    {
        if (!defaultMaterial || !lockMaterial) return;

        bool showLock = !_piece.CanBeRemovedFromGrid && @event.currentBuildMode == BuildMode.Remove;
        Material target = showLock ? lockMaterial : defaultMaterial;

        foreach (Renderer rend in _renderers)
        {
            Material[] mats = rend.materials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = target;
            rend.materials = mats;
        }
    }
}
