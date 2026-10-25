// Module: Vehicles   Owner: Member 1   Purpose: Anti-gravity ship physics with magnetic track adhesion

using UnityEngine;

namespace NeonRush
{
    [RequireComponent(typeof(Rigidbody))]
    public class ShipController : MonoBehaviour
    {
        [Header("Ship Data")]
        public ShipData shipData;
        
        [Header("Hover Settings")]
        public float hoverHeight = 1.5f;
        public float hoverForce = 50f;
        public float hoverDamping = 5f;
        public LayerMask groundLayer = ~0;
        
        [Header("Movement")]
        public float currentSpeed = 0f;
        public float targetSpeed = 0f;
        public float steeringInput = 0f;
        public float airBrakeInput = 0f;
        
        [Header("Energy System")]
        public float currentEnergy = 100f;
        public float maxEnergy = 100f;
        public bool isBoosting = false;
        public float boostEnergyDrain = 15f;
        public float wallDrainRate = 10f;
        
        [Header("References")]
        public Transform[] hoverPoints;
        public TrailRenderer[] engineTrails;
        public Light[] engineLights;
        
        private Rigidbody rb;
        private Vector3 trackNormal = Vector3.up;
        private bool isGrounded = false;
        private float tiltAmount = 0f;
        
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = shipData ? shipData.weight : 300f;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.None;
            
            maxEnergy = shipData ? shipData.energyCapacity : 100f;
            currentEnergy = maxEnergy;
            
            // Auto-create hover points if missing
            if (hoverPoints == null || hoverPoints.Length == 0)
            {
                GameObject fl = new GameObject("HoverFL");
                GameObject fr = new GameObject("HoverFR");
                GameObject rl = new GameObject("HoverRL");
                GameObject rr = new GameObject("HoverRR");
                
                fl.transform.SetParent(transform);
                fr.transform.SetParent(transform);
                rl.transform.SetParent(transform);
                rr.transform.SetParent(transform);
                
                fl.transform.localPosition = new Vector3(-0.5f, 0f, 1f);
                fr.transform.localPosition = new Vector3(0.5f, 0f, 1f);
                rl.transform.localPosition = new Vector3(-0.5f, 0f, -1f);
                rr.transform.localPosition = new Vector3(0.5f, 0f, -1f);
                
                hoverPoints = new Transform[] { fl.transform, fr.transform, rl.transform, rr.transform };
            }
        }
        
        void FixedUpdate()
        {
            ApplyHoverForce();
            AlignToTrackNormal();
            ApplyMovement();
            ApplyEnergyDrain();
            UpdateVisuals();
        }
        
        void ApplyHoverForce()
        {
            isGrounded = false;
            trackNormal = Vector3.up;
            int groundedPoints = 0;
            
            foreach (Transform hoverPoint in hoverPoints)
            {
                if (Physics.Raycast(hoverPoint.position, -hoverPoint.up, out RaycastHit hit, hoverHeight * 2f, groundLayer))
                {
                    isGrounded = true;
                    groundedPoints++;
                    trackNormal += hit.normal;
                    
                    float hoverError = hoverHeight - hit.distance;
                    float upVelocity = Vector3.Dot(rb.linearVelocity, hoverPoint.up);
                    float force = (hoverError * hoverForce) - (upVelocity * hoverDamping);
                    
                    rb.AddForceAtPosition(hoverPoint.up * force, hoverPoint.position, ForceMode.Acceleration);
                }
            }
            
            if (groundedPoints > 0)
            {
                trackNormal = (trackNormal / (groundedPoints + 1)).normalized;
            }
        }
        
        void AlignToTrackNormal()
        {
            // Gravity follows the track surface normal (magnetic adhesion)
            Vector3 gravityDir = -trackNormal;
            rb.AddForce(gravityDir * 20f, ForceMode.Acceleration);
            
            // Smoothly rotate ship to align with track normal
            Quaternion targetRot = Quaternion.FromToRotation(transform.up, trackNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 5f);
        }
        
        void ApplyMovement()
        {
            if (!isGrounded) return;
            
            float acceleration = shipData ? shipData.acceleration : 100f;
            float maxSpd = shipData ? shipData.maxSpeed : 350f;
            float handling = shipData ? shipData.handling : 1.5f;
            
            // Boost multiplier
            if (isBoosting && currentEnergy > 0f)
            {
                targetSpeed = maxSpd * 1.3f;
            }
            else
            {
                targetSpeed = maxSpd;
            }
            
            // Apply air brakes
            if (airBrakeInput > 0.1f)
            {
                targetSpeed *= 0.5f;
            }
            
            // Accelerate towards target speed
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
            
            // Apply forward movement
            Vector3 forwardForce = transform.forward * currentSpeed;
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, forwardForce, Time.fixedDeltaTime * 3f);
            
            // Steering
            if (Mathf.Abs(steeringInput) > 0.01f)
            {
                float turnSpeed = handling * 100f;
                float turnAmount = steeringInput * turnSpeed * Time.fixedDeltaTime;
                
                // Tilt into turns
                tiltAmount = Mathf.Lerp(tiltAmount, -steeringInput * 15f, Time.fixedDeltaTime * 5f);
                
                Quaternion turnRot = Quaternion.AngleAxis(turnAmount, transform.up);
                rb.MoveRotation(rb.rotation * turnRot);
            }
            else
            {
                tiltAmount = Mathf.Lerp(tiltAmount, 0f, Time.fixedDeltaTime * 5f);
            }
            
            // Apply tilt
            transform.Rotate(Vector3.forward, tiltAmount * Time.fixedDeltaTime, Space.Self);
        }
        
        void ApplyEnergyDrain()
        {
            if (isBoosting && currentEnergy > 0f)
            {
                currentEnergy -= boostEnergyDrain * Time.fixedDeltaTime;
                currentEnergy = Mathf.Max(0f, currentEnergy);
            }
        }
        
        void UpdateVisuals()
        {
            // Update engine trails
            if (engineTrails != null)
            {
                foreach (var trail in engineTrails)
                {
                    if (trail != null)
                    {
                        trail.emitting = currentSpeed > 50f;
                    }
                }
            }
            
            // Update engine lights
            if (engineLights != null)
            {
                foreach (var light in engineLights)
                {
                    if (light != null)
                    {
                        light.intensity = Mathf.Lerp(1f, 3f, currentSpeed / (shipData ? shipData.maxSpeed : 350f));
                    }
                }
            }
        }
        
        public void SetInput(float steering, float airBrake, bool boost)
        {
            steeringInput = Mathf.Clamp(steering, -1f, 1f);
            airBrakeInput = Mathf.Clamp01(airBrake);
            isBoosting = boost && currentEnergy > 0f;
        }
        
        public void AddEnergy(float amount)
        {
            currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        }
        
        public void TakeDamage(float amount)
        {
            currentEnergy = Mathf.Max(0f, currentEnergy - amount);
            
            if (currentEnergy <= 0f)
            {
                OnShipDestroyed();
            }
        }
        
        void OnShipDestroyed()
        {
            Debug.Log($"{shipData.shipName} destroyed!");
            // TODO: Explosion effect and respawn
        }
        
        void OnCollisionStay(Collision collision)
        {
            // Drain energy when scraping walls
            if (collision.gameObject.CompareTag("Wall"))
            {
                TakeDamage(wallDrainRate * Time.fixedDeltaTime);
            }
        }
    }
}
