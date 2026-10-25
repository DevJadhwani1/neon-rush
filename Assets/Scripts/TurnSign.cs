using UnityEngine;

public class TurnSign : MonoBehaviour
{
    public bool turnRight = true;
    public float pulseSpeed = 6f;

    private Light signLight;
    private MeshRenderer[] chevronRenderers;

    void Awake()
    {
        signLight = GetComponentInChildren<Light>();
        chevronRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    void Update()
    {
        // Pulsate chevron glow intensity
        float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * pulseSpeed);
        if (signLight != null)
        {
            signLight.intensity = pulse * 5f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        VehicleController vc = other.GetComponent<VehicleController>() ?? other.GetComponentInParent<VehicleController>();
        if (vc != null && GameUIManager.Instance != null)
        {
            string msg = turnRight ? ">>> SHARP RIGHT TURN — DRIFT [SHIFT] >>>" : "<<< SHARP LEFT TURN — DRIFT [SHIFT] <<<";
            GameUIManager.Instance.ShowNotification(msg, 2.0f);
        }
    }
}
