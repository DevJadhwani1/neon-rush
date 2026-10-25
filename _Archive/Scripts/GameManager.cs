using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGameActive = true;
    public bool hasWon = false;
    public float raceTime = 0f;
    public int score = 0;
    public int shardsCollected = 0;
    public int totalShards = 0;

    [Header("Core References")]
    public GameObject player;
    public GameObject vehicle;
    public VehicleInteraction vehicleInteraction;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        totalShards = 0;
        shardsCollected = 0;
        hasWon = false;
        isGameActive = true;
        raceTime = 0f;
        score = 0;

        // Ensure Grand Prix Circuit is built and active
        GameObject env = GameObject.Find("Environment");
        if (env != null && env.GetComponent<TrackBuilder>() == null)
        {
            env.AddComponent<TrackBuilder>();
        }

        // Ensure SpawnManager is present for dynamic instantiation of Player and Enemy from Prefabs
        if (GetComponent<SpawnManager>() == null && FindAnyObjectByType<SpawnManager>() == null)
        {
            gameObject.AddComponent<SpawnManager>();
        }

        FindReferences();
    }

    void Start()
    {
        hasWon = false;
        isGameActive = true;
        raceTime = 0f;
        score = 0;
        shardsCollected = 0;

        if (player == null && SpawnManager.Instance != null)
        {
            player = SpawnManager.Instance.activePlayerInstance;
        }

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowGameHUD();
            GameUIManager.Instance.UpdateShards(shardsCollected, totalShards);
        }
    }

    void FindReferences()
    {
        if (player == null) player = GameObject.Find("Player");
        if (vehicle == null) vehicle = GameObject.Find("Vehicle");
        if (vehicleInteraction == null) vehicleInteraction = GetComponent<VehicleInteraction>();
    }

    void Update()
    {
        if (isGameActive && !hasWon)
        {
            raceTime += Time.deltaTime;
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdateTimer(raceTime);
            }
        }

        // Test key: Damage Hazard simulation [K]
        if (GameInput.GetDamageTestDown())
        {
            if (vehicleInteraction != null && vehicleInteraction.isInVehicle && vehicle != null)
            {
                VehicleController vc = vehicle.GetComponent<VehicleController>();
                if (vc != null) vc.TakeDamage(25);
            }
            else if (player != null)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(25);
            }
        }

        // Test key: Finish Circuit simulation [F]
        if (GameInput.GetFinishTestDown())
        {
            OnCourseCompleted();
        }

        // Restart on [R]
        if (GameInput.GetResetDown())
        {
            RestartGame();
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
        if (hasWon) return;

        hasWon = true;
        isGameActive = false;

        // Calculate Star Rating
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

        Debug.Log($"GRAND PRIX COMPLETED! Time: {raceTime:F2}s | Score: {score} | Shards: {shardsCollected}/{totalShards} | Stars: {stars}");

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowVictoryScreen(raceTime, score, stars, shardsCollected, totalShards);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayVictorySound();
        }
    }

    public void OnPlayerDied()
    {
        isGameActive = false;
        Debug.Log("Player eliminated! Press [R] to Restart.");

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowGameOverScreen("PLAYER ELIMINATED");
        }
    }

    public void OnVehicleDestroyed()
    {
        isGameActive = false;
        Debug.Log("Vehicle destroyed! Press [R] to Restart.");

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowGameOverScreen("VEHICLE DESTROYED");
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateScore(score);
        }
    }

    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
