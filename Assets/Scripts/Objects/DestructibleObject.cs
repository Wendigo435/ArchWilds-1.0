using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;

public class DestructibleObject : NetworkBehaviour, IDestructible
{
    [Header("Configurações de Vida")]
    public float maxHealth = 100f;
    [SyncVar(hook = nameof(OnHealthChanged))]
    private float currentHealth;

    [Header("Requisitos")]
    public ToolType requiredTool = ToolType.None;
    public ToolType RequiredTool => requiredTool;

    [Header("Visual & Feedback")]
    public GameObject gfxModel;
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.05f;
    private Vector3 originalPos;

    [Header("Loot Aleatório")]
    // Agora temos uma lista de possíveis prefabs para dropar
    public GameObject[] possibleDrops;
    public int minDropAmount = 1;
    public int maxDropAmount = 3;
    public float spawnRadius = 0.5f;

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        if (gfxModel != null) originalPos = gfxModel.transform.localPosition;
    }

    [Server]
    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        RpcPlayHitEffect();

        if (currentHealth <= 0)
        {
            Break();
        }
    }

    [ClientRpc]
    void RpcPlayHitEffect()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeEffect());
    }

    IEnumerator ShakeEffect()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            gfxModel.transform.localPosition = originalPos + Random.insideUnitSphere * shakeMagnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        gfxModel.transform.localPosition = originalPos;
    }

    [Server]
    void Break()
    {
        if (possibleDrops != null && possibleDrops.Length > 0)
        {
            int totalToSpawn = Random.Range(minDropAmount, maxDropAmount + 1);

            for (int i = 0; i < totalToSpawn; i++)
            {
                // SORTEIO: Escolhe um índice aleatório da lista de modelos
                int randomIndex = Random.Range(0, possibleDrops.Length);
                GameObject selectedPrefab = possibleDrops[randomIndex];

                if (selectedPrefab != null)
                {
                    Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius + Vector3.up;
                    GameObject loot = Instantiate(selectedPrefab, randomPos, Quaternion.identity);

                    // Registra o objeto na rede Mirror
                    NetworkServer.Spawn(loot);
                }
            }
        }

        NetworkServer.Destroy(gameObject);
    }

    void OnHealthChanged(float oldV, float newV) { }
}