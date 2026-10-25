// Module: Vehicles   Owner: Member 1   Purpose: Applies vehicle data + upgrades to VehicleController at race start

using UnityEngine;

namespace NeonRush
{
    public class VehicleStatsApplier : MonoBehaviour
    {
        private VehicleController vehicleController;
        
        void Start()
        {
            ApplyStatsToVehicle();
        }

        public void ApplyStatsToVehicle()
        {
            // Get the existing VehicleController
            vehicleController = GetComponent<VehicleController>();
            if (vehicleController == null)
            {
                Debug.LogWarning("[VehicleStatsApplier] No VehicleController found on this GameObject.");
                return;
            }
            
            // Load vehicle data
            string vehicleId = GameManager.Instance != null ? GameManager.Instance.selectedVehicleId : "VOLT";
            VehicleData data = LoadVehicleData(vehicleId);
            
            if (data == null)
            {
                Debug.LogWarning($"[VehicleStatsApplier] Could not load data for vehicle '{vehicleId}', using defaults.");
                return;
            }
            
            // Get upgrade levels
            int[] upgrades = SaveSystem.GetUpgradeLevels(vehicleId);
            
            // Apply stats with upgrades (each level gives +8%)
            float engineMult = 1f + (upgrades[0] * 0.08f);
            float handlingMult = 1f + (upgrades[1] * 0.08f);
            float boostMult = 1f + (upgrades[2] * 0.08f);
            float armorMult = 1f + (upgrades[3] * 0.08f);
            
            // === NEON RUSH: Apply vehicle stats ===
            vehicleController.maxSpeed = data.maxSpeed * engineMult;
            vehicleController.acceleration = data.acceleration * engineMult;
            vehicleController.turnSpeed = data.turnRate * handlingMult;
            vehicleController.antiGravityDuration = data.boostDuration * boostMult;
            vehicleController.maxHealth = Mathf.RoundToInt(data.maxArmor * armorMult);
            vehicleController.health = vehicleController.maxHealth;
            // ==========================================
            
            // Rebuild visual appearance
            RebuildVehicleVisuals(data);
            
            Debug.Log($"[VehicleStatsApplier] Applied {data.displayName} stats with upgrades [{upgrades[0]},{upgrades[1]},{upgrades[2]},{upgrades[3]}]");
        }

        void RebuildVehicleVisuals(VehicleData data)
        {
            // Remove old visual children (keep DriverSeat, ExitPoint, UnderglowLight)
            Transform[] children = GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child != transform && 
                    child.name != "DriverSeat" && 
                    child.name != "ExitPoint" && 
                    child.name != "UnderglowLight")
                {
                    Destroy(child.gameObject);
                }
            }
            
            // Create new body based on shape preset
            switch (data.shapePreset)
            {
                case VehicleShape.Interceptor:
                    CreateInterceptorBody(data.bodyColor, data.glowColor);
                    break;
                case VehicleShape.Brick:
                    CreateBrickBody(data.bodyColor, data.glowColor);
                    break;
                case VehicleShape.Blade:
                    CreateBladeBody(data.bodyColor, data.glowColor);
                    break;
            }
        }

        void CreateInterceptorBody(Color body, Color glow)
        {
            // Main body
            GameObject bodyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bodyObj.name = "Body";
            bodyObj.transform.SetParent(transform, false);
            bodyObj.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            bodyObj.transform.localScale = new Vector3(1.8f, 0.6f, 3.6f);
            SetColor(bodyObj, body);
            RemoveCollider(bodyObj);
            
            // Cockpit
            GameObject cockpit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cockpit.name = "Cockpit";
            cockpit.transform.SetParent(transform, false);
            cockpit.transform.localPosition = new Vector3(0f, 0.7f, 0.2f);
            cockpit.transform.localScale = new Vector3(1.2f, 0.4f, 1.4f);
            SetColor(cockpit, glow * 0.7f);
            RemoveCollider(cockpit);
            
            // Wings
            CreateWing("Wing_Left", new Vector3(-1.2f, 0.2f, -0.8f), new Vector3(0.8f, 0.1f, 1.2f), glow);
            CreateWing("Wing_Right", new Vector3(1.2f, 0.2f, -0.8f), new Vector3(0.8f, 0.1f, 1.2f), glow);
        }

        void CreateBrickBody(Color body, Color glow)
        {
            // Boxy main body
            GameObject bodyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bodyObj.name = "Body";
            bodyObj.transform.SetParent(transform, false);
            bodyObj.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            bodyObj.transform.localScale = new Vector3(2.2f, 0.8f, 3.8f);
            SetColor(bodyObj, body);
            RemoveCollider(bodyObj);
            
            // Armored top
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "Armor";
            top.transform.SetParent(transform, false);
            top.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            top.transform.localScale = new Vector3(2.0f, 0.3f, 3.4f);
            SetColor(top, body * 0.7f);
            RemoveCollider(top);
            
            // Small glow accents
            CreateWing("Accent_Left", new Vector3(-1.0f, 0.3f, 0f), new Vector3(0.2f, 0.5f, 2.0f), glow);
            CreateWing("Accent_Right", new Vector3(1.0f, 0.3f, 0f), new Vector3(0.2f, 0.5f, 2.0f), glow);
        }

        void CreateBladeBody(Color body, Color glow)
        {
            // Sleek narrow body
            GameObject bodyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bodyObj.name = "Body";
            bodyObj.transform.SetParent(transform, false);
            bodyObj.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            bodyObj.transform.localScale = new Vector3(1.4f, 0.5f, 4.2f);
            SetColor(bodyObj, body);
            RemoveCollider(bodyObj);
            
            // Sharp nose
            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Nose";
            nose.transform.SetParent(transform, false);
            nose.transform.localPosition = new Vector3(0f, 0.25f, 2.5f);
            nose.transform.localScale = new Vector3(0.8f, 0.3f, 1.0f);
            SetColor(nose, glow);
            RemoveCollider(nose);
            
            // Large angled wings
            CreateWing("Wing_Left", new Vector3(-1.4f, 0.15f, -1.0f), new Vector3(1.2f, 0.08f, 2.0f), glow);
            CreateWing("Wing_Right", new Vector3(1.4f, 0.15f, -1.0f), new Vector3(1.2f, 0.08f, 2.0f), glow);
        }

        void CreateWing(string name, Vector3 pos, Vector3 scale, Color color)
        {
            GameObject wing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wing.name = name;
            wing.transform.SetParent(transform, false);
            wing.transform.localPosition = pos;
            wing.transform.localScale = scale;
            SetColor(wing, color);
            RemoveCollider(wing);
        }

        void SetColor(GameObject obj, Color color)
        {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                mat.SetFloat("_Metallic", 0.5f);
                mat.SetFloat("_Glossiness", 0.8f);
                if (color.r > 0.8f || color.g > 0.8f || color.b > 0.8f)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", color * 0.3f);
                }
                rend.material = mat;
            }
        }

        void RemoveCollider(GameObject obj)
        {
            Collider col = obj.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        VehicleData LoadVehicleData(string vehicleId)
        {
            // Try to load from Resources or Assets
            VehicleData data = Resources.Load<VehicleData>($"Data/Vehicles/{vehicleId}");
            
#if UNITY_EDITOR
            if (data == null)
            {
                data = UnityEditor.AssetDatabase.LoadAssetAtPath<VehicleData>($"Assets/Data/Vehicles/{vehicleId}.asset");
            }
#endif
            
            return data;
        }
    }
}
