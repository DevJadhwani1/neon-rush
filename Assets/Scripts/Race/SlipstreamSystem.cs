// Module: Race   Owner: Member 1   Purpose: Slipstream speed boost when drafting behind vehicles

using UnityEngine;

namespace NeonRush
{
    public class SlipstreamSystem : MonoBehaviour
    {
        [Header("Slipstream Settings")]
        public float detectionDistance = 15f;
        public float detectionAngle = 25f; // Cone angle
        public float slipstreamBoost = 5f; // Speed bonus
        public float slipstreamDuration = 0.5f; // How long boost lasts after leaving slipstream
        
        private bool inSlipstream = false;
        private float slipstreamTimer = 0f;
        private VehicleController vehicle;
        private Rigidbody rb;
        
        void Start()
        {
            vehicle = GetComponent<VehicleController>();
            rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            if (vehicle == null || !vehicle.isPlayerInVehicle || rb == null) return;
            
            CheckForSlipstream();
            ApplySlipstreamBoost();
        }

        void CheckForSlipstream()
        {
            bool foundSlipstream = false;
            
            // Raycast forward to find vehicles
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, detectionDistance))
            {
                // Check if hit is a vehicle
                VehicleController targetVehicle = hit.collider.GetComponent<VehicleController>();
                PoliceVehicle policeVehicle = hit.collider.GetComponent<PoliceVehicle>();
                
                if (targetVehicle != null || policeVehicle != null)
                {
                    // Check angle
                    Vector3 directionToTarget = (hit.point - transform.position).normalized;
                    float angle = Vector3.Angle(transform.forward, directionToTarget);
                    
                    if (angle < detectionAngle)
                    {
                        foundSlipstream = true;
                    }
                }
            }
            
            if (foundSlipstream)
            {
                if (!inSlipstream)
                {
                    inSlipstream = true;
                    if (GameUIManager.Instance != null)
                    {
                        GameUIManager.Instance.ShowNotification(">> SLIPSTREAM! >>", 1f);
                    }
                }
                slipstreamTimer = slipstreamDuration;
            }
            else
            {
                if (slipstreamTimer > 0f)
                {
                    slipstreamTimer -= Time.fixedDeltaTime;
                }
                else
                {
                    inSlipstream = false;
                }
            }
        }

        void ApplySlipstreamBoost()
        {
            if (inSlipstream && slipstreamTimer > 0f)
            {
                rb.AddForce(transform.forward * slipstreamBoost, ForceMode.Acceleration);
            }
        }

        public bool IsInSlipstream()
        {
            return inSlipstream && slipstreamTimer > 0f;
        }
    }
}
