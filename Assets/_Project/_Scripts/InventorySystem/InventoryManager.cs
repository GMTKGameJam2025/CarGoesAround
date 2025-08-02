using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private InventoryPreset inventoryPreset;

    // Runtime inventory data
    private Dictionary<int, int> currentQuantities = new Dictionary<int, int>();

    // Events for UI updates
    public event Action<int, int> OnQuantityChanged; // itemID, newQuantity
    public event Action OnInventoryInitialized;

    public InventoryPreset CurrentPreset => inventoryPreset;

    private void Start()
    {
        InitializeInventory();
    }

    /// Initialize inventory from the preset
    private void InitializeInventory()
    {
        if (inventoryPreset == null)
        {
            Debug.LogError("InventoryManager: No inventory preset assigned!");
            return;
        }

        currentQuantities.Clear();

        foreach (var item in inventoryPreset.Items)
        {
            if (item != null)
            {
                currentQuantities[item.ID] = item.InitialQuantity;
            }
        }

        OnInventoryInitialized?.Invoke();
        Debug.Log($"Inventory initialized with {currentQuantities.Count} items from preset: {inventoryPreset.PresetName}");
    }


    // Check if there's enough quantity of an item
    public bool HasEnoughItems(int itemID, int requiredQuantity = 1)
    {
        return currentQuantities.TryGetValue(itemID, out int currentQty) && currentQty >= requiredQuantity;
    }


    // Consume items (e.g., when placing a building)
    public bool ConsumeItems(int itemID, int quantity = 1)
    {
        if (!HasEnoughItems(itemID, quantity))
        {
            Debug.LogWarning($"Not enough items to consume. ItemID: {itemID}, Required: {quantity}, Available: {GetCurrentQuantity(itemID)}");
            return false;
        }

        currentQuantities[itemID] -= quantity;
        OnQuantityChanged?.Invoke(itemID, currentQuantities[itemID]);

        Debug.Log($"Consumed {quantity} of item {itemID}. Remaining: {currentQuantities[itemID]}");
        return true;
    }


    // Add items back to inventory (e.g., when removing a building)
    public void AddItems(int itemID, int quantity = 1)
    {
        if (currentQuantities.ContainsKey(itemID))
        {
            currentQuantities[itemID] += quantity;
        }
        else
        {
            currentQuantities[itemID] = quantity;
        }

        OnQuantityChanged?.Invoke(itemID, currentQuantities[itemID]);
        Debug.Log($"Added {quantity} of item {itemID}. New total: {currentQuantities[itemID]}");
    }


    // Get current quantity of an item
    public int GetCurrentQuantity(int itemID)
    {
        return currentQuantities.TryGetValue(itemID, out int quantity) ? quantity : 0;
    }


    // Get all current quantities
    public Dictionary<int, int> GetAllQuantities()
    {
        return new Dictionary<int, int>(currentQuantities);
    }


    // Reset inventory to initial state
    public void ResetInventory()
    {
        InitializeInventory();
    }


    // Change the inventory preset useful for different levels)
    public void SetInventoryPreset(InventoryPreset newPreset)
    {
        inventoryPreset = newPreset;
        InitializeInventory();
    }
}