// Module: Race   Owner: Member 2   Purpose: High-visibility 3-2-1-GO countdown sequence at race start
// Fully independent of Time.timeScale, drop shadow rendering, and zero-allocation GUI styling

using UnityEngine;
using System.Collections;

namespace NeonRush
{
    public class CountdownController : MonoBehaviour
    {
        private static CountdownController instance;
        public static CountdownController Instance => instance;
        public static bool IsCountingDown => (instance != null && instance.countdownActive);

        [Header("Countdown Settings")]
        public float countdownDuration = 4f; // 3 seconds + 1 for GO
        public bool countdownActive = false;

        private int currentCountdownNumber = 0;
        private string countdownText = "";
        private float numberAnimTimer = 0f;
        private bool hasStartedSequence = false;

        // Cached GUI Styles
        private GUIStyle numStyle;
        private GUIStyle shadowStyle;
        private GUIStyle prepStyle;

        void Awake()
        {
            if (instance == null) instance = this;
            else if (instance != this) Destroy(this);
        }

        void Start()
        {
            if (!hasStartedSequence)
            {
                StartCountdown();
            }
        }

        public void StartCountdown()
        {
            if (countdownActive) return;
            countdownActive = true;
            hasStartedSequence = true;

            // Notify vehicles with PerfectStartSystem
            GameObject[] vehicles = GameObject.FindGameObjectsWithTag("Vehicle");
            foreach (GameObject vehicle in vehicles)
            {
                PerfectStartSystem perfectStart = vehicle.GetComponent<PerfectStartSystem>();
                if (perfectStart != null)
                {
                    perfectStart.StartCountdown();
                }
            }

            // Zero vehicle velocity initially
            LockPlayerVehicle(true);

            StartCoroutine(CountdownSequence());
        }

        private void LockPlayerVehicle(bool locked)
        {
            var vcs = FindObjectsByType<VehicleController>();
            foreach (var vc in vcs)
            {
                if (vc.isPlayerInVehicle || vc.CompareTag("Player") || vc.name.Contains("Player") || vc.name == "Vehicle")
                {
                    Rigidbody rb = vc.GetComponent<Rigidbody>();
                    if (rb != null && locked)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                }
            }
        }

        IEnumerator CountdownSequence()
        {
            try
            {
                // 3
                currentCountdownNumber = 3;
                countdownText = "3";
                numberAnimTimer = 1f;
                TriggerSound(3);
                yield return new WaitForSecondsRealtime(1f);

                // 2
                currentCountdownNumber = 2;
                countdownText = "2";
                numberAnimTimer = 1f;
                TriggerSound(2);
                yield return new WaitForSecondsRealtime(1f);

                // 1
                currentCountdownNumber = 1;
                countdownText = "1";
                numberAnimTimer = 1f;
                TriggerSound(1);
                yield return new WaitForSecondsRealtime(1f);

                // GO!
                currentCountdownNumber = 0;
                countdownText = "GO!";
                numberAnimTimer = 1.2f;
                TriggerSound(0);

                // Start race in RaceManager
                RaceManager raceManager = FindAnyObjectByType<RaceManager>();
                if (raceManager != null)
                {
                    raceManager.StartRace();
                }

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ChangeState(GameState.Racing);
                }

                yield return new WaitForSecondsRealtime(1.2f);
            }
            finally
            {
                LockPlayerVehicle(false);
                countdownActive = false;
                countdownText = "";
            }
        }

        private void TriggerSound(int number)
        {
            if (SoundManager.Instance != null)
            {
                if (number > 0) SoundManager.Instance.PlayCountdownBeep(false);
                else SoundManager.Instance.PlayCountdownBeep(true);
            }
            if (GameUIManager.Instance != null && !string.IsNullOrEmpty(countdownText))
            {
                GameUIManager.Instance.ShowNotification(countdownText, 0.9f);
            }
        }

        void Update()
        {
            if (numberAnimTimer > 0f)
            {
                numberAnimTimer -= Time.unscaledDeltaTime;
            }
        }

        void OnGUI()
        {
            if (!countdownActive || string.IsNullOrEmpty(countdownText)) return;

            GUI.depth = -500;

            if (numStyle == null)
            {
                numStyle = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

                shadowStyle = new GUIStyle(numStyle);
                shadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.9f);

                prepStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                prepStyle.normal.textColor = new Color(1f, 0.15f, 0.85f);
            }

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.32f;

            Color textColor = (countdownText == "GO!") ? new Color(0.1f, 1f, 0.45f) : new Color(0f, 0.95f, 1f);
            int baseSize = (countdownText == "GO!") ? 112 : 96;

            float bounce = Mathf.Clamp01(numberAnimTimer);
            int dynamicSize = Mathf.RoundToInt(baseSize + (bounce * 18f));

            numStyle.fontSize = dynamicSize;
            numStyle.normal.textColor = textColor;
            shadowStyle.fontSize = dynamicSize;

            // Dual-layer drop shadow for high contrast over track
            GUI.Label(new Rect(cx - 200 + 4, cy - 80 + 4, 400, 160), countdownText, shadowStyle);
            GUI.Label(new Rect(cx - 200, cy - 80, 400, 160), countdownText, numStyle);

            if (countdownText != "GO!")
            {
                GUI.Label(new Rect(cx - 250, cy + 50, 500, 30), "PREPARE TO LAUNCH", prepStyle);
            }
        }

        public int GetCurrentNumber()
        {
            return currentCountdownNumber;
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}
