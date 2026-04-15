using UnityEngine;

public class EquipItem : MonoBehaviour
{
    private ItemData data;
    private float lastAttackTime;
    private bool isLocal;
    private Camera playerCam;
    private Equipment equipment;

    public void Initialize(ItemData itemData, bool local, Equipment eq)
    {
        data = itemData;
        isLocal = local;
        equipment = eq;
        if (isLocal) playerCam = Camera.main;
    }

    void Update()
    {
        if (!isLocal || data == null) return;
        if (InventoryUI.isOpen) return;

        // Verifica se o item é usável (tem dano ou é ferramenta)
        if (Input.GetMouseButton(0))
        {
            if (Time.time >= lastAttackTime + data.attackCooldown)
            {
                lastAttackTime = Time.time;
                TryHit();
            }
        }
    }

    void TryHit()
    {
        Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, data.range))
        {
            // O CmdHit já faz a validação de ToolType no servidor, o que é ótimo
            equipment.CmdHit(hit.collider.gameObject, data.damage, data.toolType);
        }
    }
}