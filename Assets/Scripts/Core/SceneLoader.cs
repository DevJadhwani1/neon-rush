// Module: Core   Owner: Member 3   Purpose: Professional async scene loader with real progress and cyberpunk UI overlay

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace NeonRush
{
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader instance;
        public static SceneLoader Instance => instance;

        [Header("State")]
        private bool isLoading = false;
        public bool IsLoading => isLoading;

        // UI & Animation State
        private float displayProgress = 0f;
        private string currentStatusMessage = "INITIALIZING CIRCUIT...";
        private string currentTip = "";
        private float pulseTimer = 0f;

        private Texture2D overlayBgTex;
        private Texture2D barBgTex;
        private Texture2D barFillTex;
        private Texture2D borderTex;

        public static readonly string[] GAMEPLAY_TIPS = new string[]
        {
            "PRO TIP: Hit [SPACE] to activate Anti-Gravity Hover and glide over circuit hazards.",
            "PRO TIP: Tap and hold [LEFT SHIFT] while steering into sharp bends to charge your Drift Nitro.",
            "PRO TIP: Blue Nitro gives an immediate speed burst; hold the drift longer for Orange and Pink tiers!",
            "PRO TIP: Collect Quantum Shards scattered on the track to maximize your final credits reward.",
            "PRO TIP: Visit the Garage from the Main Menu to upgrade Engine, Handling, Boost, and Armor.",
            "PRO TIP: Draft directly behind opponent racers to activate Slipstream acceleration."
        };

        public static float NormalizeProgress(float rawOpProgress)
        {
            return Mathf.Clamp01(rawOpProgress / 0.9f);
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitTextures();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        void InitTextures()
        {
            overlayBgTex = new Texture2D(1, 1);
            overlayBgTex.SetPixel(0, 0, new Color(0.02f, 0.03f, 0.06f, 0.98f));
            overlayBgTex.Apply();

            barBgTex = new Texture2D(1, 1);
            barBgTex.SetPixel(0, 0, new Color(0.08f, 0.1f, 0.16f, 0.9f));
            barBgTex.Apply();

            barFillTex = new Texture2D(1, 1);
            barFillTex.SetPixel(0, 0, new Color(0f, 0.95f, 1f, 1f)); // Neon cyan
            barFillTex.Apply();

            borderTex = new Texture2D(1, 1);
            borderTex.SetPixel(0, 0, new Color(1f, 0.15f, 0.85f, 0.8f)); // Neon magenta border
            borderTex.Apply();
        }

        public static void EnsureLoaderExists()
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SceneLoader");
                go.AddComponent<SceneLoader>();
            }
        }

        public static void LoadMainMenu()
        {
            EnsureLoaderExists();
            if (instance != null && !instance.isLoading)
            {
                instance.StartCoroutine(instance.LoadSceneAsyncRoutine("MainMenu", false));
            }
        }

        public static void LoadRace()
        {
            EnsureLoaderExists();
            if (instance != null && !instance.isLoading)
            {
                instance.StartCoroutine(instance.LoadSceneAsyncRoutine("GravShiftNeonCircuit", true));
            }
        }

        IEnumerator LoadSceneAsyncRoutine(string sceneName, bool isRaceScene)
        {
            isLoading = true;
            displayProgress = 0f;
            currentTip = GAMEPLAY_TIPS[Random.Range(0, GAMEPLAY_TIPS.Length)];
            currentStatusMessage = isRaceScene ? "ESTABLISHING QUANTUM CIRCUIT..." : "INITIALIZING MENU MATRIX...";

            // Ensure timeScale is normal
            Time.timeScale = 1f;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Loading);
            }

            Debug.Log($"[SceneLoader] Asynchronous loading initiated: {sceneName}");

            // Small initial buffer to render loading overlay
            yield return new WaitForSecondsRealtime(0.15f);

            AsyncOperation op = null;
            try
            {
                if (isRaceScene)
                {
                    op = SceneManager.LoadSceneAsync(1);
                }
                else
                {
                    op = SceneManager.LoadSceneAsync(sceneName);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SceneLoader] LoadSceneAsync error: {ex.Message}");
            }

            if (op == null)
            {
                Debug.LogWarning($"[SceneLoader] Failed async load for '{sceneName}'. Attempting direct fallback.");
                try
                {
                    SceneManager.LoadScene(isRaceScene ? 1 : 0);
                }
                catch (System.Exception ex2)
                {
                    Debug.LogError($"[SceneLoader] Fallback load also failed: {ex2.Message}");
                }
                isLoading = false;
                yield break;
            }

            // Keep scene from activating automatically until smooth progress completes
            op.allowSceneActivation = false;

            // Normalize Unity's 0.0 -> 0.9 raw progress to 0.0 -> 1.0 real progress
            while (op.progress < 0.9f || displayProgress < 0.95f)
            {
                float targetNormalizedProgress = NormalizeProgress(op.progress);

                // Smoothly animate displayProgress towards the real async progress
                displayProgress = Mathf.MoveTowards(displayProgress, targetNormalizedProgress, Time.unscaledDeltaTime * 1.5f);

                if (displayProgress < 0.35f)
                {
                    currentStatusMessage = "SYNCHRONIZING TELEMETRY STREAM...";
                }
                else if (displayProgress < 0.75f)
                {
                    currentStatusMessage = "COMPILING SHADERS & STREAMING GEOMETRY...";
                }
                else
                {
                    currentStatusMessage = "CIRCUIT READY — ACTIVATING INTERFACE...";
                }

                yield return null;
            }

            displayProgress = 1f;
            currentStatusMessage = "SYSTEMS ONLINE";
            yield return new WaitForSecondsRealtime(0.2f);

            // Activate loaded scene
            op.allowSceneActivation = true;

            // Wait for activation completion
            while (!op.isDone)
            {
                yield return null;
            }

            // Post-activation fade transition
            yield return new WaitForSecondsRealtime(0.1f);
            isLoading = false;
            Debug.Log($"[SceneLoader] Scene '{sceneName}' successfully loaded and activated.");
        }

        void Update()
        {
            if (isLoading)
            {
                pulseTimer += Time.unscaledDeltaTime * 4f;
            }
        }

        void OnGUI()
        {
            if (!isLoading) return;

            // Set GUI depth so loading screen renders atop all scene GUIs
            GUI.depth = -1000;

            if (overlayBgTex == null) InitTextures();

            // Background dark backdrop
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayBgTex);

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.45f;

            // Header branding
            GUIStyle brandStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.045f), 36, 64),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            brandStyle.normal.textColor = new Color(0f, 0.95f, 1f);

            GUIStyle subBrandStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.018f), 16, 26),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            subBrandStyle.normal.textColor = new Color(1f, 0.15f, 0.85f);

            GUI.Label(new Rect(0, cy - 120, Screen.width, 70), "NEON RUSH", brandStyle);
            GUI.Label(new Rect(0, cy - 50, Screen.width, 35), "GRID RUNNERS", subBrandStyle);

            // Progress Bar dimensions
            float barW = Mathf.Clamp(Screen.width * 0.65f, 320f, 750f);
            float barH = 24f;
            float barX = cx - (barW * 0.5f);
            float barY = cy + 30f;

            // Outer border
            GUI.DrawTexture(new Rect(barX - 2, barY - 2, barW + 4, barH + 4), borderTex);
            // Bar background
            GUI.DrawTexture(new Rect(barX, barY, barW, barH), barBgTex);
            // Bar fill (reflecting real normalized progress)
            float filledW = barW * Mathf.Clamp01(displayProgress);
            GUI.DrawTexture(new Rect(barX, barY, filledW, barH), barFillTex);

            // Percentage label
            int percent = Mathf.Clamp(Mathf.RoundToInt(displayProgress * 100f), 0, 100);
            GUIStyle percentStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };
            percentStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(barX, barY - 30, barW, 25), $"{percent}%", percentStyle);

            // Status message
            GUIStyle statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            statusStyle.normal.textColor = new Color(0f, 0.95f, 1f);
            GUI.Label(new Rect(barX, barY - 30, barW, 25), currentStatusMessage, statusStyle);

            // Gameplay Tip panel at bottom
            if (!string.IsNullOrEmpty(currentTip))
            {
                GUIStyle tipStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.014f), 13, 18),
                    alignment = TextAnchor.MiddleCenter
                };
                tipStyle.normal.textColor = new Color(0.85f, 0.9f, 0.95f, 0.85f);
                GUI.Label(new Rect(cx - (barW * 0.5f), Screen.height - 90, barW, 40), currentTip, tipStyle);
            }
        }
    }
}
