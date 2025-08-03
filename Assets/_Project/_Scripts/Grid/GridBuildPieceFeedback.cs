using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class GridBuildPieceFeedback : MonoBehaviour, IBuildable
{
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material lockMaterial;

    private GridBuildPiece _piece;
    private List<Renderer> _renderers;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>().ToList();
        
        // Only include renderers tagged with "Previewable"
        _renderers = _renderers.Where(rend => rend.CompareTag("Previewable")).ToList();
    }

    public void OnBuild(GridBuildPiece piece)
    {
        _piece = piece;
    }
    
    private void OnEnable()
    {
        EventBus.Subscribe<BuildModeChangedEvent>(OnBuildModeChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BuildModeChangedEvent>(OnBuildModeChanged);
    }

    public void OnBuildModeChanged(BuildModeChangedEvent @event)
    {
        if (!defaultMaterial || !lockMaterial) return;
        
        foreach (Renderer rend in _renderers)
        {
            Material[] materials = rend.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = !_piece.canBeRemovedFromGrid && @event.currentBuildMode == BuildMode.Remove ? lockMaterial : defaultMaterial;;
            }
            rend.materials = materials;
        }
    }
}