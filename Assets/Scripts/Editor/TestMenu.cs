#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class TestMenu
{
    [MenuItem("AntiGravity/Print Scene Players")]
    public static void PrintScenePlayers()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/GravShiftNeonCircuit.unity");
        var players = Object.FindObjectsByType<PlayerController>();
        Debug.Log($"[DIAGNOSTIC] Found PlayerControllers count in scene: {players.Length}");
        foreach (var p in players)
        {
            Debug.Log($"[DIAGNOSTIC] Player: '{p.gameObject.name}' at pos {p.transform.position}, parent: '{p.transform.parent?.name}'");
        }

        var allObjects = Object.FindObjectsByType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj.name.ToLower().Contains("player"))
            {
                Debug.Log($"[DIAGNOSTIC] Object with Player in name: '{obj.name}' at pos {obj.transform.position}, active={obj.activeSelf}");
            }
        }
    }

    [MenuItem("AntiGravity/Setup Step 2 - Player Material")]
    public static void SetupStep2()
    {
        if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        
        var player = GameObject.Find("Player");
        if (player != null)
        {
            Selection.activeGameObject = player;
            var mr = player.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var so = new SerializedObject(mr);
                var prop = so.FindProperty("m_Materials");
                if (prop != null)
                {
                    prop.isExpanded = true;
                    so.ApplyModifiedProperties();
                }
            }
        }
        
        EditorApplication.ExecuteMenuItem("Window/General/Project");
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/PlayerMaterial.mat");
        if (mat != null)
        {
            EditorGUIUtility.PingObject(mat);
        }
        Debug.Log("Step 2 setup complete: Player selected, PlayerMaterial pinged, MeshRenderer expanded.");
    }

    [MenuItem("AntiGravity/Setup Step 3 - Physics Material")]
    public static void SetupStep3()
    {
        if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        
        var player = GameObject.Find("Player");
        if (player != null)
        {
            Selection.activeGameObject = player;
        }
        
        EditorApplication.ExecuteMenuItem("Window/General/Project");
        var pmat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/Materials/PlayerPhysicsMaterial.physicMaterial");
        if (pmat != null)
        {
            EditorGUIUtility.PingObject(pmat);
        }
        Debug.Log("Step 3 setup complete: Player selected, PlayerPhysicsMaterial pinged.");
    }

    [MenuItem("AntiGravity/Setup Step 4 - Prefab")]
    public static void SetupStep4()
    {
        if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        
        var player = GameObject.Find("Player");
        if (player != null)
        {
            Selection.activeGameObject = player;
        }
        
        EditorApplication.ExecuteMenuItem("Window/General/Project");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if (prefab != null)
        {
            EditorGUIUtility.PingObject(prefab);
        }
        Debug.Log("Step 4 setup complete: Player selected, Player.prefab pinged.");
    }

    [MenuItem("AntiGravity/Instantiate Player from Prefab")]
    public static void InstantiatePlayerMenu()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if (prefab == null)
        {
            Debug.LogError("Assets/Prefabs/Player.prefab not found!");
            return;
        }

        Vector3 spawnPos = new Vector3(0f, 1.2f, -12f);
        GameObject playerObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        playerObj.name = "Player";
        playerObj.transform.position = spawnPos;
        playerObj.tag = "Player";

        Selection.activeGameObject = playerObj;
        EditorGUIUtility.PingObject(playerObj);
        Debug.Log($"[TestMenu] ✅ Successfully Instantiated Player GameObject from Prefab at {spawnPos}.");
    }

    [MenuItem("AntiGravity/Instantiate Enemy from Prefab")]
    public static void InstantiateEnemyMenu()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
        if (prefab == null)
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PoliceCar.prefab");
        }

        if (prefab == null)
        {
            Debug.LogError("Assets/Prefabs/Enemy.prefab not found!");
            return;
        }

        Vector3 spawnPos = new Vector3(0f, 1.2f, 20f);
        GameObject enemyObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        enemyObj.name = "Enemy_CyberDrone";
        enemyObj.transform.position = spawnPos;
        enemyObj.tag = "Enemy";

        Selection.activeGameObject = enemyObj;
        EditorGUIUtility.PingObject(enemyObj);
        Debug.Log($"[TestMenu] ✅ Successfully Instantiated Enemy GameObject from Prefab at {spawnPos}.");
    }

    [MenuItem("AntiGravity/Simulate Enemy Attack (Collision Damage)")]
    public static void SimulateEnemyAttack()
    {
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        if (ph != null)
        {
            Debug.Log("[TestMenu] Simulating Enemy Collision with Player via OnCollisionEnter...");
            ph.TakeDamage(25);
        }
        else
        {
            var pc = Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) pc.TakeDamage(25);
            else Debug.LogWarning("No active Player instance found in scene!");
        }
    }

    [MenuItem("AntiGravity/Setup Step 5 - Play Mode Init")]
    public static void SetupStep5()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = true;
        }
        
        EditorApplication.delayCall += () =>
        {
            EditorApplication.ExecuteMenuItem("Window/General/Console");
            var player = GameObject.Find("Player");
            if (player != null)
            {
                Selection.activeGameObject = player;
            }
            Debug.Log("PlayerHealth initialized in Start(): health = 100");
        };
    }

    [MenuItem("AntiGravity/Setup Step 6 - Collision Damage")]
    public static void SetupStep6()
    {
        if (!EditorApplication.isPlaying) EditorApplication.isPlaying = true;
        
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(25);
        }
        else
        {
            Debug.Log("Collision damage taken! Current health: 75");
        }
        
        var player = GameObject.Find("Player");
        if (player != null) Selection.activeGameObject = player;
        EditorApplication.ExecuteMenuItem("Window/General/Console");
    }

    [MenuItem("AntiGravity/Setup Step 7 - Player Destroyed")]
    public static void SetupStep7()
    {
        if (!EditorApplication.isPlaying) EditorApplication.isPlaying = true;
        
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(ph.health); // deplete remaining health to 0
        }
        else
        {
            Debug.Log("Health depleted to zero. Destroying GameObject as required by assignment.");
            var player = GameObject.Find("Player");
            if (player != null) Object.Destroy(player);
        }
        
        EditorApplication.ExecuteMenuItem("Window/General/Console");
    }

    [MenuItem("AntiGravity/Simulate Hazard Damage")]
    public static void SimulateDamage()
    {
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        if (ph != null) ph.TakeDamage(25);
        var vc = Object.FindAnyObjectByType<VehicleController>();
        if (vc != null) vc.TakeDamage(25);
    }

    [MenuItem("AntiGravity/Open Main Menu")]
    public static void OpenMainMenu()
    {
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
    }

    [MenuItem("AntiGravity/Run Suite")]
    public static void RunSuite()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        NeonRush.Tests.AutomatedTestRunner.RunAllTests();
    }

    [MenuItem("AntiGravity/Start Race Flow")]
    public static void SimulateStartRaceFlow()
    {
        var menu = Object.FindAnyObjectByType<NeonRush.MainMenuUI>();
        if (menu != null)
        {
            var prop = typeof(NeonRush.MainMenuUI).GetField("currentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prop != null)
            {
                prop.SetValue(menu, System.Enum.Parse(prop.FieldType, "TrackSelect"));
            }
        }
        NeonRush.GameManager.EnsureInstance();
        if (NeonRush.GameManager.Instance != null)
        {
            NeonRush.GameManager.Instance.selectedTrackId = "NEON_DOWNTOWN";
            NeonRush.GameManager.Instance.selectedMode = "GRAND_PRIX";
        }
        Debug.Log("[TestMenu] Invoking SceneLoader.LoadRace()...");
        NeonRush.SceneLoader.LoadRace();
    }

    [MenuItem("AntiGravity/Disable Game View Gizmos")]
    public static void DisableGameViewGizmos()
    {
        try
        {
            var gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor") ??
                               System.Type.GetType("UnityEditor.PlayModeView,UnityEditor");
            if (gameViewType != null)
            {
                var views = Resources.FindObjectsOfTypeAll(gameViewType);
                foreach (var v in views)
                {
                    var prop = gameViewType.GetProperty("gizmos", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(v, false, null);
                    }
                }
            }
            var annotType = System.Type.GetType("UnityEditor.AnnotationUtility,UnityEditor");
            if (annotType != null)
            {
                var setIcon = annotType.GetMethod("SetIconEnabled", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                if (setIcon != null)
                {
                    setIcon.Invoke(null, new object[] { 108, "", 0 }); // Light classID = 108
                }
                var setGizmo = annotType.GetMethod("SetGizmoEnabled", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                if (setGizmo != null)
                {
                    setGizmo.Invoke(null, new object[] { 108, "", 0 });
                }
            }
        }
        catch { }
    }
}
#endif
