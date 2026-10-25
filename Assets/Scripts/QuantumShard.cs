using UnityEngine;
using NeonRush;

public class QuantumShard : MonoBehaviour
{
    public float rotationSpeed = 120f;
    public float bobFrequency = 3.5f;
    public float bobAmplitude = 0.35f;
    public int scoreValue = 50;

    [Header("Health Restoration")]
    public int healthRestoreAmount = 25;

    private Vector3 initialPos;
    private float timeOffset;
    private bool isCollected = false;
    private Light shardLight;

    public bool IsCollected => isCollected;

    void Awake()
    {
        initialPos = transform.position;
        timeOffset = Random.Range(0f, 6.28f);

        // Setup glowing light
        GameObject lightObj = new GameObject("ShardGlow");
        lightObj.transform.SetParent(transform, false);
        shardLight = lightObj.AddComponent<Light>();
        shardLight.type = LightType.Point;
        shardLight.range = 5f;
        shardLight.color = new Color(0f, 0.95f, 1f); // Neon Cyan
        shardLight.intensity = 2.5f;

        // Ensure trigger collider
        Collider c = GetComponent<Collider>();
        if (c != null)
        {
            c.isTrigger = true;
        }
        else
        {
            SphereCollider sc = gameObject.AddComponent<SphereCollider>();
            sc.radius = 1.2f;
            sc.isTrigger = true;
        }
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterShard();
        }
    }

    void Update()
    {
        if (isCollected) return;

        // Continuous spin
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Vertical bobbing float
        float newY = initialPos.y + Mathf.Sin((Time.time * bobFrequency) + timeOffset) * bobAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        // Do not restore health or collect after game over or race completion
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.currentState == GameState.GameOver ||
                GameManager.Instance.currentState == GameState.Finish ||
                GameManager.Instance.currentState == GameState.Results ||
                GameManager.Instance.hasWon ||
                !GameManager.Instance.isGameActive)
            {
                return;
            }
        }

        VehicleController vc = other.GetComponent<VehicleController>() ?? other.GetComponentInParent<VehicleController>();
        if (vc == null && (other.name.Contains("Vehicle") || other.transform.root.name.Contains("Vehicle")))
        {
            vc = other.transform.root.GetComponentInChildren<VehicleController>();
        }

        PlayerHealth ph = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();

        if (vc != null)
        {
            Collect(vc, null);
        }
        else if (ph != null)
        {
            Collect(null, ph);
        }
        else if (other.CompareTag("Player") || other.name.Contains("Player"))
        {
            Collect();
        }
    }

    public void Collect()
    {
        VehicleController vc = FindAnyObjectByType<VehicleController>();
        PlayerHealth ph = (vc == null) ? FindAnyObjectByType<PlayerHealth>() : null;
        Collect(vc, ph);
    }

    public void Collect(VehicleController vc, PlayerHealth ph)
    {
        if (isCollected) return;
        isCollected = true;

        // Immediately disable collider so duplicate triggers within the frame are rejected
        Collider c = GetComponent<Collider>();
        if (c != null) c.enabled = false;

        // Authoritative health restoration capped at 100
        if (vc != null)
        {
            if (!vc.isDestroyed && vc.health > 0)
            {
                vc.RestoreHealth(healthRestoreAmount);
                var vh = vc.GetComponent<VehicleHealth>();
                if (vh != null)
                {
                    vh.currentArmor = Mathf.Clamp(vh.currentArmor + healthRestoreAmount, 0, vh.maxArmor);
                }
            }
        }
        else if (ph != null)
        {
            if (ph.health > 0)
            {
                ph.RestoreHealth(healthRestoreAmount);
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCollectedShard(scoreValue);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnergyPickup();
        }

        // Brief flash before destroy
        if (shardLight != null)
        {
            shardLight.intensity = 8.0f;
            shardLight.color = Color.white;
        }

        // Hide mesh immediately
        MeshRenderer r = GetComponent<MeshRenderer>();
        if (r != null) r.enabled = false;

        Destroy(gameObject, 0.15f);
    }
}
