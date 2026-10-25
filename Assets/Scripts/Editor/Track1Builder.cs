// Module: Track 1 Builder   Owner: Member 2   Purpose: Build "Neon Downtown" race track

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;

namespace NeonRush
{
    /// <summary>
    /// Editor tool to build Track 1: Neon Downtown
    /// </summary>
    public class Track1Builder : EditorWindow
    {
        [MenuItem("Tools/Neon Rush/Build Track 1 - Neon Downtown")]
        public static void BuildTrack1()
        {
            // Create track container
            GameObject trackRoot = new GameObject("Track_NeonDowntown");

            // Create spline container
            GameObject splineObj = new GameObject("TrackSpline");
            splineObj.transform.SetParent(trackRoot.transform);
            
            SplineContainer splineContainer = splineObj.AddComponent<SplineContainer>();
            Spline spline = splineContainer.Spline;

            // Clear default knots
            spline.Clear();

            // Create a figure-8 downtown circuit with elevation changes
            Vector3[] controlPoints = new Vector3[]
            {
                // Start/finish straight
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 0f, 50f),
                
                // Right turn onto elevated section
                new Vector3(30f, 5f, 80f),
                new Vector3(60f, 10f, 80f),
                
                // High-speed straight with banking
                new Vector3(90f, 10f, 60f),
                new Vector3(90f, 10f, 20f),
                
                // Sharp hairpin descent
                new Vector3(80f, 5f, -10f),
                new Vector3(60f, 0f, -20f),
                
                // Chicane section
                new Vector3(40f, 0f, -15f),
                new Vector3(30f, 0f, -5f),
                new Vector3(20f, 0f, -10f),
                
                // Final corner back to start
                new Vector3(0f, 0f, -20f),
                new Vector3(-10f, 0f, -10f),
                new Vector3(0f, 0f, 0f) // Close loop
            };

            // Add knots to spline
            foreach (Vector3 point in controlPoints)
            {
                BezierKnot knot = new BezierKnot(point);
                spline.Add(knot);
            }

            spline.Closed = true;

            // Add TrackBuilder component
            TrackBuilder trackBuilder = splineObj.AddComponent<TrackBuilder>();
            trackBuilder.trackWidth = 15f;
            trackBuilder.checkpointInterval = 20f;
            trackBuilder.autoPlaceCheckpoints = true;
            trackBuilder.autoPlaceBoostPads = true;

            // Build the track mesh
            EditorApplication.delayCall += () =>
            {
                if (trackBuilder != null)
                {
                    // Trigger track generation (would normally be via editor button)
                    Debug.Log("Track spline created. Use TrackBuilder inspector to generate mesh.");
                }
            };

            // Add lighting
            CreateTrackLighting(trackRoot.transform);

            // Add environment objects
            CreateEnvironment(trackRoot.transform);

            Selection.activeGameObject = trackRoot;
            EditorGUIUtility.PingObject(trackRoot);

            Debug.Log("✅ Track 1 'Neon Downtown' spline created");
        }

        static void CreateTrackLighting(Transform parent)
        {
            // Main directional light (cyberpunk moon)
            GameObject mainLightObj = new GameObject("MainLight");
            mainLightObj.transform.SetParent(parent);
            mainLightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            
            Light mainLight = mainLightObj.AddComponent<Light>();
            mainLight.type = LightType.Directional;
            mainLight.color = new Color(0.5f, 0.6f, 1f); // Cool blue
            mainLight.intensity = 1.5f;
            mainLight.shadows = LightShadows.Soft;

            // Neon accent lights along track
            for (int i = 0; i < 20; i++)
            {
                GameObject neonLightObj = new GameObject($"NeonLight_{i}");
                neonLightObj.transform.SetParent(parent);
                
                float angle = i * (360f / 20f) * Mathf.Deg2Rad;
                float radius = 50f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 2f, Mathf.Sin(angle) * radius);
                neonLightObj.transform.position = pos;

                Light neonLight = neonLightObj.AddComponent<Light>();
                neonLight.type = LightType.Point;
                neonLight.color = i % 2 == 0 ? Color.cyan : Color.magenta;
                neonLight.intensity = 3f;
                neonLight.range = 20f;
                neonLight.shadows = LightShadows.None;
            }

            Debug.Log("Created track lighting");
        }

        static void CreateEnvironment(Transform parent)
        {
            // Create simple building blocks around track
            GameObject buildings = new GameObject("Buildings");
            buildings.transform.SetParent(parent);

            Material buildingMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            buildingMat.color = new Color(0.1f, 0.1f, 0.15f);
            buildingMat.SetFloat("_Metallic", 0.3f);
            buildingMat.SetFloat("_Smoothness", 0.7f);

            // Ring of buildings
            for (int i = 0; i < 30; i++)
            {
                float angle = i * (360f / 30f) * Mathf.Deg2Rad;
                float radius = 120f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 15f, Mathf.Sin(angle) * radius);

                GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
                building.name = $"Building_{i}";
                building.transform.SetParent(buildings.transform);
                building.transform.position = pos;
                building.transform.localScale = new Vector3(
                    Random.Range(8f, 15f),
                    Random.Range(20f, 60f),
                    Random.Range(8f, 15f)
                );
                
                building.GetComponent<MeshRenderer>().material = buildingMat;

                // Add neon window lights
                Material windowMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                windowMat.EnableKeyword("_EMISSION");
                Color neonColor = i % 3 == 0 ? Color.cyan : (i % 3 == 1 ? Color.magenta : Color.yellow);
                windowMat.SetColor("_EmissionColor", neonColor * 2f);
                windowMat.color = neonColor * 0.5f;
            }

            Debug.Log("Created environment buildings");
        }
    }
}
#endif
