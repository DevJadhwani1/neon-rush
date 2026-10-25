using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Player Instantiation Configuration")]
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;
    public Vector3 defaultPlayerPosition = new Vector3(0f, 1.2f, -16f);
    public Quaternion defaultPlayerRotation = Quaternion.identity;

    [Header("Enemy Instantiation Configuration")]
    public GameObject enemyPrefab;
    public Vector3[] defaultEnemySpawnPositions = new Vector3[]
    {
        new Vector3(0f, 1.2f, 45f),     // Sector 1 Straightaway Patrol
        new Vector3(12f, 1.2f, 85f),    // Sector 2 Chicane Interceptor
        new Vector3(-6f, 1.2f, 160f),   // Sector 3 Laser S-Curve Guard
        new Vector3(0f, 1.2f, 260f)     // Sector 4 Gauntlet Interceptor
    };

    [Header("Runtime Instantiated Instances")]
    public GameObject activePlayerInstance;
    public GameObject[] activeEnemyInstances;

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

        LoadPrefabsIfMissing();
    }

    void Start()
    {
        // 1. Clean up any static/duplicate scene players to ensure strictly ONE player instance
        GameObject[] existingPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in existingPlayers)
        {
            if (p != null)
            {
                Destroy(p);
            }
        }
        GameObject namedPlayer = GameObject.Find("Player");
        if (namedPlayer != null)
        {
            Destroy(namedPlayer);
        }

        // 2. Dynamic Instantiation of the single Player from Prefab
        activePlayerInstance = InstantiatePlayer();

        // 3. Dynamic Instantiation of Enemies
        SpawnAllEnemies();
    }

    void LoadPrefabsIfMissing()
    {
        // Auto-resolve Player Prefab
        if (playerPrefab == null)
        {
            playerPrefab = Resources.Load<GameObject>("Player");
#if UNITY_EDITOR
            if (playerPrefab == null)
            {
                playerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            }
#endif
        }

        // Auto-resolve Enemy Prefab
        if (enemyPrefab == null)
        {
            enemyPrefab = Resources.Load<GameObject>("Enemy");
#if UNITY_EDITOR
            if (enemyPrefab == null)
            {
                enemyPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
            }
            if (enemyPrefab == null)
            {
                enemyPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PoliceCar.prefab");
            }
#endif
        }
    }

    // =========================================================================
    // PLAYER INSTANTIATION (Assignment Rubric Requirement)
    // =========================================================================
    public GameObject InstantiatePlayer(Vector3? customPosition = null, Quaternion? customRotation = null)
    {
        LoadPrefabsIfMissing();

        Vector3 spawnPos = customPosition ?? (playerSpawnPoint != null ? playerSpawnPoint.position : defaultPlayerPosition);
        Quaternion spawnRot = customRotation ?? (playerSpawnPoint != null ? playerSpawnPoint.rotation : defaultPlayerRotation);

        GameObject playerObj = null;

        if (playerPrefab != null)
        {
            playerObj = Instantiate(playerPrefab, spawnPos, spawnRot);
            playerObj.name = "Player";
            Debug.Log($"[SpawnManager] ✅ SUCCESS: Instantiated Player GameObject from Prefab '{playerPrefab.name}' at {spawnPos}.");
        }
        else
        {
            Debug.LogWarning("[SpawnManager] Player Prefab reference missing! Creating procedural Player GameObject instance.");
            playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.name = "Player";
            playerObj.transform.position = spawnPos;
            playerObj.transform.rotation = spawnRot;
            playerObj.AddComponent<Rigidbody>();
            playerObj.AddComponent<PlayerController>();
            playerObj.AddComponent<PlayerHealth>();
        }

        playerObj.tag = "Player";
        activePlayerInstance = playerObj;

        // Ensure PlayerHealth & PlayerController are properly attached
        if (playerObj.GetComponent<PlayerHealth>() == null)
        {
            playerObj.AddComponent<PlayerHealth>();
        }
        if (playerObj.GetComponent<PlayerController>() == null)
        {
            playerObj.AddComponent<PlayerController>();
        }

        // Connect Main Camera Follow to newly instantiated player
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.gameObject.SetActive(true);
            mainCam.enabled = true;
            mainCam.tag = "MainCamera";

            SimpleCameraFollow follow = mainCam.GetComponent<SimpleCameraFollow>();
            if (follow == null)
            {
                follow = mainCam.gameObject.AddComponent<SimpleCameraFollow>();
            }

            follow.isVehicleCamera = false;
            follow.target = playerObj.transform;
            follow.distance = 3.8f;
            follow.height = 1.8f;
            follow.lookAheadDistance = 3.0f;
            follow.lookUpOffset = 1.0f;
            follow.SnapToTarget();
        }

        // Deactivate VehicleCamera so it doesn't hijack the on-foot player view
        GameObject vCamObj = GameObject.Find("VehicleCamera");
        if (vCamObj != null)
        {
            vCamObj.SetActive(false);
        }

        // Connect GameManager and VehicleInteraction
        if (GameManager.Instance != null)
        {
            GameManager.Instance.player = playerObj;
        }

        VehicleInteraction vi = FindAnyObjectByType<VehicleInteraction>();
        if (vi != null)
        {
            vi.player = playerObj;
            vi.ResolveReferences();
        }

        return playerObj;
    }

    // =========================================================================
    // ENEMY INSTANTIATION (Assignment Rubric Requirement)
    // =========================================================================
    public GameObject InstantiateEnemy(Vector3 position, Quaternion rotation, string customName = "Enemy")
    {
        LoadPrefabsIfMissing();

        GameObject enemyObj = null;

        if (enemyPrefab != null)
        {
            enemyObj = Instantiate(enemyPrefab, position, rotation);
            enemyObj.name = customName;
            Debug.Log($"[SpawnManager] ✅ SUCCESS: Instantiated Enemy GameObject from Prefab '{enemyPrefab.name}' at {position}.");
        }
        else
        {
            Debug.LogWarning("[SpawnManager] Enemy Prefab reference missing! Creating procedural Enemy GameObject instance.");
            enemyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemyObj.name = customName;
            enemyObj.transform.position = position;
            enemyObj.transform.rotation = rotation;
            enemyObj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            enemyObj.AddComponent<Rigidbody>();
            enemyObj.AddComponent<EnemyController>();
        }

        enemyObj.tag = "Enemy";

        if (enemyObj.GetComponent<EnemyController>() == null && enemyObj.GetComponent<PoliceVehicle>() == null)
        {
            enemyObj.AddComponent<EnemyController>();
        }

        return enemyObj;
    }

    public void SpawnAllEnemies()
    {
        if (defaultEnemySpawnPositions == null || defaultEnemySpawnPositions.Length == 0) return;

        activeEnemyInstances = new GameObject[defaultEnemySpawnPositions.Length];

        for (int i = 0; i < defaultEnemySpawnPositions.Length; i++)
        {
            Vector3 pos = defaultEnemySpawnPositions[i];
            Quaternion rot = Quaternion.Euler(0f, 180f, 0f); // Facing toward starting player
            activeEnemyInstances[i] = InstantiateEnemy(pos, rot, $"Enemy_Unit_{i + 1}");
        }

        Debug.Log($"[SpawnManager] Spawned {activeEnemyInstances.Length} Enemy units across the circuit.");
    }

    public void RespawnPlayer()
    {
        if (activePlayerInstance != null)
        {
            Destroy(activePlayerInstance);
        }

        activePlayerInstance = InstantiatePlayer(defaultPlayerPosition, defaultPlayerRotation);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowNotification("⚡ PLAYER RE-INSTANTIATED AT STARTING BAY", 2.2f);
        }
    }
}
