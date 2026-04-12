using UnityEngine;
using Mirror;

public class PickupItem : NetworkBehaviour
{
    public int itemID;
    public int amount = 1;

    [Header("Auto Pickup")]
    public bool autoPickup = false;
    public float attractDelay = 1.5f;  // tempo antes de começar a atrair
    public float attractRange = 3f;
    public float attractSpeed = 8f;
    public float pickupRange = 0.5f;

    private Transform target;
    private Rigidbody rig;
    private Collider col;

    private float spawnTime;

    void OnEnable()
    {
        spawnTime = Time.time;
    }
    void Awake()
    {
        rig = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        if (!autoPickup) return;
        if (!isServer) return;

        // Espera o delay antes de começar a atrair
        if (Time.time < spawnTime + attractDelay) return;

        if (target == null)
        {
            FindNearestPlayer();
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist <= attractRange && dist > pickupRange)
        {
            SetAttractMode(true);
            transform.position = Vector3.MoveTowards(transform.position, target.position, attractSpeed * Time.deltaTime);
        }

        if (dist <= pickupRange)
        {
            PlayerInventory inv = target.GetComponent<PlayerInventory>();
            if (inv != null)
                inv.AddItem(new Item(itemID, amount));

            NetworkServer.Destroy(gameObject);
        }
    }

    void SetAttractMode(bool attracting)
    {
        if (rig != null)
        {
            rig.useGravity = !attracting;
            rig.linearVelocity = Vector3.zero;
        }

        if (col != null)
            col.isTrigger = attracting;
    }

    void FindNearestPlayer()
    {
        float nearest = attractRange;
        foreach (var player in FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None))
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < nearest)
            {
                nearest = dist;
                target = player.transform;
            }
        }
    }

    public Item GetNetworkItem()
    {
        return new Item(itemID, amount);
    }
}