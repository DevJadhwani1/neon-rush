// Module: AI   Owner: Member 2   Purpose: AI racer that follows waypoints with rubber-banding

using UnityEngine;
using System.Collections.Generic;

namespace NeonRush
{
    public class AIRacer : MonoBehaviour
    {
        [Header("AI Settings")]
        public float baseSpeed = 35f; // Reduced from 30f - slightly faster
        public float turnSpeed = 4f; // Increased from 3f - better cornering
        public float waypointReachDistance = 15f; // Increased from 10f
        public bool enableRubberBanding = true;
        public float rubberBandMultiplier = 1.15f; // Reduced from 1.2f - less aggressive
        
        [Header("Waypoint Tracking")]
        public List<Vector3> waypoints = new List<Vector3>();
        public int currentWaypointIndex = 0;
        
        private Rigidbody rb;
        private GameObject playerVehicle;
        
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.mass = 400f;
                rb.linearDamping = 0.5f;
                rb.angularDamping = 2f;
            }
            
            playerVehicle = GameObject.Find("Vehicle");
            
            // Register with RaceManager
            RaceManager raceManager = FindAnyObjectByType<RaceManager>();
            if (raceManager != null)
            {
                // RaceManager will track checkpoints automatically
                Debug.Log($"[AIRacer] {gameObject.name} registered with RaceManager");
            }
            
            // Load waypoints from TrackBuilder if available
            LoadWaypointsFromTrack();
        }

        void LoadWaypointsFromTrack()
        {
            // TODO: TrackBuilder needs to expose centerLineWaypoints
            // For now, create simple waypoints along Z-axis
            waypoints.Clear();
            for (int i = 0; i < 20; i++)
            {
                float z = i * 30f;
                waypoints.Add(new Vector3(0f, 1f, z));
            }
            
            Debug.Log($"[AIRacer] Loaded {waypoints.Count} waypoints");
        }

        void FixedUpdate()
        {
            if (waypoints.Count == 0) return;
            
            FollowWaypoint();
            ApplyRubberBanding();
        }

        void FollowWaypoint()
        {
            if (currentWaypointIndex >= waypoints.Count)
            {
                currentWaypointIndex = 0; // Loop back
            }
            
            Vector3 targetWaypoint = waypoints[currentWaypointIndex];
            float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint);
            
            // Check if reached waypoint
            if (distanceToWaypoint < waypointReachDistance)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Count)
                {
                    currentWaypointIndex = 0;
                }
            }
            
            // Steer towards waypoint
            Vector3 direction = (targetWaypoint - transform.position).normalized;
            direction.y = 0f; // Keep on ground plane
            
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
            }
            
            // Move forward
            float currentSpeed = baseSpeed;
            rb.linearVelocity = transform.forward * currentSpeed + Vector3.up * rb.linearVelocity.y;
        }

        void ApplyRubberBanding()
        {
            if (!enableRubberBanding || playerVehicle == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerVehicle.transform.position);
            
            // If too far behind (more than 60m), speed up significantly
            if (distanceToPlayer > 60f)
            {
                rb.linearVelocity *= 1.25f;
            }
            // If somewhat behind (30-60m), speed up moderately
            else if (distanceToPlayer > 30f)
            {
                rb.linearVelocity *= rubberBandMultiplier;
            }
            // If too far ahead (less than 15m), slow down slightly
            else if (distanceToPlayer < 15f)
            {
                rb.linearVelocity *= 0.92f;
            }
        }
    }
}
