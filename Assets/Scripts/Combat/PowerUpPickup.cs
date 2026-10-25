// Module: Combat   Owner: Member 1   Purpose: Rotating power-up cube pickup with random type

using UnityEngine;

namespace NeonRush
{
    public enum PowerUpType
    {
        Shield,       // 4s invulnerability
        EMP,          // Disables enemies within 25m for 3s
        PulseBlaster, // 5 forward shots
        Magnet        // Pulls shards within 15m for 6s
    }

    public class PowerUpPickup : MonoBehaviour
    {
        public PowerUpType powerUpType;
        private bool isCollected = false;
        private float rotationSpeed = 90f;
        private Light glowLight;
        
        void Start()
        {
            // Random power-up type
            powerUpType = (PowerUpType)Random.Range(0, 4);
            
            // Create visual cube
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(transform, false);
            cube.transform.localScale = Vector3.one * 0.8f;
            
            // Color based on type
            Color color = GetPowerUpColor();
            Renderer rend = cube.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color);
                rend.material = mat;
            }
            
            // Remove cube collider (parent has trigger)
            Collider cubeCol = cube.GetComponent<Collider>();
            if (cubeCol != null) Destroy(cubeCol);
            
            // Add glow light
            GameObject lightObj = new GameObject("Glow");
            lightObj.transform.SetParent(transform, false);
            glowLight = lightObj.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.range = 5f;
            glowLight.intensity = 2f;
            glowLight.color = color;
            
            // Ensure trigger collider
            SphereCollider trigger = GetComponent<SphereCollider>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<SphereCollider>();
            }
            trigger.isTrigger = true;
            trigger.radius = 1.5f;
        }

        void Update()
        {
            if (isCollected) return;
            
            // Rotate
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            
            // Bob up/down
            float bob = Mathf.Sin(Time.time * 2f) * 0.2f;
            transform.position = new Vector3(transform.position.x, transform.position.y + bob * Time.deltaTime, transform.position.z);
        }

        void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;
            
            // Check if it's the player's vehicle
            VehicleController vehicle = other.GetComponent<VehicleController>();
            if (vehicle == null) vehicle = other.GetComponentInParent<VehicleController>();
            
            if (vehicle != null && vehicle.isPlayerInVehicle)
            {
                Collect(vehicle.gameObject);
            }
        }

        void Collect(GameObject collector)
        {
            isCollected = true;
            
            // Give power-up to PowerUpSystem
            PowerUpSystem system = collector.GetComponent<PowerUpSystem>();
            if (system == null)
            {
                system = collector.AddComponent<PowerUpSystem>();
            }
            
            system.PickUpPowerUp(powerUpType);
            
            // Visual feedback
            if (glowLight != null)
            {
                glowLight.intensity = 8f;
            }
            
            // Destroy pickup
            Destroy(gameObject, 0.1f);
        }

        Color GetPowerUpColor()
        {
            switch (powerUpType)
            {
                case PowerUpType.Shield: return new Color(0f, 0.8f, 1f); // Cyan
                case PowerUpType.EMP: return new Color(1f, 1f, 0f); // Yellow
                case PowerUpType.PulseBlaster: return new Color(1f, 0.2f, 0f); // Red-orange
                case PowerUpType.Magnet: return new Color(0.5f, 1f, 0f); // Lime
                default: return Color.white;
            }
        }
    }
}
