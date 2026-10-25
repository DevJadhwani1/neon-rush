using UnityEngine;

public class BoostPad : MonoBehaviour
{
    public float boostForce = 28f;
    private float cooldown = 0f;

    void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (cooldown > 0f) return;

        VehicleController vc = other.GetComponent<VehicleController>() ?? other.GetComponentInParent<VehicleController>();
        if (vc != null)
        {
            Rigidbody rb = vc.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(vc.transform.forward * boostForce, ForceMode.VelocityChange);
                cooldown = 1.5f;

                if (GameUIManager.Instance != null)
                {
                    GameUIManager.Instance.ShowNotification("⚡ NITRO BOOST +50 PTS!", 2.0f);
                }

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddScore(50);
                }

                Debug.Log("Nitro Boost Pad triggered!");
            }
        }
    }
}
