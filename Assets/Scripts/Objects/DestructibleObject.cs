using UnityEngine;
using Mirror;
using System.Collections;

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
    public GameObject gfxModel; // O modelo visual que pode sumir ou tremer
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.05f;
    private Vector3 originalPos;

    [Header("Loot / Fragmentos")]
    public GameObject dropPrefab; // O que vai spawnar quando quebrar
    public int minDrop = 1;
    public int maxDrop = 3;
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
        // Aqui você pode adicionar som de impacto genérico ou partículas
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
        // Spawna o loot configurado
        int amount = Random.Range(minDrop, maxDrop + 1);
        for (int i = 0; i < amount; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius + Vector3.up;
            GameObject loot = Instantiate(dropPrefab, randomPos, Quaternion.identity);

            // Se o seu dropPrefab tiver o script de Pickup, você pode configurar o ID aqui
            // if(loot.TryGetComponent(out PickupItem p)) p.SetData(...);

            NetworkServer.Spawn(loot);
        }

        NetworkServer.Destroy(gameObject);
    }

    void OnHealthChanged(float oldV, float newV)
    {
        // Hook para garantir que quem entrar no meio da partida veja o estado atual se necessário
    }
}