using UnityEngine;
using NeonRush;

public class SpeedRing : MonoBehaviour
{
    public float boostVelocity = 22f;
    public int bonusScore = 150;
    public float cooldown = 2.0f;

    private float lastTriggerTime = -10f;
    private Light ringLight;

    void Awake()
    {
        ringLight = GetComponentInChildren<Light>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastTriggerTime < cooldown) return;

        VehicleController vc = other.GetComponent<VehicleController>() ?? other.GetComponentInParent<VehicleController>();
        Rigidbody rb = other.GetComponent<Rigidbody>() ?? other.GetComponentInParent<Rigidbody>();

        if (vc != null && rb != null)
        {
            lastTriggerTime = Time.time;

            // Apply forward velocity impulse in the direction of the ring
            Vector3 forwardBoost = transform.forward * boostVelocity;
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x * 0.5f + forwardBoost.x,
                Mathf.Max(rb.linearVelocity.y, 4f),
                rb.linearVelocity.z * 0.5f + forwardBoost.z
            );

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(bonusScore);
            }

            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowNotification("⚡ SUPERSONIC RING BOOST! +150 PTS", 2.2f);
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayAntiGravityActive();
            }

            // Visual ring pulse
            if (ringLight != null)
            {
                ringLight.color = Color.white;
                ringLight.intensity = 10f;
                Invoke(nameof(ResetLight), 0.35f);
            }

            Debug.Log("Player passed through SpeedRing! Supersonic boost applied.");
        }
    }

    void ResetLight()
    {
        if (ringLight != null)
        {
            ringLight.color = new Color(0f, 0.95f, 1f);
            ringLight.intensity = 4.0f;
        }
    }
}
