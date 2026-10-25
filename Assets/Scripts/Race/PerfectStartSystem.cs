// Module: Race   Owner: Member 1   Purpose: Perfect start timing mechanic (green zone boost)

using UnityEngine;

namespace NeonRush
{
    public class PerfectStartSystem : MonoBehaviour
    {
        [Header("Perfect Start Settings")]
        public bool isCountdownActive = false;
        public float countdownTime = 3f; // 3-2-1-GO timing
        public float perfectStartWindow = 0.3f; // 0.3s window around "GO"
        public float perfectStartBonus = 25f; // Speed boost on perfect start
        
        private float countdownTimer = 0f;
        private bool hasCheckedForPerfectStart = false;
        private VehicleController vehicle;
        
        void Start()
        {
            vehicle = GetComponent<VehicleController>();
            if (vehicle == null)
            {
                Debug.LogWarning("[PerfectStartSystem] No VehicleController found on " + gameObject.name);
            }
        }

        public void StartCountdown()
        {
            isCountdownActive = true;
            countdownTimer = 0f;
            hasCheckedForPerfectStart = false;
            
            // Disable vehicle input during countdown
            if (vehicle != null)
            {
                vehicle.isPlayerInVehicle = false;
            }
        }

        void Update()
        {
            if (!isCountdownActive) return;
            
            countdownTimer += Time.deltaTime;
            
            // Check for accelerator input in perfect window (just before GO)
            float perfectStartTiming = countdownTime - perfectStartWindow;
            
            if (!hasCheckedForPerfectStart && countdownTimer >= perfectStartTiming && countdownTimer < countdownTime)
            {
                // Check if player is holding accelerator
                if (GameInput.GetVertical() > 0.1f)
                {
                    hasCheckedForPerfectStart = true;
                    TriggerPerfectStart();
                }
            }
            
            // End countdown at GO
            if (countdownTimer >= countdownTime)
            {
                isCountdownActive = false;
                
                // Re-enable vehicle input
                if (vehicle != null)
                {
                    vehicle.isPlayerInVehicle = true;
                }
                
                // If player didn't get perfect start but held accelerator too early, show message
                if (!hasCheckedForPerfectStart && GameInput.GetVertical() > 0.1f)
                {
                    if (GameUIManager.Instance != null)
                    {
                        GameUIManager.Instance.ShowNotification("TOO EARLY!", 1.5f);
                    }
                }
            }
        }

        void TriggerPerfectStart()
        {
            Debug.Log("[PerfectStartSystem] PERFECT START!");
            
            // Apply speed boost
            if (vehicle != null)
            {
                Rigidbody rb = vehicle.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(vehicle.transform.forward * perfectStartBonus, ForceMode.VelocityChange);
                }
            }
            
            // Visual feedback
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowNotification("⚡ PERFECT START! ⚡", 2f);
            }
            
            // Award bonus credits
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(100);
            }
        }

        public int GetCountdownNumber()
        {
            if (!isCountdownActive) return 0;
            
            int remaining = Mathf.CeilToInt(countdownTime - countdownTimer);
            return Mathf.Clamp(remaining, 0, 3);
        }

        public bool IsGo()
        {
            return isCountdownActive && countdownTimer >= countdownTime;
        }
    }
}
