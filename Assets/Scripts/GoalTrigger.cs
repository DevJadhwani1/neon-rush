using UnityEngine;
using NeonRush;

public class GoalTrigger : MonoBehaviour
{
    private bool isTriggered = false;
    public bool IsTriggered => isTriggered;

    void Awake()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            box = gameObject.AddComponent<BoxCollider>();
        }
        box.isTrigger = true;
        // Ensure trigger spans entire drivable track (at least 22m wide, 10m high, 6m deep)
        Vector3 size = box.size;
        if (size.x < 20f) size.x = 22f;
        if (size.y < 8f) size.y = 10f;
        if (size.z < 4f) size.z = 6f;
        box.size = size;

        if (box.center.y < 2f)
        {
            box.center = new Vector3(0f, 5f, 0f);
        }
    }

    public void ResetTrigger()
    {
        isTriggered = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        // Check if vehicle or player passed through (check root, parent, and attached rigidbody)
        VehicleController vehicle = other.GetComponent<VehicleController>() ?? other.GetComponentInParent<VehicleController>();
        if (vehicle == null && other.attachedRigidbody != null)
        {
            vehicle = other.attachedRigidbody.GetComponent<VehicleController>();
        }

        ShipController ship = other.GetComponent<ShipController>() ?? other.GetComponentInParent<ShipController>();
        if (ship == null && other.attachedRigidbody != null)
        {
            ship = other.attachedRigidbody.GetComponent<ShipController>();
        }

        bool isPlayer = false;
        if (vehicle != null)
        {
            isPlayer = vehicle.isPlayerInVehicle || vehicle.CompareTag("Player") || vehicle.name.Contains("Player") || vehicle.name == "Vehicle";
        }
        else if (ship != null)
        {
            isPlayer = ship.GetComponent<ShipInput>() != null || ship.CompareTag("Player");
        }
        else if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            isPlayer = true;
        }

        if (!isPlayer) return;

        isTriggered = true;
        Debug.Log("[GoalTrigger] Finish endpoint reached by: " + other.name);

        RaceManager rm = FindAnyObjectByType<RaceManager>();
        Checkpoint cp = GetComponent<Checkpoint>();

        if (rm != null)
        {
            if (cp != null && vehicle != null)
            {
                bool finished = rm.OnVehiclePassCheckpoint(vehicle, cp);
                if (!finished && (vehicle.isPlayerInVehicle || vehicle.CompareTag("Player")))
                {
                    // Fallback safeguard: If player physically reached the finish arch at Z >= 500m with legitimate race time
                    if (vehicle.transform.position.z >= 500f && rm.RaceTime > 5f)
                    {
                        Debug.Log("[GoalTrigger] Player reached finish line arch at Z >= 500m. Completing race run!");
                        rm.FinishPlayerRun();
                    }
                }
            }
            else
            {
                rm.FinishPlayerRun();
            }
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCourseCompleted();
        }
    }
}
