using UnityEngine;

public class Pickaxe : MonoBehaviour
{
    [Header("Stats")]
    public float damage = 25f;
    public float range = 2.5f;
    public float attackCooldown = 0.5f;

    private float lastAttackTime;
    private bool isLocal;
    private Camera playerCam;

    void Start()
    {
        PlayerMovement player = GetComponentInParent<PlayerMovement>();
        if (player != null && player.isLocalPlayer)
        {
            isLocal = true;
            playerCam = Camera.main;
        }
    }

    void Update()
    {
        if (!isLocal) return;
        if (InventoryUI.isOpen) return;

        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                TryHit();
            }
        }
    }

    void TryHit()
    {
        if (playerCam == null) return;
        Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            if (hit.collider.TryGetComponent(out IDestructible destructible))
            {
                Equipment eq = GetComponentInParent<Equipment>();
                if (eq != null)
                {
                    ItemData data = eq.GetEquippedItemData();
                    ToolType tool = data != null ? data.toolType : ToolType.None;
                    eq.CmdHit(hit.collider.gameObject, damage, tool);
                }
            }
        }
    }
}