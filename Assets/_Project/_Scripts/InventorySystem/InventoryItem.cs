using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    [SerializeField] private int itemId;
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private int initialQuantity;

    public int ID => itemId;
    public string ItemName => itemName;
    public Sprite ItemIcon => itemIcon;
    public int InitialQuantity => initialQuantity;

    public InventoryItem(int id, string name, Sprite icon, int quantity)
    {
        itemId = id;
        itemName = name;
        itemIcon = icon;
        initialQuantity = quantity;
    }
}
