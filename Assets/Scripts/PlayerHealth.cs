using UnityEngine;
using NeonRush;

public class PlayerHealth : MonoBehaviour
{
    [Header("Assignment Required Health System")]
    public int health;
    public int maxHealth = 100;
    public int damageAmount = 5;
    public int policeDamageAmount = 20;

    [Header("Invulnerability & Damage Cooldown")]
    public float invulnerabilityDuration = 1.0f;
    private float invulnerabilityTimer = 0f;
    private bool isDead = false;

    void Awake()
    {
        damageAmount = 5;
        policeDamageAmount = 20;
    }

    void Start()
    {
        // Assignment Requirement: Initialize health in Start()
        health = 100;
        damageAmount = 5;
        policeDamageAmount = 20;
        isDead = false;
        invulnerabilityTimer = 0f;
        Debug.Log("PlayerHealth initialized in Start(): health = " + health);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHealth(health, maxHealth);
        }
    }

    void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
    }

    // Assignment Requirement: Collision Handling with OnCollisionEnter()
    void OnCollisionEnter(Collision collision)
    {
        if (isDead || invulnerabilityTimer > 0f) return;

        // Ignore ground and road surfaces
        string colName = collision.gameObject.name.ToLower();
        if (colName.Contains("road") || colName.Contains("floor") || colName.Contains("slab") ||
            colName.Contains("track") || colName.Contains("ground") || colName.Contains("terrain") ||
            colName.Contains("runway") || colName.Contains("grid") || colName.Contains("surface"))
        {
            return;
        }

        if (collision.contactCount > 0)
        {
            ContactPoint contact = collision.GetContact(0);
            if (contact.normal.y > 0.55f)
            {
                return; // Horizontal ground landing
            }
        }

        // Safe check for damaging objects
        bool isHazard = false;
        try
        {
            isHazard = collision.gameObject.CompareTag("Hazard") || collision.gameObject.CompareTag("Enemy");
        }
        catch
        {
            isHazard = false;
        }

        bool isPolice = false;
        try
        {
            isPolice = collision.gameObject.CompareTag("Enemy") || collision.gameObject.name.Contains("Police") || collision.gameObject.name.Contains("Enemy");
        }
        catch { }

        if (isPolice)
        {
            TakeDamage(policeDamageAmount);
            return;
        }

        if (isHazard)
        {
            TakeDamage(damageAmount);
        }
    }

    public void RestoreHealth(int amount)
    {
        if (isDead || health <= 0) return;
        health = Mathf.Clamp(health + amount, 0, maxHealth);
        Debug.Log("Player health restored! Current health: " + health);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHealth(health, maxHealth);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead || invulnerabilityTimer > 0f) return;
        invulnerabilityTimer = invulnerabilityDuration;

        health = Mathf.Clamp(health - amount, 0, maxHealth);
        Debug.Log("Collision damage taken! Current health: " + health);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHealth(health, maxHealth);
            GameUIManager.Instance.TriggerHitFlash();
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHazardHit();
        }

        // Assignment Requirement: Destroy on zero health
        if (health <= 0)
        {
            health = 0;
            isDead = true;
            Debug.Log("Health depleted to zero. Destroying GameObject as required by assignment.");

            if (GameManager.Instance != null)
            {
                if (gameObject.name.Contains("Player"))
                {
                    GameManager.Instance.OnPlayerDied();
                }
                else
                {
                    GameManager.Instance.OnVehicleDestroyed();
                }
            }

            Destroy(gameObject, 0.05f);
        }
    }
}
