using UnityEngine;
using UnityEngine.Rendering;
public class PreviewSystem : MonoBehaviour
{
    [SerializeField] private float previewYOffset = 0.06f;
    [SerializeField] private GameObject cellIndicator;
    [SerializeField] private Material previewMaterialsPrefab;
    [SerializeField] private Material highlightMaterial; // highlighting existing objects

    private GameObject _previewObject;
    private Material _previewMaterialInstance;
    private Renderer _cellIndicatorRender;

    // For highlighting existing objects during removal
    private GameObject _currentHighlightedObject;
    private Renderer[] _originalRenderers;
    private Material[][] _originalMaterials;

    private void Start()
    {
        _previewMaterialInstance = new Material(previewMaterialsPrefab);
        cellIndicator.SetActive(false);
        _cellIndicatorRender = cellIndicator.GetComponentInChildren<Renderer>();

        if (highlightMaterial == null)
        {
            highlightMaterial = new Material(Shader.Find("Standard"));
            highlightMaterial.color = new Color(1f, 0f, 0f, 0.7f); // Red with transparency
            highlightMaterial.SetFloat("_Mode", 2); // Set to Fade mode for transparency
            highlightMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            highlightMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            highlightMaterial.SetInt("_ZWrite", 0);
            highlightMaterial.DisableKeyword("_ALPHATEST_ON");
            highlightMaterial.EnableKeyword("_ALPHABLEND_ON");
            highlightMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            highlightMaterial.renderQueue = 3000;
        }
    }

    public void StartShowingPlacementPreview(GameObject prefab, Vector2Int size)
    {
        _previewObject = Instantiate(prefab);

        PreparePreview(_previewObject);
        PrepareCursor(size);
        cellIndicator.SetActive(true);
    }

    private void PrepareCursor(Vector2Int size)
    {
        if (size.x > 0 || size.y > 0)
        {
            cellIndicator.transform.localScale = new Vector3(size.x, 1, size.y);
            _cellIndicatorRender.material.mainTextureScale = size;
            cellIndicator.transform.rotation = Quaternion.identity;
        }
    }

    private void PreparePreview(GameObject previewObject)
    {
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            Material[] materials = rend.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = _previewMaterialInstance;
            }
            rend.materials = materials;
        }
    }

    public void StopShowingPreview()
    {
        cellIndicator.SetActive(false);
        if (_previewObject)
            Destroy(_previewObject);

        ClearObjectHighlight();
    }

    public void SetRotation(float rotationAngle)
    {
        Quaternion rotation = Quaternion.Euler(0, rotationAngle, 0);
        _previewObject.transform.rotation = rotation;
        cellIndicator.transform.rotation = rotation;
    }

    // Object highlighting during removal
    public void HighlightObjectAt(GameObject targetObject)
    {
        if (targetObject == _currentHighlightedObject)
            return;

        // Clear previous highlight
        ClearObjectHighlight();

        if (!targetObject)
            return;

        _currentHighlightedObject = targetObject;
        _originalRenderers = targetObject.GetComponentsInChildren<Renderer>();
        _originalMaterials = new Material[_originalRenderers.Length][];

        // Store original materials and apply highlight
        for (int i = 0; i < _originalRenderers.Length; i++)
        {
            _originalMaterials[i] = _originalRenderers[i].materials;
            Material[] highlightMaterials = new Material[_originalMaterials[i].Length];

            for (int j = 0; j < highlightMaterials.Length; j++)
            {
                highlightMaterials[j] = highlightMaterial;
            }

            _originalRenderers[i].materials = highlightMaterials;
        }
    }

    public void ClearObjectHighlight()
    {
        if (_currentHighlightedObject && _originalRenderers != null)
        {
            // Restore original materials
            for (int i = 0; i < _originalRenderers.Length; i++)
            {
                if (_originalRenderers[i]&& _originalMaterials[i] != null)
                {
                    _originalRenderers[i].materials = _originalMaterials[i];
                }
            }
        }

        _currentHighlightedObject = null;
        _originalRenderers = null;
        _originalMaterials = null;
    }

    public void UpdatePosition(Vector3 position, Vector3 offset, bool isValid)
    {
        if (_previewObject != null)
        {
            MovePreview(position + offset);
            ApplyFeedbackToPreview(isValid);
        }

        MoveCursor(position  + offset);
        ApplyFeedbackToCursor(isValid);
    }

    private void MovePreview(Vector3 position)
    {
        _previewObject.transform.position = new Vector3(position.x, position.y + previewYOffset, position.z);
    }

    private void MoveCursor(Vector3 position)
    {
        cellIndicator.transform.position = position;
    }

    private void ApplyFeedbackToPreview(bool isValid)
    {
        Color feedbackColor = isValid ? Color.green : Color.red;
        feedbackColor.a = 0.5f;
        _previewMaterialInstance.color = feedbackColor;
    }

    private void ApplyFeedbackToCursor(bool isValid)
    {
        Color feedbackColor = isValid ? Color.green : Color.red;
        feedbackColor.a = 0.5f;
        _cellIndicatorRender.material.color = feedbackColor;
    }

    internal void StartShowingRemovePreview()
    {
        cellIndicator.SetActive(true);
        PrepareCursor(Vector2Int.one);
        ApplyFeedbackToCursor(false);
    }
}