// Module: Scene Assembler   Owner: Member 1   Purpose: Complete scene assembly tool for Track 1

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace NeonRush
{
    /// <summary>
    /// Complete scene assembly for Track 1
    /// </summary>
    public class SceneAssembler : EditorWindow
    {
        [MenuItem("Tools/Neon Rush/Assemble Track 1 Scene")]
        public static void AssembleTrack1Scene()
        {
            if (!EditorUtility.DisplayDialog(
                "Assemble Track 1 Scene",
                "This will execute all setup steps:\n\n" +
                "1. Setup URP Rendering\n" +
                "2. Create Post-Processing Volume\n" +
                "3. Create Neon Skybox\n" +
                "4. Build Ship Prefabs\n" +
                "5. Build Track 1 spline\n" +
                "6. Setup Race Cameras\n" +
                "7. Add RaceManager\n\n" +
                "Continue?",
                "Yes", "Cancel"))
            {
                return;
            }

            Debug.Log("=== Starting Track 1 Scene Assembly ===");

            // 1. Setup URP
            Debug.Log("Step 1/7: Setting up URP...");
            URPSetup.SetupURPAssets();

            // 2. Post-processing
            Debug.Log("Step 2/7: Creating post-processing volume...");
            URPSetup.CreatePostProcessingVolume();

            // 3. Skybox
            Debug.Log("Step 3/7: Creating neon skybox...");
            URPSetup.CreateNeonSkybox();

            // 4. Ship prefabs
            Debug.Log("Step 4/7: Building ship prefabs...");
            ShipBuilder.BuildAllShips();

            // 5. Track 1
            Debug.Log("Step 5/7: Building Track 1 spline...");
            Track1Builder.BuildTrack1();

            // 6. Cameras
            Debug.Log("Step 6/7: Setting up cameras...");
            // Wait for ship to be instantiated first
            EditorApplication.delayCall += () =>
            {
                // Instantiate player ship
                GameObject playerShipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Ship_Velocity.prefab");
                if (playerShipPrefab != null)
                {
                    GameObject playerShip = PrefabUtility.InstantiatePrefab(playerShipPrefab) as GameObject;
                    if (playerShip != null)
                    {
                        playerShip.transform.position = new Vector3(0f, 2f, 0f);
                        playerShip.name = "PlayerShip";
                        
                        // Setup cameras
                        CameraSetup.SetupRaceCameras();
                    }
                }

                // 7. Race Manager
                Debug.Log("Step 7/7: Adding RaceManager...");
                GameObject raceManagerObj = new GameObject("RaceManager");
                RaceManager raceManager = raceManagerObj.AddComponent<RaceManager>();

                // Add Music Manager
                GameObject musicObj = new GameObject("MusicManager");
                musicObj.AddComponent<MusicManager>();

                // Save scene
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();

                Debug.Log("=== ✅ Track 1 Scene Assembly Complete! ===");
                Debug.Log("Next: Instantiate 3-4 AI ships, then press Play to test!");
            };
        }

        [MenuItem("Tools/Neon Rush/Quick Test - Spawn AI Ships")]
        public static void SpawnAIShips()
        {
            // Spawn 3 AI ships
            string[] teams = { "Titan", "Phantom", "Helix" };
            
            for (int i = 0; i < teams.Length; i++)
            {
                GameObject shipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Ship_{teams[i]}.prefab");
                if (shipPrefab != null)
                {
                    GameObject ship = PrefabUtility.InstantiatePrefab(shipPrefab) as GameObject;
                    if (ship != null)
                    {
                        ship.transform.position = new Vector3(-5f + (i * 5f), 2f, -5f);
                        ship.name = $"AIShip_{teams[i]}";
                    }
                }
            }

            Debug.Log("✅ Spawned 3 AI ships");
        }
    }
}
#endif
