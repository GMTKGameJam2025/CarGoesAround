using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private int initialQuantity;
    [SerializeField] private ObjectData buildPieceData;
    
    public string ItemName => itemName;
    public Sprite ItemIcon => itemIcon;
    public int InitialQuantity => initialQuantity;
    public ObjectData BuildPieceData => buildPieceData;
    
    public InventoryItem(string name, Sprite icon, int quantity, ObjectData data)
    {
        itemName = name;
        itemIcon = icon;
        initialQuantity = quantity;
        buildPieceData = data;
    }
}
