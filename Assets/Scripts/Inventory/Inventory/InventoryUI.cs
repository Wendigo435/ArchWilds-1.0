using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject inventoryPanel;

    [Header("Slots — arrasta em ordem 0 a 24")]
    public List<InventorySlot> hotbarSlots = new List<InventorySlot>();
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();

    [Header("Database")]
    public ItemDatabase database;

    public static bool isOpen;
    private PlayerInventory localPlayer;

    [Header("Hotbar Selection")]
    public List<Image> hotbarBorders = new List<Image>();
    public Color selectedColor = Color.white;
    public Color unselectedColor = new Color(1, 1, 1, 0.3f);

    void Awake()
    {
        inventoryPanel.SetActive(false);
        isOpen = false;

        for (int i = 0; i < hotbarBorders.Count; i++)
            if (hotbarBorders[i] != null)
                hotbarBorders[i].color = unselectedColor;
    }


    public void UpdateHotbarSelection(int index)
    {
        for (int i = 0; i < hotbarBorders.Count; i++)
        {
            if (hotbarBorders[i] == null) continue;
            hotbarBorders[i].color = i == index ? selectedColor : unselectedColor;
        }
    }

    public void BindPlayer(PlayerInventory player)
    {
        localPlayer = player;

        for (int i = 0; i < hotbarSlots.Count; i++)
            if (hotbarSlots[i] != null) hotbarSlots[i].myIndex = i;

        for (int i = 0; i < inventorySlots.Count; i++)
            if (inventorySlots[i] != null) inventorySlots[i].myIndex = i + hotbarSlots.Count;

        UpdateHotbarSelection(0);
        RefreshUI(player.inventory);
    }

    public void ToggleUI()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

        if (!isOpen)
        {
            foreach (var slot in hotbarSlots) if (slot != null) slot.ResetIconPosition();
            foreach (var slot in inventorySlots) if (slot != null) slot.ResetIconPosition();
        }
    }

    public void RefreshUI(SyncList<Item> inventory)
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (hotbarSlots[i] == null) continue;
            Item item = inventory[i];
            if (item.IsEmpty)
            {
                hotbarSlots[i].ClearSlot();
            }
            else
            {
                ItemData data = database.GetItemByID(item.itemID);
                if (data != null) hotbarSlots[i].UpdateSlot(data.itemName, item.amount, data.icon);
            }
        }

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] == null) continue;
            int invIndex = i + hotbarSlots.Count;
            Item item = inventory[invIndex];
            if (item.IsEmpty)
            {
                inventorySlots[i].ClearSlot();
            }
            else
            {
                ItemData data = database.GetItemByID(item.itemID);
                if (data != null) inventorySlots[i].UpdateSlot(data.itemName, item.amount, data.icon);
            }
        }
    }

    public void RequestSwap(int fromIndex, int toIndex)
    {
        if (localPlayer != null)
            localPlayer.CmdSwapItems(fromIndex, toIndex);
    }

    public void DropItem(int index)
    {
        if (localPlayer != null)
            localPlayer.CmdDropItem(index, localPlayer.transform.position);
    }
}