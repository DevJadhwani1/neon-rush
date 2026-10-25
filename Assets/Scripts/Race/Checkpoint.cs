// Module: Checkpoint   Owner: Member 2   Purpose: Track checkpoint for lap counting, progression, and safe recovery anchoring

using UnityEngine;

namespace NeonRush
{
    /// <summary>
    /// Checkpoint that racers pass through to count laps, sequence progression, and anchor safe track recovery.
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint Settings")]
        [SerializeField] private int checkpointIndex = 0;
        [SerializeField] private bool isFinishLine = false;
        [SerializeField] private Color gizmoColor = Color.yellow;

        [Header("Safe Respawn Transform")]
        [SerializeField] private Vector3 safeRespawnPosition = Vector3.zero;
        [SerializeField] private Quaternion safeRespawnRotation = Quaternion.identity;

        public int Index => checkpointIndex;
        public bool IsFinishLine => isFinishLine;

        public void SetCheckpointData(int index, bool finishLine, Vector3 safePos, Quaternion safeRot)
        {
            checkpointIndex = index;
            isFinishLine = finishLine;
            safeRespawnPosition = safePos;
            safeRespawnRotation = safeRot;
        }

        public Vector3 GetSafeRespawnPosition()
        {
            if (safeRespawnPosition != Vector3.zero)
            {
                return safeRespawnPosition;
            }
            return transform.position + Vector3.up * 0.5f;
        }

        public Quaternion GetSafeRespawnRotation()
        {
            if (safeRespawnRotation != Quaternion.identity)
            {
                return safeRespawnRotation;
            }
            return transform.rotation;
        }

        private void OnTriggerEnter(Collider other)
        {
            RaceManager raceManager = FindAnyObjectByType<RaceManager>();

            // 1. Ship racer interaction
            ShipController ship = other.GetComponent<ShipController>() ?? other.GetComponentInParent<ShipController>();
            if (ship != null)
            {
                if (raceManager != null)
                {
                    raceManager.OnShipPassCheckpoint(ship, this);
                }
                return;
            }

            // 2. Cyber vehicle racer interaction
            VehicleController vehicle = other.GetComponent<VehicleController>() ?? other.GetComponentInParent<VehicleController>();
            if (vehicle != null)
            {
                bool isLegitimateSequence = true;
                if (raceManager != null)
                {
                    isLegitimateSequence = raceManager.OnVehiclePassCheckpoint(vehicle, this);
                }

                // If checkpoint passage is legitimate or pre-race, anchor recovery safely
                if (isLegitimateSequence || raceManager == null)
                {
                    vehicle.UpdateLastCheckpoint(this);

                    VehicleHealth vh = vehicle.GetComponent<VehicleHealth>();
                    if (vh != null)
                    {
                        vh.UpdateCheckpoint(GetSafeRespawnPosition(), GetSafeRespawnRotation());
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isFinishLine ? Color.green : gizmoColor;
            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }

            // Draw safe respawn point indicator
            Vector3 safePos = GetSafeRespawnPosition();
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(safePos, 0.8f);
            Gizmos.DrawRay(safePos, GetSafeRespawnRotation() * Vector3.forward * 3f);
        }
    }
}
