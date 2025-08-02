using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private InventoryPreset inventoryPreset;

    // Runtime inventory data
    private Dictionary<int, int> _currentQuantities = new Dictionary<int, int>();

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

        _currentQuantities.Clear();

        foreach (var item in inventoryPreset.Items)
        {
            if (item != null)
            {
                _currentQuantities[item.ID] = item.InitialQuantity;
            }
        }

        OnInventoryInitialized?.Invoke();
        Debug.Log($"Inventory initialized with {_currentQuantities.Count} items from preset: {inventoryPreset.PresetName}");
    }


    // Check if there's enough quantity of an item
    public bool HasEnoughItems(int itemID, int requiredQuantity = 1)
    {
        return _currentQuantities.TryGetValue(itemID, out int currentQty) && currentQty >= requiredQuantity;
    }


    // Consume items (e.g., when placing a building)
    public bool ConsumeItems(int itemID, int quantity = 1)
    {
        if (!HasEnoughItems(itemID, quantity))
        {
            Debug.LogWarning($"Not enough items to consume. ItemID: {itemID}, Required: {quantity}, Available: {GetCurrentQuantity(itemID)}");
            return false;
        }

        _currentQuantities[itemID] -= quantity;
        OnQuantityChanged?.Invoke(itemID, _currentQuantities[itemID]);

        Debug.Log($"Consumed {quantity} of item {itemID}. Remaining: {_currentQuantities[itemID]}");
        return true;
    }


    // Add items back to inventory (e.g., when removing a building)
    public void AddItems(int itemID, int quantity = 1)
    {
        if (!_currentQuantities.TryAdd(itemID, quantity))
        {
            _currentQuantities[itemID] += quantity;
        }

        OnQuantityChanged?.Invoke(itemID, _currentQuantities[itemID]);
        Debug.Log($"Added {quantity} of item {itemID}. New total: {_currentQuantities[itemID]}");
    }


    // Get current quantity of an item
    public int GetCurrentQuantity(int itemID)
    {
        return _currentQuantities.GetValueOrDefault(itemID, 0);
    }


    // Get all current quantities
    public Dictionary<int, int> GetAllQuantities()
    {
        return new Dictionary<int, int>(_currentQuantities);
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