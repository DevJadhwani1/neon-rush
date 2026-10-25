// Module: UI   Owner: Member 3   Purpose: Race results screen with position, time, credits, laps, score, collectibles, best time, and safe transitions

using UnityEngine;

namespace NeonRush
{
    public class ResultsScreenUI : MonoBehaviour
    {
        private int finalPosition = 1;
        private float raceTime = 0f;
        private int creditsEarned = 0;
        private int finalScore = 0;
        private int shardsCollected = 0;
        private int totalShards = 0;
        private float bestTime = float.MaxValue;
        private bool isNewRecord = false;
        private int totalBalance = 0;
        private static bool hasAwardedThisSession = false;

        private GUIStyle titleStyle;
        private GUIStyle panelStyle;
        private GUIStyle statsStyle;
        private GUIStyle buttonStyle;
        private Texture2D panelBgTex;
        private Texture2D dimBgTex;

        private bool isShowing = false;
        public bool IsShowing => isShowing;

        void EnsureTextures()
        {
            if (dimBgTex == null)
            {
                dimBgTex = new Texture2D(1, 1);
                dimBgTex.SetPixel(0, 0, new Color(0.02f, 0.03f, 0.06f, 0.94f));
                dimBgTex.Apply();
            }

            if (panelBgTex == null)
            {
                panelBgTex = new Texture2D(1, 1);
                panelBgTex.SetPixel(0, 0, new Color(0.05f, 0.07f, 0.13f, 0.95f));
                panelBgTex.Apply();
            }
        }

        void Start()
        {
            EnsureTextures();

            RaceManager raceManager = FindAnyObjectByType<RaceManager>();
            if (raceManager != null && raceManager.IsRaceFinished)
            {
                ShowResults(raceManager.GetPlayerPosition(), raceManager.RaceTime);
            }
        }

        void Update()
        {
            if (!isShowing) return;

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneLoader.LoadRace();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneLoader.LoadMainMenu();
            }
        }

        public void ShowResults(int position = -1, float time = -1f)
        {
            EnsureTextures();

            // Do not grant rewards or show results if Game Over and not an explicit override
            if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.GameOver && position < 0)
            {
                return;
            }

            RaceManager raceManager = FindAnyObjectByType<RaceManager>();
            if (position > 0) finalPosition = position;
            else if (raceManager != null) finalPosition = raceManager.GetPlayerPosition();

            if (time >= 0f) raceTime = time;
            else if (raceManager != null) raceTime = raceManager.RaceTime;
            else if (GameManager.Instance != null) raceTime = GameManager.Instance.raceTime;



            if (GameManager.Instance != null)
            {
                finalScore = GameManager.Instance.score;
                shardsCollected = GameManager.Instance.shardsCollected;
                totalShards = GameManager.Instance.totalShards;
            }

            // Award credits exactly once per race completion session: 1st=500, 2nd=300, 3rd=200, 4th=100
            int[] positionRewards = { 500, 300, 200, 100 };
            int baseCredits = positionRewards[Mathf.Clamp(finalPosition - 1, 0, 3)];
            int shardBonusCredits = shardsCollected * 10;
            creditsEarned = baseCredits + shardBonusCredits;

            string trackKey = "NEON_DOWNTOWN_GRAND_PRIX";
            if (GameManager.Instance != null)
            {
                trackKey = $"{GameManager.Instance.selectedTrackId}_{GameManager.Instance.selectedMode}";
            }

            bestTime = SaveSystem.GetBestTime(trackKey);

            if (!hasAwardedThisSession)
            {
                hasAwardedThisSession = true;
                SaveData data = SaveSystem.Load();
                data.credits += creditsEarned;

                if (raceTime > 0f && raceTime < bestTime)
                {
                    isNewRecord = true;
                    bestTime = raceTime;
                    SaveSystem.SetBestTime(trackKey, raceTime);
                }

                int stars = 1;
                float shardRatio = (totalShards > 0) ? (float)shardsCollected / totalShards : 0f;
                if (raceTime <= 65f && shardRatio >= 0.5f) stars = 3;
                else if (raceTime <= 85f || shardRatio >= 0.3f) stars = 2;
                SaveSystem.SetTrackStars(trackKey, Mathf.Max(stars, SaveSystem.GetTrackStars(trackKey)));

                SaveSystem.Save(data);
                totalBalance = data.credits;
                Debug.Log($"[ResultsScreenUI] Position {finalPosition}, Time {raceTime:F2}s, Awarded {creditsEarned} CR (Balance: {totalBalance} CR), Best: {bestTime:F2}s");
            }
            else
            {
                SaveData data = SaveSystem.Load();
                totalBalance = data.credits;
                Debug.Log($"[ResultsScreenUI] Position {finalPosition}, Time {raceTime:F2}s, Previously Awarded {creditsEarned} CR");
            }

