using UnityEngine;
using Mirror;
using System.Collections;

public class Rock : NetworkBehaviour, IDestructible
{
    [Header("Vida")]
    public float MaxHealth = 100f;
    [SyncVar(hook = nameof(OnHealthChanged))]
    public float CurrentHealth;

    [Header("Ferramenta")]
    public ToolType requiredTool = ToolType.Pickaxe;
    public ToolType RequiredTool => requiredTool;

    [Header("Fragmentos")]
    public GameObject fragmentPrefab;
    public Mesh[] fragmentMeshes;
    public int fragmentCount = 5;
    public float fragmentForce = 3f;

    [Header("Recurso")]
    public int itemID = 0;
    public int minAmount = 1;
    public int maxAmount = 3;

    [Header("Feedback")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;

    private Vector3 originalPosition;

    public override void OnStartServer()
    {
        CurrentHealth = MaxHealth;
    }

    void Start()
    {
        originalPosition = transform.position;
    }

    void OnHealthChanged(float oldValue, float newValue) { }

    [Server]
    public void TakeDamage(float amount)
    {
        if (CurrentHealth <= 0) return;
        CurrentHealth -= amount;
        RpcOnDamage();

        if (CurrentHealth <= 0)
            Break();
    }

    [ClientRpc]
    void RpcOnDamage()
    {
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            transform.position = originalPosition + Random.insideUnitSphere * shakeMagnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
    }

    [Server]
    void Break()
    {
        SpawnFragments();
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    void SpawnFragments()
    {
        for (int i = 0; i < fragmentCount; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 0.5f + Vector3.up * 0.5f;
            Quaternion randomRot = Random.rotation;

            GameObject frag = Instantiate(fragmentPrefab, spawnPos, randomRot);

            if (fragmentMeshes != null && fragmentMeshes.Length > 0)
            {
                MeshFilter mf = frag.GetComponent<MeshFilter>();
                if (mf != null)
                    mf.mesh = fragmentMeshes[Random.Range(0, fragmentMeshes.Length)];
            }

            if (frag.TryGetComponent(out PickupItem pickup))
            {
                pickup.itemID = itemID;
                pickup.amount = Random.Range(minAmount, maxAmount + 1);
            }

            if (frag.TryGetComponent(out Rigidbody rig))
                rig.AddForce(Random.insideUnitSphere * fragmentForce, ForceMode.Impulse);

            NetworkServer.Spawn(frag);
        }
    }
}