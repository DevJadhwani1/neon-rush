// Module: Combat   Owner: Member 1   Purpose: Manages power-up inventory and activation

using UnityEngine;
using System.Collections.Generic;

namespace NeonRush
{
    public class PowerUpSystem : MonoBehaviour
    {
        private Queue<PowerUpType> powerUpInventory = new Queue<PowerUpType>(3);
        private int maxPowerUps = 3;
        
        // Active effects
        private bool shieldActive = false;
        private float shieldTimer = 0f;
        private float shieldDuration = 4f;
        
        private bool magnetActive = false;
        private float magnetTimer = 0f;
        private float magnetDuration = 6f;
        private float magnetRange = 15f;
        
        // References
        private VehicleController vehicle;
        private Light shieldGlowLight;
        
        void Awake()
        {
            vehicle = GetComponent<VehicleController>();
            if (vehicle == null)
            {
                Debug.LogWarning("[PowerUpSystem] No VehicleController found on " + gameObject.name);
            }
        }

        void Update()
        {
            // Update active effect timers
            if (shieldActive)
            {
                shieldTimer -= Time.deltaTime;
                if (shieldTimer <= 0f)
                {
                    DeactivateShield();
                }
            }
            
            if (magnetActive)
            {
                magnetTimer -= Time.deltaTime;
                if (magnetTimer <= 0f)
                {
                    magnetActive = false;
                }
                else
                {
                    PullNearbyShards();
                }
            }
        }

        public void PickUpPowerUp(PowerUpType type)
        {
            if (powerUpInventory.Count >= maxPowerUps)
            {
                return; // Inventory full
            }
            
            powerUpInventory.Enqueue(type);
            
            // TODO: Update UI to show power-up icon
            Debug.Log("[PowerUpSystem] Picked up: " + type);
        }

        public void UsePowerUp()
        {
            if (powerUpInventory.Count == 0) return;
            
            PowerUpType type = powerUpInventory.Dequeue();
            
            switch (type)
            {
                case PowerUpType.Shield:
                    ActivateShield();
                    break;
                case PowerUpType.EMP:
                    ActivateEMP();
                    break;
                case PowerUpType.PulseBlaster:
                    ActivatePulseBlaster();
                    break;
                case PowerUpType.Magnet:
                    ActivateMagnet();
                    break;
            }
            
            // TODO: Update UI to remove used power-up icon
            Debug.Log("[PowerUpSystem] Used: " + type);
        }

        void ActivateShield()
        {
            shieldActive = true;
            shieldTimer = shieldDuration;
            
            // Create shield visual
            if (shieldGlowLight == null)
            {
                GameObject lightObj = new GameObject("ShieldGlow");
                lightObj.transform.SetParent(transform, false);
                lightObj.transform.localPosition = Vector3.zero;
                shieldGlowLight = lightObj.AddComponent<Light>();
                shieldGlowLight.type = LightType.Point;
                shieldGlowLight.range = 8f;
                shieldGlowLight.color = new Color(0f, 0.8f, 1f); // Cyan
            }
            shieldGlowLight.intensity = 6f;
            shieldGlowLight.enabled = true;
        }

        void DeactivateShield()
        {
            shieldActive = false;
            if (shieldGlowLight != null)
            {
                shieldGlowLight.enabled = false;
            }
        }

        void ActivateEMP()
        {
            // Disable all enemies within range
            Collider[] hits = Physics.OverlapSphere(transform.position, 25f);
            int disabledCount = 0;
            
            foreach (Collider hit in hits)
            {
                EnemyController enemy = hit.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.Disable(3f);
                    disabledCount++;
                }
                
                // Also affects police vehicles
                PoliceVehicle police = hit.GetComponent<PoliceVehicle>();
                if (police != null)
                {
                    police.Disable(3f);
                    disabledCount++;
                }
            }
            
            Debug.Log("[PowerUpSystem] EMP disabled " + disabledCount + " enemies");
        }

        void ActivatePulseBlaster()
        {
            // Fire 5 shots forward
            for (int i = 0; i < 5; i++)
            {
                float delay = i * 0.2f;
                StartCoroutine(FirePulseShot(delay));
            }
        }

        System.Collections.IEnumerator FirePulseShot(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Create projectile
            GameObject shot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shot.name = "PulseShot";
            shot.transform.position = transform.position + transform.forward * 3f + Vector3.up * 1f;
            shot.transform.localScale = Vector3.one * 0.5f;
            
            // Material
            Renderer rend = shot.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 0.2f, 0f); // Red-orange
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(1f, 0.2f, 0f) * 2f);
                rend.material = mat;
            }
            
            // Physics
            Rigidbody shotRb = shot.AddComponent<Rigidbody>();
            shotRb.mass = 1f;
            shotRb.useGravity = false;
            shotRb.linearVelocity = transform.forward * 50f;
            
            // Collision detection
            shot.layer = LayerMask.NameToLayer("Default");
            PulseShotBehavior behavior = shot.AddComponent<PulseShotBehavior>();
            
            // Auto-destroy after 3 seconds
            Destroy(shot, 3f);
        }

        void ActivateMagnet()
        {
            magnetActive = true;
            magnetTimer = magnetDuration;
        }

        void PullNearbyShards()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, magnetRange);
            
            foreach (Collider hit in hits)
            {
                QuantumShard shard = hit.GetComponent<QuantumShard>();
                if (shard != null)
                {
                    // Pull shard towards vehicle
                    Vector3 direction = (transform.position - shard.transform.position).normalized;
                    float pullForce = 15f;
                    shard.transform.position += direction * pullForce * Time.deltaTime;
                }
            }
        }

        public bool IsShieldActive()
        {
            return shieldActive;
        }

        public int GetPowerUpCount()
        {
            return powerUpInventory.Count;
        }
    }

    // Helper component for pulse shot collision
    public class PulseShotBehavior : MonoBehaviour
    {
        void OnCollisionEnter(Collision collision)
        {
            // Damage enemies
            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(50); // 50 damage per shot
            }
            
            PoliceVehicle police = collision.gameObject.GetComponent<PoliceVehicle>();
            if (police != null)
            {
                // Police vehicles take damage too
                police.TakeDamage(50);
            }
            
            // Destroy shot on impact
            Destroy(gameObject);
        }
    }
}
