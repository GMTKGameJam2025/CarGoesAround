using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Inventory Preset", menuName = "Inventory/Inventory Preset")]
public class InventoryPreset : ScriptableObject
{
    [SerializeField] private string presetName;
    [SerializeField] private List<InventoryItem> items = new List<InventoryItem>();

    public string PresetName => presetName;
    public List<InventoryItem> Items => items;

    // Why the fuck not both ways?
    // Get an item by its BuildPieceData ID
    public InventoryItem GetItemByID(int id)
    {
        return items.Find(item => item.BuildPieceData != null && item.BuildPieceData.ID == id);
    }

    // Get an item by its name
    public InventoryItem GetItemByName(string itemName)
    {
        return items.Find(item => item.ItemName == itemName);
    }
}