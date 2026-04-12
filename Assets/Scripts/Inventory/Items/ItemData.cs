using UnityEngine;

[CreateAssetMenu(fileName = "Novo Item", menuName = "Inventario/Item")]
public class ItemData : ScriptableObject
{
    public int itemID;
    public string itemName;
    public Sprite icon;
    public GameObject worldPrefab;
    public bool stackable = true;
    public GameObject equipPrefab;
    public ToolType toolType;
}