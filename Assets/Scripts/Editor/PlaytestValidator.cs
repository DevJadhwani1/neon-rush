// Module: Playtest Validator   Owner: Member 1   Purpose: Verify scene is ready for playtest

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace NeonRush
{
    /// <summary>
    /// Validates scene setup before playtesting
    /// </summary>
    public class PlaytestValidator : EditorWindow
    {
        [MenuItem("Tools/Neon Rush/Validate Scene Setup")]
        public static void ValidateScene()
        {
            Debug.Log("=== Validating Scene Setup ===");

            bool allGood = true;
            int checks = 0;
            int passed = 0;

            // Check 1: URP Asset
            checks++;
            if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null)
            {
                Debug.Log("✅ URP render pipeline assigned");
                passed++;
            }
            else
            {
                Debug.LogWarning("⚠️  URP render pipeline not assigned. Run Tools > Neon Rush > Setup URP Rendering");
                allGood = false;
            }

            // Check 2: Player ship
            checks++;
            ShipInput playerInput = FindAnyObjectByType<ShipInput>();
            if (playerInput != null)
            {
                Debug.Log("✅ Player ship found");
                passed++;
            }
            else
            {
                Debug.LogWarning("⚠️  No player ship found. Instantiate Ship_Velocity.prefab");
                allGood = false;
            }

            // Check 3: Race Manager
            checks++;
            RaceManager raceManager = FindAnyObjectByType<RaceManager>();
            if (raceManager != null)
            {
                Debug.Log("✅ RaceManager found");
                passed++;
            }
            else
            {
                Debug.LogWarning("⚠️  No RaceManager found. Add RaceManager component to scene");
                allGood = false;
            }

            // Check 4: Checkpoints
            checks++;
            Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>();
            if (checkpoints.Length > 0)
            {
                Debug.Log($"✅ {checkpoints.Length} checkpoints found");
                passed++;
            }
            else
            {
                Debug.LogWarning("⚠️  No checkpoints found. Use TrackBuilder to generate checkpoints");
                allGood = false;
            }

            // Check 5: Camera
            checks++;
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                Debug.Log("✅ Main camera found");
                passed++;
            }
            else
            {
                Debug.LogWarning("⚠️  No main camera found");
                allGood = false;
            }

            // Check 6: Cinemachine Brain
            checks++;
            if (mainCam != null && mainCam.GetComponent<Unity.Cinemachine.CinemachineBrain>() != null)
            {
                Debug.Log("✅ Cinemachine Brain found");
                passed++;
            }
            else
            {
                Debug.LogWarning("⚠️  No Cinemachine Brain on camera. Run Tools > Neon Rush > Setup Race Cameras");
                allGood = false;
            }

            // Check 7: Virtual Cameras
            checks++;
            Unity.Cinemachine.CinemachineCamera[] vcams = FindObjectsByType<Unity.Cinemachine.CinemachineCamera>();
            if (vcams.Length > 0)
            {
                Debug.Log($"✅ {vcams.Length} virtual cameras found");
                passed++;
            }
            else
            {
                Debug.LogWarning("⚠️  No virtual cameras found. Run Tools > Neon Rush > Setup Race Cameras");
                allGood = false;
            }

            // Check 8: Race HUD
            checks++;
            RaceHUD hud = FindAnyObjectByType<RaceHUD>();
            if (hud != null)
            {
                Debug.Log("✅ RaceHUD found");
                passed++;
            }
            else
            {
                Debug.LogWarning("⚠️  No RaceHUD found. Add Canvas with RaceHUD component");
                allGood = false;
            }

            // Summary
            Debug.Log($"\n=== Validation Complete: {passed}/{checks} checks passed ===");
            
            if (allGood)
            {
                Debug.Log("✅✅✅ Scene is ready for playtest! Press Play to race! ✅✅✅");
            }
            else
            {
                Debug.LogWarning("⚠️  Scene needs more setup before playtest. See warnings above.");
            }
        }
    }
}
#endif
