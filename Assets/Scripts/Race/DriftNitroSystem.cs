// Module: Race   Owner: Member 1   Purpose: Enhanced drift-to-nitro system with multiple tiers

using UnityEngine;

namespace NeonRush
{
    public class DriftNitroSystem : MonoBehaviour
    {
        [Header("Drift Nitro Settings")]
        public bool isDrifting = false;
        public float driftCharge = 0f; // 0 to 1
        public int nitroTier = 0; // 0 = none, 1 = blue, 2 = orange, 3 = pink
        
        public float blueTierThreshold = 1.0f;
        public float orangeTierThreshold = 2.0f;
        public float pinkTierThreshold = 3.5f;
        
        public float blueTierBoost = 8f;
        public float orangeTierBoost = 15f;
        public float pinkTierBoost = 25f;
        public float nitroDuration = 1.5f;
        
        private float driftTimer = 0f;
        private int driftDirection = 0; // -1 left, 1 right
        private bool nitroActive = false;
        private float nitroTimer = 0f;
        
        private VehicleController vehicle;
        private Rigidbody rb;
        private Light underglowLight;
        
        void Start()
        {
            vehicle = GetComponent<VehicleController>();
            rb = GetComponent<Rigidbody>();
            
            Transform ug = transform.Find("UnderglowLight");
            if (ug != null)
            {
                underglowLight = ug.GetComponent<Light>();
            }
        }

        void Update()
        {
            if (vehicle == null || !vehicle.isPlayerInVehicle) return;
            
            UpdateDriftState();
            UpdateNitroState();
        }

        void UpdateDriftState()
        {
            float speed = (rb != null) ? Vector3.Dot(rb.linearVelocity, transform.forward) : 0f;
            float turnInput = GameInput.GetHorizontal();
            bool driftInput = GameInput.GetDrift() || GameInput.GetBrake();
            
            bool wantsDrift = driftInput && Mathf.Abs(turnInput) > 0.3f && speed > 10f;
            
            if (wantsDrift && !vehicle.isAntiGravityActive)
            {
                if (!isDrifting)
                {
                    isDrifting = true;
                    driftTimer = 0f;
                    driftDirection = (turnInput >= 0f) ? 1 : -1;
                }
                
                driftTimer += Time.deltaTime;
                driftCharge = driftTimer;
                
                // Determine tier
                if (driftTimer >= pinkTierThreshold)
                {
                    nitroTier = 3;
                    if (underglowLight != null) underglowLight.color = new Color(1f, 0.1f, 0.85f); // Pink
                }
                else if (driftTimer >= orangeTierThreshold)
                {
                    nitroTier = 2;
                    if (underglowLight != null) underglowLight.color = new Color(1f, 0.5f, 0f); // Orange
                }
                else if (driftTimer >= blueTierThreshold)
                {
                    nitroTier = 1;
                    if (underglowLight != null) underglowLight.color = new Color(0.2f, 0.5f, 1f); // Blue
                }
                else
                {
                    nitroTier = 0;
                    if (underglowLight != null) underglowLight.color = new Color(0f, 0.9f, 1f); // Cyan
                }
            }
            else
            {
                if (isDrifting)
                {
                    // Release drift - trigger nitro if tier > 0
                    if (nitroTier > 0)
                    {
                        ActivateNitro();
                    }
                    
                    isDrifting = false;
                    driftTimer = 0f;
                    driftCharge = 0f;
                    nitroTier = 0;
                    
                    if (underglowLight != null && !vehicle.isAntiGravityActive)
                    {
                        underglowLight.color = new Color(0f, 0.9f, 1f); // Cyan
                    }
                }
            }
        }

        void ActivateNitro()
        {
            nitroActive = true;
            nitroTimer = nitroDuration;
            
            float boost = 0f;
            string tierName = "";
            
            switch (nitroTier)
            {
                case 1:
                    boost = blueTierBoost;
                    tierName = "BLUE";
                    break;
                case 2:
                    boost = orangeTierBoost;
                    tierName = "ORANGE";
                    break;
                case 3:
                    boost = pinkTierBoost;
                    tierName = "PINK";
                    break;
            }
            
            if (rb != null)
            {
                rb.AddForce(transform.forward * boost, ForceMode.VelocityChange);
            }
            
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowNotification("🔥 " + tierName + " NITRO! 🔥", 1.5f);
            }
            
            Debug.Log("[DriftNitroSystem] Activated " + tierName + " nitro boost: " + boost);
        }

        void UpdateNitroState()
        {
            if (nitroActive)
            {
                nitroTimer -= Time.deltaTime;
                if (nitroTimer <= 0f)
                {
                    nitroActive = false;
                }
            }
        }

        public bool IsNitroActive()
        {
            return nitroActive;
        }

        public int GetCurrentTier()
        {
            return nitroTier;
        }
    }
}
