// Module: UI   Owner: Member 3   Purpose: Race results screen with position, time, credits, and safe transitions

using UnityEngine;

namespace NeonRush
{
    public class ResultsScreenUI : MonoBehaviour
    {
        private int finalPosition = 1;
        private float raceTime = 0f;
        private int creditsEarned = 0;
        private static bool hasAwardedThisSession = false;

        private GUIStyle titleStyle;
        private GUIStyle panelStyle;
        private GUIStyle statsStyle;
        private GUIStyle buttonStyle;
        private Texture2D panelBgTex;
        private Texture2D dimBgTex;

        void EnsureTextures()
        {
            if (dimBgTex == null)
            {
                dimBgTex = new Texture2D(1, 1);
                dimBgTex.SetPixel(0, 0, new Color(0.02f, 0.03f, 0.06f, 0.92f));
                dimBgTex.Apply();
            }

            if (panelBgTex == null)
            {
                panelBgTex = new Texture2D(1, 1);
                panelBgTex.SetPixel(0, 0, new Color(0.06f, 0.08f, 0.14f, 0.9f));
                panelBgTex.Apply();
            }
        }

        void Start()
        {
            EnsureTextures();

            RaceManager raceManager = FindAnyObjectByType<RaceManager>();
            if (raceManager != null)
            {
                finalPosition = raceManager.GetPlayerPosition();
                raceTime = raceManager.RaceTime;
            }

            // Award credits exactly once per race completion session: 1st=500, 2nd=300, 3rd=200, 4th=100
            int[] positionRewards = { 500, 300, 200, 100 };
            creditsEarned = positionRewards[Mathf.Clamp(finalPosition - 1, 0, 3)];

            if (!hasAwardedThisSession)
            {
                SaveData data = SaveSystem.Load();
                data.credits += creditsEarned;
                SaveSystem.Save(data);
                hasAwardedThisSession = true;
                Debug.Log($"[ResultsScreenUI] Position {finalPosition}, Time {raceTime:F2}s, Awarded {creditsEarned} CR");
            }
            else
            {
                Debug.Log($"[ResultsScreenUI] Position {finalPosition}, Time {raceTime:F2}s, Previously Awarded {creditsEarned} CR");
            }
        }

        public static void ResetRewardSession()
        {
            hasAwardedThisSession = false;
        }

        void OnGUI()
        {
            // Do not draw results over active loading screen
            if (SceneLoader.Instance != null && SceneLoader.Instance.IsLoading) return;

            GUI.depth = -100;
            EnsureTextures();

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 58,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperCenter
                };
                titleStyle.normal.textColor = new Color(0f, 0.95f, 1f);

                panelStyle = new GUIStyle(GUI.skin.box);

                statsStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 28,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                statsStyle.normal.textColor = Color.white;

                buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 24,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            // Dim backdrop
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimBgTex);

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.42f;

            GUI.Label(new Rect(0, cy - 180, Screen.width, 70), "RACE COMPLETE", titleStyle);

            // Card Panel
            float cardW = Mathf.Clamp(Screen.width * 0.6f, 440f, 620f);
            float cardH = 220f;
            float cardX = cx - (cardW * 0.5f);
            float cardY = cy - 90f;

            GUI.DrawTexture(new Rect(cardX, cardY, cardW, cardH), panelBgTex);

            string positionText = GetOrdinal(finalPosition);
            string timeText = FormatTime(raceTime);

            string stats = $"FINAL POSITION:  {positionText}\n\n" +
                           $"ELAPSED TIME:  {timeText}\n\n" +
                           $"CREDITS EARNED:  +{creditsEarned} CR";

            GUI.Label(new Rect(cardX + 20, cardY + 25, cardW - 40, cardH - 50), stats, statsStyle);

            // Action Buttons
            float buttonY = cy + 160f;
            float btnW = 200f;
            float btnH = 55f;
            float spacing = 20f;
            float totalBtnW = (btnW * 3) + (spacing * 2);
            float startBtnX = cx - (totalBtnW * 0.5f);

            if (GUI.Button(new Rect(startBtnX, buttonY, btnW, btnH), "RACE AGAIN", buttonStyle))
            {
                SceneLoader.LoadRace();
            }

            if (GUI.Button(new Rect(startBtnX + btnW + spacing, buttonY, btnW, btnH), "GARAGE", buttonStyle))
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

            if (GUI.Button(new Rect(startBtnX + (btnW * 2) + (spacing * 2), buttonY, btnW, btnH), "MAIN MENU", buttonStyle))
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
