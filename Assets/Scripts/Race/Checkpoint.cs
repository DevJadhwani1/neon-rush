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
            ShipController ship = other.GetComponent<ShipController>();
            if (ship != null)
            {
                RaceManager raceManager = FindAnyObjectByType<RaceManager>();
                if (raceManager != null)
                {
                    raceManager.OnShipPassCheckpoint(ship, this);
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
        }
    }
}
