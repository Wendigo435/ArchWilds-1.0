using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Vida")]
    public float MaxHealth = 100f;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public float CurrentHealth;

    [Header("Barra Flutuante")]
    public Image floatingBarFill;
    private Canvas floatingCanvas;

    [Header("Knockback")]
    public float KBX = 5f;
    public float KBY = 3f;

    private Rigidbody rig;
    private Image hudBarFill;

    void Awake()
    {
        rig = GetComponent<Rigidbody>();
        if (floatingBarFill != null)
            floatingCanvas = floatingBarFill.GetComponentInParent<Canvas>();
    }

    public override void OnStartServer()
    {
        CurrentHealth = MaxHealth;
    }

    public override void OnStartLocalPlayer()
    {
        // Esconde barra flutuante do próprio player
        if (floatingCanvas != null)
            floatingCanvas.enabled = false;

        // Acha a barra na HUD
        GameObject hudBar = GameObject.Find("HudHealthFill");
        if (hudBar != null)
            hudBarFill = hudBar.GetComponent<Image>();

        UpdateUI(CurrentHealth);
    }

    void OnHealthChanged(float oldValue, float newValue)
    {
        UpdateUI(newValue);
    }

    void UpdateUI(float value)
    {
        float fill = value / MaxHealth;

        if (floatingBarFill != null)
            floatingBarFill.fillAmount = fill;

        if (isLocalPlayer && hudBarFill != null)
            hudBarFill.fillAmount = fill;
    }

    void LateUpdate()
    {
        if (floatingCanvas == null || !floatingCanvas.enabled) return;
        if (Camera.main == null) return;
        floatingCanvas.transform.forward = Camera.main.transform.forward;
    }

    [Server]
    public void TakeDamage(float amount, Vector3 damageOrigin)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth -= amount;
        if (CurrentHealth < 0) CurrentHealth = 0;

        RpcKnockback(damageOrigin);

        if (CurrentHealth == 0)
            OnDie();
    }

    [Server]
    void OnDie()
    {
        Debug.Log($"{gameObject.name} morreu!");
        // respawn futuramente aqui
    }

    [ClientRpc]
    void RpcKnockback(Vector3 damageOrigin)
    {
        if (!isLocalPlayer) return;
        Vector3 direction = (transform.position - damageOrigin).normalized;
        Vector3 force = new Vector3(direction.x * KBX, KBY, direction.z * KBX);
        rig.AddForce(force, ForceMode.Impulse);
    }

    [Server]
    public void Heal(float amount)
    {
        if (CurrentHealth <= 0) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
    }
}