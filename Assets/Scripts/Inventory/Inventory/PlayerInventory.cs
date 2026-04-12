using UnityEngine;
using Mirror;

public class PlayerInventory : NetworkBehaviour
{
    [Header("Configurações")]
    public int inventorySize = 24; // 9 hotbar + 27 inventário
    public float PickDistance = 3f;
    public string itemTag = "Item";
    public KeyCode InvKey = KeyCode.Tab;
    public KeyCode PickKey = KeyCode.E;

    [Header("Componentes")]
    public ItemDatabase database;

    public readonly SyncList<Item> inventory = new SyncList<Item>();

    private InventoryUI UI;
    private Camera Cam;

    public override void OnStartServer()
    {
        // Inicializa inventário com slots vazios
        for (int i = 0; i < inventorySize; i++)
            inventory.Add(Item.Empty);
    }

    public override void OnStartLocalPlayer()
    {
        GameObject uiObj = GameObject.FindWithTag("InventoryUI");
        if (uiObj != null) UI = uiObj.GetComponent<InventoryUI>();
        if (UI != null) UI.BindPlayer(this);

        inventory.Callback += OnInventoryUpdated;
        Cam = Camera.main;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(InvKey))
        {
            if (UI == null)
            {
                GameObject uiObj = GameObject.FindWithTag("InventoryUI");
                if (uiObj != null) UI = uiObj.GetComponent<InventoryUI>();
            }
            if (UI != null) UI.ToggleUI();
        }

        if (Input.GetKeyDown(PickKey))
            TryPickup();
    }

    [Server]
    public void AddItem(Item newItem)
    {
        ItemData dataSO = database.GetItemByID(newItem.itemID);

        if (dataSO != null && dataSO.stackable)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].itemID == newItem.itemID)
                {
                    Item temp = inventory[i];
                    temp.amount += newItem.amount;
                    inventory[i] = temp;
                    return;
                }
            }
        }

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].IsEmpty)
            {
                inventory[i] = newItem;
                return;
            }
        }
    }
    void OnInventoryUpdated(SyncList<Item>.Operation op, int index, Item oldItem, Item newItem)
    {
        if (isLocalPlayer && UI != null) UI.RefreshUI(inventory);

        // Atualiza equipamento se o slot afetado for da hotbar
        Equipment eq = GetComponent<Equipment>();
        if (eq != null && index == eq.selectedSlot)
            eq.EquipSlot(eq.selectedSlot);
    }

    void TryPickup()
    {
        if (Cam == null) return;
        Ray ray = Cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, PickDistance))
        {
            if (hit.collider.CompareTag(itemTag))
            {
                if (hit.collider.TryGetComponent(out PickupItem pickup))
                    CmdPickupItem(pickup.gameObject);
            }
        }
    }

    [Command]
    public void CmdPickupItem(GameObject itemObject)
    {
        if (itemObject == null) return;
        if (!itemObject.TryGetComponent(out PickupItem pickup)) return;

        Item newItem = pickup.GetNetworkItem();
        ItemData dataSO = database.GetItemByID(newItem.itemID);

        // Tenta stackar se possível
        if (dataSO != null && dataSO.stackable)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].itemID == newItem.itemID)
                {
                    Item temp = inventory[i];
                    temp.amount += newItem.amount;
                    inventory[i] = temp;
                    NetworkServer.Destroy(itemObject);
                    return;
                }
            }
        }

        // Acha primeiro slot vazio
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].IsEmpty)
            {
                inventory[i] = newItem;
                NetworkServer.Destroy(itemObject);
                return;
            }
        }

        Debug.Log("Inventário cheio!");
    }

    [Command]
    public void CmdSwapItems(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= inventory.Count) return;
        if (indexB < 0 || indexB >= inventory.Count) return;

        Item temp = inventory[indexA];
        inventory[indexA] = inventory[indexB];
        inventory[indexB] = temp;
    }

    [Command]
    public void CmdDropItem(int index, Vector3 playerPosition)
    {
        if (index < 0 || index >= inventory.Count) return;
        if (inventory[index].IsEmpty) return;

        Item networkItem = inventory[index];
        ItemData dataSO = database.GetItemByID(networkItem.itemID);

        if (dataSO != null && dataSO.worldPrefab != null)
        {
            Vector3 spawnPos = playerPosition + Vector3.up * 0.5f;
            GameObject dropped = Instantiate(dataSO.worldPrefab, spawnPos, Quaternion.identity);

            if (dropped.TryGetComponent(out PickupItem pickup))
                pickup.amount = networkItem.amount;

            NetworkServer.Spawn(dropped);
        }

        inventory[index] = Item.Empty;
    }
}