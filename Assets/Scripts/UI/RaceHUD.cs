// Module: UI   Owner: Member 2   Purpose: In-race HUD display

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NeonRush
{
    public class RaceHUD : MonoBehaviour
    {
        [Header("Speed & Energy")]
        public TextMeshProUGUI speedText;
        public Image energyBar;
        
        [Header("Position & Lap")]
        public TextMeshProUGUI positionText;
        public TextMeshProUGUI lapText;
        public TextMeshProUGUI lapTimeText;
        
        [Header("Weapon")]
        public Image weaponIcon;
        public GameObject weaponPanel;
        
        [Header("Mini Map")]
        public RawImage miniMapImage;
        public Transform[] racerDots;
        
        [Header("Notifications")]
        public TextMeshProUGUI notificationText;
        public GameObject wrongWayPanel;
        
        private ShipController playerShip;
        private float notificationTimer = 0f;
        private RaceManager raceManager;
        
        void Start()
        {
            if (wrongWayPanel != null) wrongWayPanel.SetActive(false);
            if (weaponPanel != null) weaponPanel.SetActive(false);
            HideNotification();

            // Find race manager
            raceManager = FindAnyObjectByType<RaceManager>();

            // Find player ship
            ShipInput playerInput = FindAnyObjectByType<ShipInput>();
            if (playerInput != null)
            {
                playerShip = playerInput.GetComponent<ShipController>();
            }
        }
        
        void Update()
        {
            UpdateSpeed();
            UpdateEnergy();
            UpdateRaceInfo();
            UpdateNotifications();
        }

        void UpdateRaceInfo()
        {
            if (raceManager == null) return;

            // Update position
            int position = raceManager.GetPlayerPosition();
            if (positionText != null)
            {
                positionText.text = $"{position}";
            }

            // Update lap
            int currentLap = raceManager.GetPlayerLap();
            if (lapText != null)
            {
                lapText.text = $"LAP {currentLap}/{raceManager.TotalLaps}";
            }

            // Update lap time
            if (lapTimeText != null)
            {
                lapTimeText.text = FormatTime(raceManager.RaceTime);
            }

            // Show countdown at race start
            float countdown = raceManager.GetCountdownTime();
            if (countdown > 0f)
            {
                int countdownInt = Mathf.CeilToInt(countdown);
                ShowNotification(countdownInt.ToString(), 0.5f);
            }
        }
        
        void UpdateSpeed()
        {
            if (playerShip == null) return;
            
            if (speedText != null)
            {
                // Convert m/s to km/h
                float kmh = playerShip.currentSpeed * 3.6f;
                speedText.text = $"{kmh:F0}";
            }
        }
        
        void UpdateEnergy()
        {
            if (playerShip == null || energyBar == null) return;
            
            float energyRatio = playerShip.currentEnergy / playerShip.maxEnergy;
            energyBar.fillAmount = energyRatio;
            
            // Color shift based on energy level
            if (energyRatio < 0.3f)
            {
                energyBar.color = Color.Lerp(Color.red, Color.yellow, energyRatio / 0.3f);
            }
            else
            {
                energyBar.color = Color.Lerp(Color.yellow, Color.cyan, (energyRatio - 0.3f) / 0.7f);
            }
        }
        
        void UpdateNotifications()
        {
            if (notificationTimer > 0f)
            {
                notificationTimer -= Time.deltaTime;
                if (notificationTimer <= 0f)
                {
                    HideNotification();
                }
            }
        }
        
        public void SetPlayerShip(ShipController ship)
        {
            playerShip = ship;
        }
        
        public void UpdatePosition(int position, int total)
        {
            if (positionText != null)
            {
                positionText.text = $"{position}/{total}";
            }
        }
        
        public void UpdateLap(int currentLap, int totalLaps, float lapTime, float bestLap)
        {
            if (lapText != null)
            {
                lapText.text = $"LAP {currentLap}/{totalLaps}";
            }
            
            if (lapTimeText != null)
            {
                string bestText = bestLap > 0f ? $" | BEST {FormatTime(bestLap)}" : "";
                lapTimeText.text = $"{FormatTime(lapTime)}{bestText}";
            }
        }
        
        public void ShowNotification(string message, float duration = 2f)
        {
            if (notificationText != null)
            {
                notificationText.text = message;
                notificationText.gameObject.SetActive(true);
                notificationTimer = duration;
            }
        }
        
        public void HideNotification()
        {
            if (notificationText != null)
            {
                notificationText.gameObject.SetActive(false);
            }
        }
        
        public void ShowWrongWay(bool show)
        {
            if (wrongWayPanel != null)
            {
                wrongWayPanel.SetActive(show);
            }
        }
        
        string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            float secs = seconds % 60f;
            return $"{minutes}:{secs:00.00}";
        }
    }
}