            isShowing = true;
            enabled = true;
        }

        public void Hide()
        {
            isShowing = false;
        }

        public static void ResetRewardSession()
        {
            hasAwardedThisSession = false;
        }

        public static bool HasNextLevel()
        {
            int currentSceneIdx = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            int totalScenes = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < totalScenes; i++)
            {
                string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                if (i != currentSceneIdx && !path.Contains("MainMenu") && !path.Contains("SampleScene"))
                {
                    return true;
                }
            }
            return false;
        }

        void OnGUI()
        {
            if (!isShowing) return;

            // Do not draw results over active loading screen
            if (SceneLoader.Instance != null && SceneLoader.Instance.IsLoading) return;

            GUI.depth = -100;
            EnsureTextures();

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 54,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperCenter
                };
                titleStyle.normal.textColor = new Color(0f, 0.95f, 1f);

                panelStyle = new GUIStyle(GUI.skin.box);

                statsStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    richText = true
                };
                statsStyle.normal.textColor = Color.white;

                buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            // Dim backdrop
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimBgTex);

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.42f;

            string title = (finalPosition == 1) ? "🏁 VICTORY — GRAND PRIX WINNER!" : "RACE COMPLETE";
            GUI.Label(new Rect(0, cy - 200, Screen.width, 65), title, titleStyle);

            // Card Panel
            float cardW = Mathf.Clamp(Screen.width * 0.65f, 480f, 660f);
            float cardH = 260f;
            float cardX = cx - (cardW * 0.5f);
            float cardY = cy - 130f;

            GUI.DrawTexture(new Rect(cardX, cardY, cardW, cardH), panelBgTex);

            string positionText = GetOrdinal(finalPosition);
            string timeText = FormatTime(raceTime);
            string recordBadge = isNewRecord ? " <color=#39FF14>[NEW RECORD!]</color>" : ((bestTime < float.MaxValue && bestTime > 0f) ? $" (BEST: {FormatTime(bestTime)})" : "");
            string shardsText = (totalShards > 0) ? $"🔷 {shardsCollected} / {totalShards}" : $"{shardsCollected} SHARDS";

            string stats = $"<b>STATUS:</b>  <color=#00FFFF>COURSE COMPLETED — {positionText} PLACE</color>\n" +
                           $"<b>ELAPSED TIME:</b>  <color=#FFFFFF>{timeText}</color>{recordBadge}\n" +
                           $"<b>FINAL SCORE:</b>  <color=#00FFAA>{finalScore:N0} PTS</color>\n" +
                           $"<b>COLLECTIBLES:</b>  <color=#00E5FF>{shardsText}</color>\n" +
                           $"<b>REWARDS EARNED:</b>  <color=#FFD700>+{creditsEarned} CR</color>\n" +
                           $"<b>CREDITS BALANCE:</b>  <color=#FFFFFF>{totalBalance} CR</color>";

            GUI.Label(new Rect(cardX + 28, cardY + 20, cardW - 56, cardH - 40), stats, statsStyle);

            // Action Buttons
            bool hasNext = HasNextLevel();
            int buttonCount = hasNext ? 4 : 3;
            float buttonY = cy + 155f;
            float btnW = 180f;
            float btnH = 50f;
            float spacing = 16f;
            float totalBtnW = (btnW * buttonCount) + (spacing * (buttonCount - 1));
            float startBtnX = cx - (totalBtnW * 0.5f);

            int btnIdx = 0;
            if (GUI.Button(new Rect(startBtnX + (btnIdx * (btnW + spacing)), buttonY, btnW, btnH), "RESTART [R]", buttonStyle))
            {
                SceneLoader.LoadRace();
            }
            btnIdx++;

            if (hasNext)
            {
                if (GUI.Button(new Rect(startBtnX + (btnIdx * (btnW + spacing)), buttonY, btnW, btnH), "NEXT LEVEL", buttonStyle))
                {
                    int nextIdx = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(nextIdx);
                }
                btnIdx++;
            }

            if (GUI.Button(new Rect(startBtnX + (btnIdx * (btnW + spacing)), buttonY, btnW, btnH), "GARAGE", buttonStyle))
            {
                var garageUI = FindAnyObjectByType<GarageUI>();
                if (garageUI != null)
                {
                    garageUI.enabled = true;
                    enabled = false;
                }
                else
                {
                    gameObject.AddComponent<GarageUI>();
                    enabled = false;
                }
            }
            btnIdx++;

            if (GUI.Button(new Rect(startBtnX + (btnIdx * (btnW + spacing)), buttonY, btnW, btnH), "MAIN MENU [ESC]", buttonStyle))
            {
                SceneLoader.LoadMainMenu();
            }
        }

        string GetOrdinal(int number)
        {
            if (number <= 0) return number.ToString();

            switch (number % 100)
            {
                case 11:
                case 12:
                case 13:
                    return number + "TH";
            }

            switch (number % 10)
            {
                case 1: return number + "ST";
                case 2: return number + "ND";
                case 3: return number + "RD";
                default: return number + "TH";
            }
        }

        string FormatTime(float timeSeconds)
        {
            int minutes = Mathf.FloorToInt(timeSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeSeconds % 60f);
            int milliseconds = Mathf.FloorToInt((timeSeconds * 100f) % 100f);
            return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }

        void OnDestroy()
        {
            if (dimBgTex != null) { Destroy(dimBgTex); dimBgTex = null; }
            if (panelBgTex != null) { Destroy(panelBgTex); panelBgTex = null; }
        }
    }
}

