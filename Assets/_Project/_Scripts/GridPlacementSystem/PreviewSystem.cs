using System;
using UnityEngine;

public class PreviewSystem : MonoBehaviour
{
    [SerializeField] private float previewYOffset = 0.06f;
    [SerializeField] private GameObject cellIndicator;
    [SerializeField] private Material previewMaterialsPrefab;
    [SerializeField] private Material highlightMaterial; // highlighting existing objects

    private GameObject previewObject;
    private Material previewMaterialInstance;
    private Renderer cellIndicatorRender;

    // For highlighting existing objects during removal
    private GameObject currentHighlightedObject;
    private Renderer[] originalRenderers;
    private Material[][] originalMaterials;

    private void Start()
    {
        previewMaterialInstance = new Material(previewMaterialsPrefab);
        cellIndicator.SetActive(false);
        cellIndicatorRender = cellIndicator.GetComponentInChildren<Renderer>();

        if (highlightMaterial == null)
        {
            highlightMaterial = new Material(Shader.Find("Standard"));
            highlightMaterial.color = new Color(1f, 0f, 0f, 0.7f); // Red with transparency
            highlightMaterial.SetFloat("_Mode", 2); // Set to Fade mode for transparency
            highlightMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            highlightMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            highlightMaterial.SetInt("_ZWrite", 0);
            highlightMaterial.DisableKeyword("_ALPHATEST_ON");
            highlightMaterial.EnableKeyword("_ALPHABLEND_ON");
            highlightMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            highlightMaterial.renderQueue = 3000;
        }
    }

    public void StartShowingPlacementPreview(GameObject prefab, Vector2Int size)
    {
        previewObject = Instantiate(prefab);
        PreparePreview(previewObject);
        PrepareCursor(size);
        cellIndicator.SetActive(true);
    }

    private void PrepareCursor(Vector2Int size)
    {
        if (size.x > 0 || size.y > 0)
        {
            cellIndicator.transform.localScale = new Vector3(size.x, 1, size.y);
            cellIndicatorRender.material.mainTextureScale = size;
        }
    }

    private void PreparePreview(GameObject previewObject)
    {
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = previewMaterialInstance;
            }
            renderer.materials = materials;
        }
    }

    public void StopShowingPreview()
    {
        cellIndicator.SetActive(false);
        if (previewObject != null)
            Destroy(previewObject);

        ClearObjectHighlight();
    }

    // Object highlighting during removal
    public void HighlightObjectAt(GameObject targetObject)
    {
        if (targetObject == currentHighlightedObject)
            return;

        // Clear previous highlight
        ClearObjectHighlight();

        if (targetObject == null)
            return;

        currentHighlightedObject = targetObject;
        originalRenderers = targetObject.GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[originalRenderers.Length][];

        // Store original materials and apply highlight
        for (int i = 0; i < originalRenderers.Length; i++)
        {
            originalMaterials[i] = originalRenderers[i].materials;
            Material[] highlightMaterials = new Material[originalMaterials[i].Length];

            for (int j = 0; j < highlightMaterials.Length; j++)
            {
                highlightMaterials[j] = highlightMaterial;
            }

            originalRenderers[i].materials = highlightMaterials;
        }
    }

    public void ClearObjectHighlight()
    {
        if (currentHighlightedObject != null && originalRenderers != null)
        {
            // Restore original materials
            for (int i = 0; i < originalRenderers.Length; i++)
            {
                if (originalRenderers[i] != null && originalMaterials[i] != null)
                {
                    originalRenderers[i].materials = originalMaterials[i];
                }
            }
        }

        currentHighlightedObject = null;
        originalRenderers = null;
        originalMaterials = null;
    }

    public void UpdatePosition(Vector3 position, bool isValid)
    {
        if (previewObject != null)
        {
            MovePreview(position);
            ApplyFeedbackToPreview(isValid);
        }

        MoveCursor(position);
        ApplyFeedbackToCursor(isValid);
    }

    private void MovePreview(Vector3 position)
    {
        previewObject.transform.position = new Vector3(position.x, position.y + previewYOffset, position.z);
    }

    private void MoveCursor(Vector3 position)
    {
        cellIndicator.transform.position = position;
    }

    private void ApplyFeedbackToPreview(bool isValid)
    {
        Color feedbackColor = isValid ? Color.green : Color.red;
        feedbackColor.a = 0.5f;
        previewMaterialInstance.color = feedbackColor;
    }

    private void ApplyFeedbackToCursor(bool isValid)
    {
        Color feedbackColor = isValid ? Color.green : Color.red;
        feedbackColor.a = 0.5f;
        cellIndicatorRender.material.color = feedbackColor;
    }

    internal void StartShowingRemovePreview()
    {
        cellIndicator.SetActive(true);
        PrepareCursor(Vector2Int.one);
        ApplyFeedbackToCursor(false);
    }
}
