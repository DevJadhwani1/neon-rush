using UnityEngine;

public class PulsingLaserGate : MonoBehaviour
{
    public float cycleDuration = 3.0f; // 1.5s ON, 1.5s OFF
    public int damageAmount = 5;

    private float timer = 0f;
    private bool isHazardActive = true;
    private BoxCollider col;
    private MeshRenderer beamRenderer;
    private Light gateLight;

    private Material hazardMat;
    private Material clearMat;

    void Awake()
    {
        col = GetComponent<BoxCollider>();
        beamRenderer = GetComponent<MeshRenderer>();
        gateLight = GetComponentInChildren<Light>();

        hazardMat = Resources.Load<Material>("Materials/HazardMaterial") ?? Resources.Load<Material>("HazardMaterial");
        clearMat = Resources.Load<Material>("Materials/NeonCyanGlow") ?? Resources.Load<Material>("NeonCyanGlow");
#if UNITY_EDITOR
        if (hazardMat == null) hazardMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/HazardMaterial.mat");
        if (clearMat == null) clearMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeonCyanGlow.mat");
#endif
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= cycleDuration)
        {
            timer = 0f;
            isHazardActive = !isHazardActive;
            UpdateGateState();
        }
    }

    void UpdateGateState()
    {
        if (col != null)
        {
            col.isTrigger = !isHazardActive; // Non-blocking when inactive
        }

        if (beamRenderer != null)
        {
            beamRenderer.sharedMaterial = isHazardActive ? hazardMat : clearMat;
        }

        if (gateLight != null)
        {
            gateLight.color = isHazardActive ? Color.red : Color.cyan;
            gateLight.intensity = isHazardActive ? 5f : 1.5f;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isHazardActive) return;
        HandleDamage(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isHazardActive) return;
        HandleDamage(other.gameObject);
    }

    void HandleDamage(GameObject target)
    {
        VehicleController vc = target.GetComponent<VehicleController>() ?? target.GetComponentInParent<VehicleController>();
        if (vc != null && !vc.isAntiGravityActive)
        {
            vc.TakeDamage(damageAmount);
            return;
        }

        PlayerHealth ph = target.GetComponent<PlayerHealth>() ?? target.GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damageAmount);
        }
    }
}
