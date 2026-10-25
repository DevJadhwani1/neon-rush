using UnityEngine;

public class RotatingHazard : MonoBehaviour
{
    public float rotationSpeed = 65f;
    public int damageAmount = 25;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    void OnCollisionEnter(Collision col)
    {
        HandleDamage(col.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
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
