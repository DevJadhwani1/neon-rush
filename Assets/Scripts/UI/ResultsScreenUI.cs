// Module: UI   Owner: Member 3   Purpose: Race results screen with position, time, credits

using UnityEngine;

namespace NeonRush
{
    public class ResultsScreenUI : MonoBehaviour
    {
        private int finalPosition = 1;
        private float raceTime = 0f;
        private int creditsEarned = 0;
        private static bool hasAwardedThisSession = false;
        
        void Start()
        {
            // Results screen enabled
            RaceManager raceManager = FindAnyObjectByType<RaceManager>();
            if (raceManager != null)
            {
                finalPosition = raceManager.GetPlayerPosition();
                raceTime = raceManager.RaceTime;
            }
            
            // Award credits exactly once per race completion session
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
            // Results screen enabled
            
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 64;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.UpperCenter;
            titleStyle.normal.textColor = new Color(0f, 0.9f, 1f);
            
            GUI.Label(new Rect(0, 40, Screen.width, 80), "RACE COMPLETE!", titleStyle);
            
            // Stats panel
            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.4f;
            
            GUIStyle statsStyle = new GUIStyle(GUI.skin.label);
            statsStyle.fontSize = 36;
            statsStyle.alignment = TextAnchor.MiddleCenter;
            statsStyle.normal.textColor = Color.white;
            
            string positionText = GetOrdinal(finalPosition);
            string timeText = FormatTime(raceTime);
            
            string stats = $"POSITION: {positionText}\n\n" +
                           $"TIME: {timeText}\n\n" +
                           $"CREDITS EARNED: +{creditsEarned}";
            
            GUI.Label(new Rect(centerX - 300, centerY - 100, 600, 300), stats, statsStyle);
            
            // Buttons
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 28;
            buttonStyle.fontStyle = FontStyle.Bold;
            
            float buttonY = Screen.height - 120;
            
            if (GUI.Button(new Rect(centerX - 420, buttonY, 260, 60), "RACE AGAIN", buttonStyle))
            {
                SceneLoader.LoadRace();
            }
            
            if (GUI.Button(new Rect(centerX - 130, buttonY, 260, 60), "GARAGE", buttonStyle))
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
            
            if (GUI.Button(new Rect(centerX + 160, buttonY, 260, 60), "MENU", buttonStyle))
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
    }
}
