using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonRush
{
    public class DemoShowcaseDirector : MonoBehaviour
    {
        private static DemoShowcaseDirector instance;
        private static bool isDemoEnabled = false;

        public static bool IsDemoEnabled => isDemoEnabled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBootstrap()
        {
            // Check command line arguments or trigger file
            string[] args = Environment.GetCommandLineArgs();
            bool flagDemo = false;
            foreach (string arg in args)
            {
                if (arg.Equals("-demo", StringComparison.OrdinalIgnoreCase))
                {
                    flagDemo = true;
                    break;
                }
            }

            if (System.IO.File.Exists("/tmp/neon_demo_active"))
            {
                flagDemo = true;
            }

            if (flagDemo && instance == null)
            {
                isDemoEnabled = true;
                GameObject go = new GameObject("DemoShowcaseDirector");
                instance = go.AddComponent<DemoShowcaseDirector>();
                DontDestroyOnLoad(go);
            }
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            EnsureDemoFunds();
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(RunDemoWorkflowForScene(SceneManager.GetActiveScene().name));
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void Update()
        {
            // Allow manual toggle with F9
            if (Input.GetKeyDown(KeyCode.F9))
            {
                if (!isDemoEnabled)
                {
                    isDemoEnabled = true;
                    EnsureDemoFunds();
                    StartCoroutine(RunDemoWorkflowForScene(SceneManager.GetActiveScene().name));
                }
            }
        }

        void EnsureDemoFunds()
        {
            SaveData data = SaveSystem.Load();
            if (data.credits < 3000)
            {
                data.credits = 3500;
                SaveSystem.Save(data);
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!isDemoEnabled) return;
            StopAllCoroutines();
            StartCoroutine(RunDemoWorkflowForScene(scene.name));
        }

        IEnumerator RunDemoWorkflowForScene(string sceneName)
        {
            yield return new WaitForSeconds(0.4f);

            if (sceneName.Contains("MainMenu"))
            {
                yield return StartCoroutine(RunMainMenuSequence());
            }
            else
            {
                yield return StartCoroutine(RunGameplaySequence());
            }
        }

        IEnumerator RunMainMenuSequence()
        {
            var menu = FindAnyObjectByType<MainMenuUI>();
            if (menu == null) yield break;

            // 1. Studio Splash
            yield return new WaitForSeconds(1.2f);
            menu.DemoAdvanceToTitle();

            // 2. Title Screen ("NEON RUSH — GRID RUNNERS")
            yield return new WaitForSeconds(1.5f);
            menu.DemoAdvanceToMain();

            // 3. Main Menu
            yield return new WaitForSeconds(1.2f);

            // 4. Garage Tour & Upgrade
            menu.DemoOpenGarage();
            yield return new WaitForSeconds(0.8f);

            var garage = FindAnyObjectByType<GarageUI>();
            if (garage != null)
            {
                // Inspect Volt
                yield return new WaitForSeconds(0.8f);
                // Switch to Brick
                garage.DemoSelectNextVehicle();
                yield return new WaitForSeconds(0.8f);
                // Switch to Blade
                garage.DemoSelectNextVehicle();
                yield return new WaitForSeconds(0.8f);

                // Purchase upgrade on Engine
                garage.DemoPurchaseUpgrade(UpgradeType.Engine);
                yield return new WaitForSeconds(1.0f);

                garage.DemoCloseGarage();
            }

            yield return new WaitForSeconds(0.8f);

            // 5. Circuit Selection & Start Race
            menu = FindAnyObjectByType<MainMenuUI>();
            if (menu != null)
            {
                menu.DemoOpenTrackSelect();
                yield return new WaitForSeconds(1.2f);
                menu.DemoStartRace();
            }
        }

        IEnumerator RunGameplaySequence()
        {
            // Wait for countdown
            while (CountdownController.IsCountingDown)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            // Activate virtual input overrides
            GameInput.OverrideActive = true;
            GameInput.OverrideVertical = 1.0f;
            GameInput.OverrideHorizontal = 0f;

            VehicleController vehicle = FindAnyObjectByType<VehicleController>();
            if (vehicle == null) yield break;

            Rigidbody rb = vehicle.GetComponent<Rigidbody>();

            // Guided waypoint tracking along the track
            Vector3[] waypoints = new Vector3[]
            {
                new Vector3(0f, 0.5f, 5f),      // Shard 1
                new Vector3(0f, 0.5f, 12f),     // Boost Pad 1
                new Vector3(-3f, 0.5f, 15f),    // Shard 2
                new Vector3(-4.5f, 0.5f, 18f),  // Energy Zone (Anti-Grav cell)
                new Vector3(0f, 0.5f, 35f),     // Checkpoint 1
                new Vector3(6.5f, 0.5f, 50f),   // Right bypass lane Boost Pad
                new Vector3(6.5f, 0.5f, 58f),   // Shard
                new Vector3(0f, 0.5f, 65f),     // Checkpoint 2
                new Vector3(9f, 0.5f, 82f),     // Turn 1 Apex Shard
                new Vector3(14f, 0.5f, 90f),    // Chicane Shard
                new Vector3(26f, 0.5f, 115f),   // Checkpoint 3 Boost Pad
                new Vector3(14f, 0.5f, 142f),   // Turn 2 Laser Gate
                new Vector3(0f, 0.5f, 172f),    // Chicane Exit
                new Vector3(0f, 0.5f, 240f),    // Mid-course
                new Vector3(0f, 0.5f, 350f),    // Sector 4
                new Vector3(-4f, 0.5f, 395f),   // Boost pad
                new Vector3(0f, 0.5f, 420f),    // Checkpoint 8
                new Vector3(0f, 0.5f, 472f),    // Final Boost 1
                new Vector3(0f, 0.5f, 488f),    // Final Boost 2
                new Vector3(0f, 0.5f, 504f),    // Final Boost 3
                new Vector3(0f, 0.5f, 520f)     // Victory Finish Arch
            };

            int wpIndex = 0;
            float startTime = Time.time;
            bool damageApplied = false;
            bool hoverTriggered = false;

            while (wpIndex < waypoints.Length)
            {
                if (vehicle == null) break;

                Vector3 target = waypoints[wpIndex];
                Vector3 toTarget = target - vehicle.transform.position;
                toTarget.y = 0;
                float dist = toTarget.magnitude;

                // Steer towards target
                float angle = Vector3.SignedAngle(vehicle.transform.forward, toTarget.normalized, Vector3.up);
                GameInput.OverrideHorizontal = Mathf.Clamp(angle / 30f, -1f, 1f);
                GameInput.OverrideVertical = 1.0f;

                // Drift effect in chicane
                if (wpIndex >= 8 && wpIndex <= 11)
                {
                    GameInput.OverrideDrift = true;
                }
                else
                {
                    GameInput.OverrideDrift = false;
                }

                // Showcase damage at Z ~ 95m
                if (!damageApplied && vehicle.transform.position.z >= 85f)
                {
                    damageApplied = true;
                    vehicle.TakeDamage(25); // Drops to 75 HP, triggers red damage vignette
                }

                // Showcase Anti-Gravity Hover at Z ~ 150m
                if (!hoverTriggered && vehicle.transform.position.z >= 140f)
                {
                    hoverTriggered = true;
                    GameInput.OverrideAntiGravity = true;
                }

                // Assist vehicle speed along track
                if (rb != null)
                {
                    Vector3 fwdVel = vehicle.transform.forward * Mathf.Clamp(rb.linearVelocity.magnitude, 28f, 55f);
                    fwdVel.y = rb.linearVelocity.y;
                    rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, fwdVel, Time.deltaTime * 6f);
                }

                if (dist < 5.0f || (target.z - vehicle.transform.position.z < 2f))
                {
                    wpIndex++;
                }

                var rm = RaceManager.Instance;
                if (rm != null && rm.IsRaceFinished)
                {
                    break;
                }

                yield return null;
            }

            // Finish reached
            GameInput.OverrideVertical = 0f;
            GameInput.OverrideBrake = true;
            GameInput.OverrideDrift = false;

            // Wait for Results Screen to display
            yield return new WaitForSeconds(4.5f);

            // Turn off overrides
            GameInput.OverrideActive = false;

            // Quit application cleanly so video capture completes
            Debug.Log("[DemoShowcaseDirector] Demo completed successfully!");
            Application.Quit();
        }
    }
}
