// Module: Checkpoint   Owner: Member 2   Purpose: Track checkpoint for lap counting and race progress

using UnityEngine;

namespace NeonRush
{
    /// <summary>
    /// Checkpoint that ships pass through to count laps and race progress
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint Settings")]
        [SerializeField] private int checkpointIndex = 0;
        [SerializeField] private bool isFinishLine = false;
        [SerializeField] private Color gizmoColor = Color.yellow;

        public int Index => checkpointIndex;
        public bool IsFinishLine => isFinishLine;

        private void OnTriggerEnter(Collider other)
        {
            RaceManager raceManager = FindAnyObjectByType<RaceManager>();
            if (raceManager == null) return;

            ShipController ship = other.GetComponent<ShipController>() ?? other.GetComponentInParent<ShipController>();
            if (ship != null)
            {
                raceManager.OnShipPassCheckpoint(ship, this);
                return;
            }

            VehicleController vehicle = other.GetComponent<VehicleController>() ?? other.GetComponentInParent<VehicleController>();
            if (vehicle != null)
            {
                raceManager.OnVehiclePassCheckpoint(vehicle, this);
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
        }
    }
}
