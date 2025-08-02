using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemUIPrefab;
    [FormerlySerializedAs("placementSystem")] [SerializeField] private BuildingManager buildingManager; // Reference to your building system

    [Header("UI Settings")]
    [SerializeField] private bool hideItemsWithZeroQuantity = true;

    private InventoryManager _inventoryManager;
    private Dictionary<int, InventoryItemUI> _itemUIElements = new Dictionary<int, InventoryItemUI>();

    private void Start()
    {
        // Find the inventory manager in the scene
        _inventoryManager = FindFirstObjectByType<InventoryManager>();

        if (_inventoryManager == null)
        {
            Debug.LogError("InventoryUI: No InventoryManager found in scene!");
            return;
        }

        // Subscribe to inventory events
        _inventoryManager.OnInventoryInitialized += InitializeUI;
        _inventoryManager.OnQuantityChanged += UpdateItemQuantity;

        // Initialize UI if inventory is already ready
        if (_inventoryManager.CurrentPreset != null)
        {
            InitializeUI();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (_inventoryManager != null)
        {
            _inventoryManager.OnInventoryInitialized -= InitializeUI;
            _inventoryManager.OnQuantityChanged -= UpdateItemQuantity;
        }
    }

    // Initialize the UI elements based on inventory preset
    private void InitializeUI()
    {
        // Clear existing UI elements
        ClearUI();

        if (_inventoryManager.CurrentPreset == null) return;

        // Create UI elements for each item in the preset
        foreach (var item in _inventoryManager.CurrentPreset.Items)
        {
            if (item == null) continue;

            CreateItemUI(item);
        }
    }


    // Create UI element for a single inventory item
    private void CreateItemUI(InventoryItem item)
    {
        GameObject itemUIObj = Instantiate(itemUIPrefab, itemContainer);
        InventoryItemUI itemUI = itemUIObj.GetComponent<InventoryItemUI>();

        if (itemUI == null)
        {
            itemUI = itemUIObj.AddComponent<InventoryItemUI>();
        }

        // Setup the UI element
        itemUI.Setup(item, _inventoryManager.GetCurrentQuantity(item.ID));

        // Add click listener to start building
        itemUI.OnItemClicked += () => OnItemSelected(item);

        // Store reference for updates
        _itemUIElements[item.ID] = itemUI;

        // Hide if quantity is zero and setting is enabled
        if (hideItemsWithZeroQuantity && _inventoryManager.GetCurrentQuantity(item.ID) <= 0)
        {
            itemUIObj.SetActive(false);
        }
    }

    // Handle item selection from UI
    private void OnItemSelected(InventoryItem item)
    {
        // Check if we have enough items
        if (!_inventoryManager.HasEnoughItems(item.ID))
        {
            Debug.Log($"Cannot build {item.ItemName} - not enough items in inventory");
            return;
        }

        // Start placement in building system
        if (buildingManager != null)
        {
            buildingManager.StartPlacement(item.ID);
        }
    }

    // Update quantity display for a specific item
    private void UpdateItemQuantity(int itemID, int newQuantity)
    {
        if (_itemUIElements.TryGetValue(itemID, out InventoryItemUI itemUI))
        {
            itemUI.UpdateQuantity(newQuantity);

            // Show/hide based on quantity and settings
            if (hideItemsWithZeroQuantity)
            {
                itemUI.gameObject.SetActive(newQuantity > 0);
            }
        }
    }

    private void ClearUI()
    {
        foreach (var kvp in _itemUIElements)
        {
            if (kvp.Value != null && kvp.Value.gameObject != null)
            {
                DestroyImmediate(kvp.Value.gameObject);
            }
        }
        _itemUIElements.Clear();
    }

    public void RefreshUI()
    {
        InitializeUI();
    }
}