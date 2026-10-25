using UnityEngine;

public class QuantumShard : MonoBehaviour
{
    public float rotationSpeed = 120f;
    public float bobFrequency = 3.5f;
    public float bobAmplitude = 0.35f;
    public int scoreValue = 50;

    private Vector3 initialPos;
    private float timeOffset;
    private bool isCollected = false;
    private Light shardLight;

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

        bool isPlayer = other.CompareTag("Player") ||
                        other.gameObject.name.Contains("Vehicle") ||
                        other.gameObject.name.Contains("Player") ||
                        other.GetComponentInParent<VehicleController>() != null;

        if (isPlayer)
        {
            Collect();
        }
    }

    void Collect()
    {
        isCollected = true;

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
