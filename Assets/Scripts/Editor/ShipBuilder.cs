// Module: Ship Builder   Owner: Member 1   Purpose: Editor tool to build ship prefabs with ShipController

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace NeonRush
{
    /// <summary>
    /// Editor tool to build ship prefabs for the 4 teams
    /// </summary>
    public class ShipBuilder : EditorWindow
    {
        [MenuItem("Tools/Neon Rush/Build Ship Prefabs")]
        public static void BuildAllShips()
        {
            // Create 4 team ships
            BuildShip("Velocity", new Color(0f, 1f, 1f, 1f), "Assets/Prefabs/Ship_Velocity.prefab");
            BuildShip("Titan", new Color(1f, 0.5f, 0f, 1f), "Assets/Prefabs/Ship_Titan.prefab");
            BuildShip("Phantom", new Color(1f, 0f, 1f, 1f), "Assets/Prefabs/Ship_Phantom.prefab");
            BuildShip("Helix", new Color(0f, 1f, 0f, 1f), "Assets/Prefabs/Ship_Helix.prefab");

            Debug.Log("✅ All 4 team ship prefabs built successfully");
        }

        static void BuildShip(string teamName, Color teamColor, string prefabPath)
        {
            // Create ship root object
            GameObject ship = new GameObject($"Ship_{teamName}");
            ship.layer = LayerMask.NameToLayer("Default");

            // Add ShipController
            ShipController controller = ship.AddComponent<ShipController>();

            // Add Rigidbody
            Rigidbody rb = ship.AddComponent<Rigidbody>();
            rb.mass = 800f;
            rb.linearDamping = 0.1f;
            rb.angularDamping = 2f;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Build ship visual geometry (simple hover-racer shape)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(ship.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(1.2f, 0.4f, 2.5f);

            // Apply team-colored material
            Material bodyMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            bodyMat.name = $"{teamName}_BodyMat";
            bodyMat.color = teamColor;
            bodyMat.SetFloat("_Metallic", 0.8f);
            bodyMat.SetFloat("_Smoothness", 0.9f);
            bodyMat.EnableKeyword("_EMISSION");
            bodyMat.SetColor("_EmissionColor", teamColor * 0.5f);
            body.GetComponent<MeshRenderer>().material = bodyMat;

            // Nose cone
            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Nose";
            nose.transform.SetParent(ship.transform);
            nose.transform.localPosition = new Vector3(0f, 0f, 1.6f);
            nose.transform.localScale = new Vector3(0.8f, 0.3f, 0.8f);
            nose.GetComponent<MeshRenderer>().material = bodyMat;

            // Cockpit
            GameObject cockpit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cockpit.name = "Cockpit";
            cockpit.transform.SetParent(ship.transform);
            cockpit.transform.localPosition = new Vector3(0f, 0.3f, 0.3f);
            cockpit.transform.localScale = new Vector3(0.8f, 0.5f, 1.2f);
            
            Material glassMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            glassMat.name = "GlassMat";
            glassMat.color = new Color(0.2f, 0.4f, 0.8f, 0.6f);
            glassMat.SetFloat("_Surface", 1); // Transparent
            glassMat.SetFloat("_Metallic", 0.9f);
            glassMat.SetFloat("_Smoothness", 1f);
            cockpit.GetComponent<MeshRenderer>().material = glassMat;

            // Wings
            GameObject wingLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wingLeft.name = "WingLeft";
            wingLeft.transform.SetParent(ship.transform);
            wingLeft.transform.localPosition = new Vector3(-1f, 0f, -0.3f);
            wingLeft.transform.localScale = new Vector3(0.8f, 0.1f, 1.5f);
            wingLeft.GetComponent<MeshRenderer>().material = bodyMat;

            GameObject wingRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wingRight.name = "WingRight";
            wingRight.transform.SetParent(ship.transform);
            wingRight.transform.localPosition = new Vector3(1f, 0f, -0.3f);
            wingRight.transform.localScale = new Vector3(0.8f, 0.1f, 1.5f);
            wingRight.GetComponent<MeshRenderer>().material = bodyMat;

            // Engine pods
            GameObject engineLeft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            engineLeft.name = "EngineLeft";
            engineLeft.transform.SetParent(ship.transform);
            engineLeft.transform.localPosition = new Vector3(-0.7f, -0.1f, -1f);
            engineLeft.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            engineLeft.transform.localScale = new Vector3(0.3f, 0.8f, 0.3f);
            
            Material engineMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            engineMat.name = $"{teamName}_EngineMat";
            engineMat.color = Color.black;
            engineMat.SetFloat("_Metallic", 1f);
            engineMat.SetFloat("_Smoothness", 0.8f);
            engineMat.EnableKeyword("_EMISSION");
            engineMat.SetColor("_EmissionColor", teamColor * 2f);
            engineLeft.GetComponent<MeshRenderer>().material = engineMat;

            GameObject engineRight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            engineRight.name = "EngineRight";
            engineRight.transform.SetParent(ship.transform);
            engineRight.transform.localPosition = new Vector3(0.7f, -0.1f, -1f);
            engineRight.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            engineRight.transform.localScale = new Vector3(0.3f, 0.8f, 0.3f);
            engineRight.GetComponent<MeshRenderer>().material = engineMat;

            // Main collider (replace body collider)
            DestroyImmediate(body.GetComponent<Collider>());
            BoxCollider mainCollider = ship.AddComponent<BoxCollider>();
            mainCollider.center = new Vector3(0f, 0f, 0.2f);
            mainCollider.size = new Vector3(1.5f, 0.6f, 2.8f);

            // Remove colliders from visual children
            foreach (Transform child in ship.transform)
            {
                Collider col = child.GetComponent<Collider>();
                if (col != null) DestroyImmediate(col);
            }

            // Add hover raycasting points (4 corners)
            CreateHoverPoint(ship.transform, new Vector3(-0.6f, -0.2f, 1f)); // Front-left
            CreateHoverPoint(ship.transform, new Vector3(0.6f, -0.2f, 1f));  // Front-right
            CreateHoverPoint(ship.transform, new Vector3(-0.6f, -0.2f, -1f)); // Rear-left
            CreateHoverPoint(ship.transform, new Vector3(0.6f, -0.2f, -1f));  // Rear-right

            // Add ShipInput if this is a player ship (Velocity is default player)
            if (teamName == "Velocity")
            {
                ship.AddComponent<ShipInput>();
            }

            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(ship, prefabPath);
            
            // Clean up scene instance
            DestroyImmediate(ship);

            Debug.Log($"✅ Built {teamName} ship prefab at {prefabPath}");
        }

        static void CreateHoverPoint(Transform parent, Vector3 localPos)
        {
            GameObject hoverPoint = new GameObject("HoverPoint");
            hoverPoint.transform.SetParent(parent);
            hoverPoint.transform.localPosition = localPos;
        }
    }
}
#endif
