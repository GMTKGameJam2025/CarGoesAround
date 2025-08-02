using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemUIPrefab;
    [SerializeField] private PlacementSystem placementSystem; // Reference to your building system

    [Header("UI Settings")]
    [SerializeField] private bool hideItemsWithZeroQuantity = true;

    private InventoryManager inventoryManager;
    private Dictionary<int, InventoryItemUI> itemUIElements = new Dictionary<int, InventoryItemUI>();

    private void Start()
    {
        // Find the inventory manager in the scene
        inventoryManager = FindFirstObjectByType<InventoryManager>();

        if (inventoryManager == null)
        {
            Debug.LogError("InventoryUI: No InventoryManager found in scene!");
            return;
        }

        // Subscribe to inventory events
        inventoryManager.OnInventoryInitialized += InitializeUI;
        inventoryManager.OnQuantityChanged += UpdateItemQuantity;

        // Initialize UI if inventory is already ready
        if (inventoryManager.CurrentPreset != null)
        {
            InitializeUI();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryInitialized -= InitializeUI;
            inventoryManager.OnQuantityChanged -= UpdateItemQuantity;
        }
    }

    // Initialize the UI elements based on inventory preset
    private void InitializeUI()
    {
        // Clear existing UI elements
        ClearUI();

        if (inventoryManager.CurrentPreset == null) return;

        // Create UI elements for each item in the preset
        foreach (var item in inventoryManager.CurrentPreset.Items)
        {
            if (item.BuildPieceData == null) continue;

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
        itemUI.Setup(item, inventoryManager.GetCurrentQuantity(item.BuildPieceData.ID));

        // Add click listener to start building
        itemUI.OnItemClicked += () => OnItemSelected(item);

        // Store reference for updates
        itemUIElements[item.BuildPieceData.ID] = itemUI;

        // Hide if quantity is zero and setting is enabled
        if (hideItemsWithZeroQuantity && inventoryManager.GetCurrentQuantity(item.BuildPieceData.ID) <= 0)
        {
            itemUIObj.SetActive(false);
        }
    }

    // Handle item selection from UI
    private void OnItemSelected(InventoryItem item)
    {
        // Check if we have enough items
        if (!inventoryManager.HasEnoughItems(item.BuildPieceData.ID))
        {
            Debug.Log($"Cannot build {item.ItemName} - not enough items in inventory");
            return;
        }

        // Start placement in building system
        if (placementSystem != null)
        {
            placementSystem.StartPlacement(item.BuildPieceData.ID);
        }

        Debug.Log($"Selected item: {item.ItemName} for building");
    }

    // Update quantity display for a specific item
    private void UpdateItemQuantity(int itemID, int newQuantity)
    {
        if (itemUIElements.TryGetValue(itemID, out InventoryItemUI itemUI))
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
        foreach (var kvp in itemUIElements)
        {
            if (kvp.Value != null && kvp.Value.gameObject != null)
            {
                DestroyImmediate(kvp.Value.gameObject);
            }
        }
        itemUIElements.Clear();
    }

    public void RefreshUI()
    {
        InitializeUI();
    }
}