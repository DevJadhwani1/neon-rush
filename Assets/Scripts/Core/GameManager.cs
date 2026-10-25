// Module: Core   Owner: Member 3   Purpose: Central game state machine and session data manager

using UnityEngine;
using System;

namespace NeonRush
{
    public enum GameState
    {
        Splash,
        Title,
        MainMenu,
        Loading,
        Intro,
        Countdown,
        Racing,
        Paused,
        GameOver,
        Finish,
        Results
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Current State")]
        public GameState currentState = GameState.MainMenu;
        public GameState CurrentState => currentState;
        
        [Header("Session Data")]
        public string selectedVehicleId = "VOLT"; // default to free vehicle
        public string selectedTrackId = "NEON_DOWNTOWN";
        public string selectedMode = "GRAND_PRIX";
        
        [Header("Race State")]
        public bool isGameActive = false;
        public float raceTime = 0f;
        public int score = 0;
        public bool hasWon = false;
        public int shardsCollected = 0;
        public int totalShards = 0;
        
        // Events for state changes
        public event Action<GameState, GameState> OnStateChanged; // oldState, newState

        void Awake()
        {
            // Singleton with DontDestroyOnLoad
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                // If this GameObject has other components attached, only destroy this duplicate component!
                if (GetComponent<VehicleInteraction>() != null || GetComponent<CameraSetup>() != null || GetComponent<GameUIManager>() != null)
                {
                    Destroy(this);
                }
                else
                {
                    Destroy(gameObject);
                }
                return;
            }
        }

        void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (scene.name.Contains("MainMenu"))
            {
                ResetSession();
                // In main menu
                if (currentState != GameState.Splash && currentState != GameState.Title)
                {
                    ChangeState(GameState.MainMenu);
                }
            }
            else
            {
                // In race scene: ensure all race systems exist and reset state
                ResetSession();
                ChangeState(GameState.Countdown);
                EnsureRaceComponents();
            }
        }

        public static void EnsureRaceComponents()
        {
            SceneLoader.EnsureLoaderExists();

            if (UnityEngine.Object.FindAnyObjectByType<CountdownController>() == null)
            {
                GameObject cdObj = new GameObject("CountdownController");
                cdObj.AddComponent<CountdownController>();
            }

            if (UnityEngine.Object.FindAnyObjectByType<PauseMenuUI>() == null)
            {
                GameObject pmObj = new GameObject("PauseMenuUI");
                pmObj.AddComponent<PauseMenuUI>();
            }

            if (UnityEngine.Object.FindAnyObjectByType<ResultsScreenUI>() == null)
            {
                GameObject rsObj = new GameObject("ResultsScreenUI");
                rsObj.AddComponent<ResultsScreenUI>();
            }

            if (UnityEngine.Object.FindAnyObjectByType<RaceManager>() == null)
            {
                GameObject rmObj = new GameObject("RaceManager");
                rmObj.AddComponent<RaceManager>();
            }
        }

        void Start()
        {
            // Initialize based on current scene
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("MainMenu"))
            {
                if (currentState != GameState.Splash && currentState != GameState.Title)
                {
                    ChangeState(GameState.MainMenu);
                }
            }
            else
            {
                ResetSession();
                EnsureRaceComponents();
            }
        }

        void Update()
        {
            // Update race time if racing
            if (currentState == GameState.Racing && isGameActive)
            {
                raceTime += Time.deltaTime;
            }

            // Quick restart with R key during gameplay or post-race/game-over states
            if ((currentState == GameState.Racing || currentState == GameState.GameOver || currentState == GameState.Finish || currentState == GameState.Results) && GameInput.GetResetDown())
            {
                if (SceneLoader.Instance != null && !SceneLoader.Instance.IsLoading)
                {
                    SceneLoader.LoadRace();
                }
            }
        }

        public void ChangeState(GameState newState)
        {
            if (newState == currentState) return;
            
            GameState oldState = currentState;
            currentState = newState;
            
            // Handle state transitions
            switch (newState)
            {
                case GameState.Racing:
                    isGameActive = true;
                    Time.timeScale = 1f;
                    break;
                    
                case GameState.Paused:
                    Time.timeScale = 0f;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    break;

                case GameState.GameOver:
                    isGameActive = false;
                    Time.timeScale = 1f;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    break;
                    
                case GameState.Finish:
                case GameState.Results:
                    isGameActive = false;
                    Time.timeScale = 1f;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    break;
                    
                default:
                    Time.timeScale = 1f;
                    break;
            }
            
            // Fire event
            OnStateChanged?.Invoke(oldState, newState);
        }

        public void AddScore(int amount)
        {
            score += amount;
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdateScore(score);
            }
        }

        public void RegisterShard()
        {
            totalShards++;
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdateShards(shardsCollected, totalShards);
            }
        }

        public void AddCollectedShard(int points = 50)
        {
            shardsCollected++;
            AddScore(points);

            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdateShards(shardsCollected, totalShards);
                GameUIManager.Instance.ShowNotification($"🔷 SHARD ACQUIRED! ({shardsCollected}/{totalShards}) +{points} PTS", 1.8f);
            }
        }

        public void OnCourseCompleted()
        {
            if (hasWon || currentState == GameState.GameOver || currentState == GameState.Finish || currentState == GameState.Results) return;

            hasWon = true;
            isGameActive = false;
            ChangeState(GameState.Finish);

            if (GameUIManager.Instance != null)
            {
                int stars = 1;
                float shardRatio = (totalShards > 0) ? (float)shardsCollected / totalShards : 0f;
                if (raceTime <= 65f && shardRatio >= 0.5f)
                {
                    stars = 3;
                }
                else if (raceTime <= 85f || shardRatio >= 0.3f)
                {
                    stars = 2;
                }

                GameUIManager.Instance.ShowVictoryScreen(raceTime, score, stars, shardsCollected, totalShards);
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayVictorySound();
            }
        }

        public void OnPlayerDied()
        {
            if (currentState == GameState.GameOver || currentState == GameState.Finish || currentState == GameState.Results) return;

            isGameActive = false;
            ChangeState(GameState.GameOver);
            Debug.Log("[GameManager] Player eliminated -> Game Over!");

            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowGameOverScreen("PLAYER ELIMINATED");
            }
        }

        public void OnVehicleDestroyed()
        {
            if (currentState == GameState.GameOver || currentState == GameState.Finish || currentState == GameState.Results) return;

            isGameActive = false;
            ChangeState(GameState.GameOver);
            Debug.Log("[GameManager] Vehicle destroyed -> Game Over!");

            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowGameOverScreen("VEHICLE DESTROYED");
            }
        }

        public void ResetSession()
        {
            score = 0;
            raceTime = 0f;
            isGameActive = false;
            hasWon = false;
            shardsCollected = 0;
            totalShards = 0;
            Time.timeScale = 1f;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                // Clean up events
                OnStateChanged = null;
            }
        }
    }
}
