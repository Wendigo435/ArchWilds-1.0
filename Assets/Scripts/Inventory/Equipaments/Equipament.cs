using UnityEngine;
using Mirror;

public class Equipment : NetworkBehaviour
{
    [Header("Referências")]
    public Transform HandR;
    public ItemDatabase database;

    public int selectedSlot = 0;

    [SyncVar(hook = nameof(OnEquippedChanged))]
    private int equippedItemID = -1;

    private GameObject equippedObject;
    private PlayerInventory playerInventory;


    void Awake()
    {
        playerInventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (InventoryUI.isOpen) return;
        HandleHotbarInput();
    }

    void HandleHotbarInput()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedSlot = i;
                EquipSlot(i);
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            selectedSlot = (selectedSlot - 1 + 9) % 9;
            EquipSlot(selectedSlot);
        }
        else if (scroll < 0f)
        {
            selectedSlot = (selectedSlot + 1) % 9;
            EquipSlot(selectedSlot);
        }
    }

    public void EquipSlot(int index)
    {
        GameObject uiObj = GameObject.FindWithTag("InventoryUI");
        if (uiObj != null)
        {
            InventoryUI ui = uiObj.GetComponent<InventoryUI>();
            if (ui != null) ui.UpdateHotbarSelection(index);
        }

        if (playerInventory == null) return;
        if (index >= playerInventory.inventory.Count) return;

        if (equippedObject != null)
        {
            Destroy(equippedObject);
            equippedObject = null;
        }

        Item item = playerInventory.inventory[index];

        if (item.IsEmpty)
        {
            CmdSetEquipped(-1);
            return;
        }

        ItemData data = database.GetItemByID(item.itemID);
        if (data == null || data.equipPrefab == null)
        {
            CmdSetEquipped(-1);
            return;
        }

        equippedObject = Instantiate(data.equipPrefab, HandR);
        equippedObject.transform.localPosition = new Vector3(0f, -0.5f, 0f);

        CmdSetEquipped(item.itemID);
    }

    public ItemData GetEquippedItemData()
    {
        if (playerInventory == null) return null;
        if (selectedSlot >= playerInventory.inventory.Count) return null;
        Item item = playerInventory.inventory[selectedSlot];
        if (item.IsEmpty) return null;
        return database.GetItemByID(item.itemID);
    }

    void OnEquippedChanged(int oldID, int newID)
    {
        if (isLocalPlayer) return;

        if (equippedObject != null)
        {
            Destroy(equippedObject);
            equippedObject = null;
        }

        if (newID == -1) return;

        ItemData data = database.GetItemByID(newID);
        if (data == null || data.equipPrefab == null) return;

        equippedObject = Instantiate(data.equipPrefab, HandR);
        equippedObject.transform.localPosition = Vector3.zero;
        equippedObject.transform.localRotation = Quaternion.identity;
    }

    [Command]
    public void CmdSetEquipped(int itemID)
    {
        equippedItemID = itemID;
    }

    [Command]
    public void CmdHit(GameObject target, float damage, ToolType tool)
    {
        if (target == null) return;
        if (!target.TryGetComponent(out IDestructible destructible)) return;
        if (destructible.RequiredTool != ToolType.None && destructible.RequiredTool != tool) return;
        destructible.TakeDamage(damage);
    }
}