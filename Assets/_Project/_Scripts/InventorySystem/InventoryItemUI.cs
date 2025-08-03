// InventoryItemUI.cs - Individual UI element for inventory items
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InventoryItemUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button itemButton;
    [SerializeField] private Image backgroundImage;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color unavailableColor = Color.gray;

    private InventoryItem _inventoryItem;
    private int _currentQuantity;

    public event Action OnItemClicked;

    private void Awake()
    {
        // Setup button click listener
        if (itemButton == null)
            itemButton = GetComponent<Button>();

        if (itemButton != null)
            itemButton.onClick.AddListener(() => OnItemClicked?.Invoke());
    }

    // Setup the UI element with inventory item data
    public void Setup(InventoryItem item, int quantity)
    {
        _inventoryItem = item;
        _currentQuantity = quantity;

        // Set icon
        if (iconImage != null && item.ItemIcon != null)
        {
            iconImage.sprite = item.ItemIcon;
        }

        // Set name
        if (nameText != null)
        {
            nameText.text = item.ItemName;
        }

        // Update quantity display
        UpdateQuantity(quantity);
    }

    // Update the quantity display
    public void UpdateQuantity(int newQuantity)
    {
        _currentQuantity = newQuantity;

        // Update quantity text
        if (quantityText != null)
        {
            quantityText.text = newQuantity.ToString();
        }

        // Update visual state based on availability
        UpdateVisualState(newQuantity > 0);
    }

    /// Update visual appearance based on item availability
    private void UpdateVisualState(bool isAvailable)
    {
        Color targetColor = isAvailable ? normalColor : unavailableColor;

        if (backgroundImage != null)
        {
            backgroundImage.color = targetColor;
        }

        if (iconImage != null)
        {
            iconImage.color = targetColor;
        }

        // Enable/disable button based on availability
        if (itemButton != null)
        {
            itemButton.interactable = isAvailable;
        }
    }

    public InventoryItem GetInventoryItem() => _inventoryItem;
    public int GetCurrentQuantity() => _currentQuantity;
}