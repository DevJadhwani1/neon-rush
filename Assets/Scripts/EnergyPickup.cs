using UnityEngine;
using NeonRush;

public class EnergyPickup : MonoBehaviour
{
    public float rotationSpeed = 90f;
    public float bobFrequency = 2.5f;
    public float bobAmplitude = 0.35f;
    public int scoreReward = 100;

    private Vector3 initialPosition;
    private Light pickupLight;
    private bool isCollected = false;

    void Start()
    {
        initialPosition = transform.position;

        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        GameObject lObj = new GameObject("PickupGlow");
        lObj.transform.SetParent(transform, false);
        pickupLight = lObj.AddComponent<Light>();
        pickupLight.type = LightType.Point;
        pickupLight.range = 6f;
        pickupLight.color = new Color(0f, 0.9f, 1f);
        pickupLight.intensity = 3.0f;
    }

    void Update()
    {
        if (isCollected) return;

        // Continuous spin
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Bob up and down
        float newY = initialPosition.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        VehicleController vehicle = other.GetComponent<VehicleController>() ?? other.GetComponentInParent<VehicleController>();
        if (vehicle != null)
        {
            Collect(vehicle);
        }
    }

    void Collect(VehicleController vehicle)
    {
        isCollected = true;
        vehicle.RechargeAntiGravity();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreReward);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnergyPickup();
        }

        if (pickupLight != null)
        {
            pickupLight.intensity = 8f;
            pickupLight.color = Color.white;
        }

        // Hide all renderers and colliders immediately
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers) r.enabled = false;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = false;

        // Single-use pickup (no infinite farming)
        Destroy(gameObject, 0.15f);
    }
}
