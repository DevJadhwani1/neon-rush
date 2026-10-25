// Module: Vehicles   Owner: Member 1   Purpose: Armor-based health system with respawn at checkpoints

using UnityEngine;
using System.Collections;

namespace NeonRush
{
    public class VehicleHealth : MonoBehaviour
    {
        [Header("Armor System")]
        public int currentArmor = 100;
        public int maxArmor = 100;
        public bool isInvulnerable = false;
        
        [Header("Respawn")]
        public Vector3 lastCheckpointPosition = Vector3.zero;
        public Quaternion lastCheckpointRotation = Quaternion.identity;
        private bool isRespawning = false;
        
        [Header("Stuck Detection")]
        private float stuckTimer = 0f;
        private Vector3 lastPosition;
        private const float STUCK_TIME_THRESHOLD = 2f;
        private const float STUCK_DISTANCE_THRESHOLD = 1f;
        
        private VehicleController vehicleController;
        private Rigidbody rb;
        
        void Start()
        {
            vehicleController = GetComponent<VehicleController>();
            rb = GetComponent<Rigidbody>();
            
            if (vehicleController != null)
            {
                maxArmor = vehicleController.maxHealth;
                currentArmor = maxArmor;
            }
            
            // Set initial checkpoint
            lastCheckpointPosition = transform.position;
            lastCheckpointRotation = transform.rotation;
            lastPosition = transform.position;
        }

        void Update()
        {
            if (isRespawning) return;
            
            // Check if vehicle is stuck or flipped
            CheckIfStuckOrFlipped();
        }

        void CheckIfStuckOrFlipped()
        {
            // Do not check for stuck while countdown is running or game is not in active racing
            if (GameManager.Instance != null && (!GameManager.Instance.isGameActive || GameManager.Instance.currentState != GameState.Racing))
            {
                stuckTimer = 0f;
                lastPosition = transform.position;
                return;
            }

            // Check if flipped (upside down)
            float dotUp = Vector3.Dot(transform.up, Vector3.up);
            if (dotUp < -0.5f)
            {
                Debug.Log("[VehicleHealth] Vehicle flipped! Respawning...");
                QuickRespawn();
                return;
            }
            
            // Check if stuck (not moving)
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            if (distanceMoved < STUCK_DISTANCE_THRESHOLD)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer >= STUCK_TIME_THRESHOLD)
                {
                    Debug.Log("[VehicleHealth] Vehicle stuck! Respawning...");
                    QuickRespawn();
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
            }
            
            lastPosition = transform.position;
        }

        public void RestoreHealth(int amount)
        {
            if (vehicleController != null)
            {
                vehicleController.RestoreHealth(amount);
                currentArmor = vehicleController.health;
            }
            else
            {
                currentArmor = Mathf.Clamp(currentArmor + amount, 0, maxArmor);
                if (GameUIManager.Instance != null)
                {
                    GameUIManager.Instance.UpdateHealth(currentArmor, maxArmor);
                }
            }
        }

        public void TakeDamage(int amount)
        {
            if (isInvulnerable || isRespawning) return;
            
            // Check if vehicle has anti-gravity active (invulnerable)
            if (vehicleController != null && vehicleController.isAntiGravityActive)
            {
                Debug.Log("[VehicleHealth] Anti-gravity shield absorbed damage!");
                return;
            }
            
            // === NEON RUSH: Check power-up shield ===
            PowerUpSystem powerUps = GetComponent<PowerUpSystem>();
            if (powerUps != null && powerUps.IsShieldActive())
            {
                Debug.Log("[VehicleHealth] Power-up shield absorbed damage!");
                return;
            }
            // =========================================
            
            currentArmor -= amount;
            
            // Update UI
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdateHealth(currentArmor, maxArmor);
                GameUIManager.Instance.TriggerHitFlash();
            }
            
            Debug.Log($"[VehicleHealth] Took {amount} damage. Armor: {currentArmor}/{maxArmor}");
            
            if (currentArmor <= 0)
            {
                currentArmor = 0;
                Die();
            }
        }

        void Die()
        {
            if (isRespawning) return;
            
            Debug.Log("[VehicleHealth] Vehicle destroyed! Respawning...");
            QuickRespawn();
        }

        public void QuickRespawn()
        {
            if (isRespawning) return;
            StartCoroutine(QuickRespawnSequence());
        }

        IEnumerator QuickRespawnSequence()
        {
            isRespawning = true;
            isInvulnerable = true;
            
            // Quick fade effect (no explosion particles)
            yield return new WaitForSeconds(0.3f);
            
            // Respawn at checkpoint
            transform.position = lastCheckpointPosition + Vector3.up * 2f;
            transform.rotation = lastCheckpointRotation;
            
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * 15f; // Give forward speed
                rb.angularVelocity = Vector3.zero;
            }
            
            // Restore armor (full if respawning from stuck/flip)
            currentArmor = maxArmor;
            
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdateHealth(currentArmor, maxArmor);
            }
            
            // 1 second invulnerability
            yield return new WaitForSeconds(1f);
            
            isInvulnerable = false;
            isRespawning = false;
            stuckTimer = 0f;
            
            Debug.Log("[VehicleHealth] Respawned with full speed");
        }

        IEnumerator RespawnSequence()
        {
            isRespawning = true;
            
            // === NEON RUSH: Explosion effect ===
            CreateExplosionEffect();
            // ===================================
            
            // Disable vehicle temporarily
            if (vehicleController != null)
            {
                vehicleController.enabled = false;
            }
            
            // Hide vehicle
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }
            
            // Apply time penalty
            if (GameManager.Instance != null)
            {
                GameManager.Instance.raceTime += 3f; // 3 second penalty
            }
            
            // Wait 2 seconds
            yield return new WaitForSeconds(2f);
            
            // Respawn at checkpoint
            transform.position = lastCheckpointPosition;
            transform.rotation = lastCheckpointRotation;
            
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            // Restore armor (partial)
            currentArmor = maxArmor / 2;
            
            // Show vehicle
            foreach (Renderer r in renderers)
            {
                r.enabled = true;
            }
            
            // Re-enable vehicle
            if (vehicleController != null)
            {
                vehicleController.enabled = true;
            }
            
            // Brief invulnerability
            isInvulnerable = true;
            yield return new WaitForSeconds(2f);
            isInvulnerable = false;
            
            isRespawning = false;
            
            Debug.Log("[VehicleHealth] Respawned with 50% armor.");
        }

        public void UpdateCheckpoint(Vector3 position, Quaternion rotation)
        {
            lastCheckpointPosition = position;
            lastCheckpointRotation = rotation;
        }

        void CreateExplosionEffect()
        {
            // Create simple explosion particles
            GameObject explosion = new GameObject("Explosion");
            explosion.transform.position = transform.position + Vector3.up;
            
            ParticleSystem ps = explosion.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 5f;
            main.startSize = 0.3f;
            main.startColor = new Color(1f, 0.5f, 0f);
            main.maxParticles = 50;
            
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 50) });
            
            ps.Play();
            Destroy(explosion, 2f);
        }

        public void InstantKill()
        {
            // For pits and instant-death hazards
            currentArmor = 0;
            Die();
        }
    }
}
