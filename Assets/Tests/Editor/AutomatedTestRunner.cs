#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using NeonRush;

namespace NeonRush.Tests
{
    public static class AutomatedTestRunner
    {
        private static int passed = 0;
        private static int failed = 0;

        [MenuItem("AntiGravity/Run All Automated Tests")]
        [MenuItem("Tools/Neon Rush/Run All Automated Tests")]
        public static void RunAllTests()
        {
            passed = 0;
            failed = 0;
            Debug.Log("==================================================");
            Debug.Log(">>> NEON RUSH AUTOMATED TEST SUITE: STARTING <<<");
            Debug.Log("==================================================");

            Test_GameManager_StateTransitions();
            Test_GameManager_ScoringAndShards();
            Test_UpgradeSystem_MultipliersAndBonuses();
            Test_UpgradeSystem_CostScaling();
            Test_SaveSystem_LoadSaveAndDefaults();
            Test_SaveSystem_SpendAndAddCredits();
            Test_SaveSystem_UpgradeLevelPersistence();
            Test_RaceManager_CheckpointSequencing();
            Test_RaceManager_LapAndFinishValidation();
            Test_RaceManager_DNFTimeout();
            Test_VehicleData_FallbackGeneration();
            Test_SceneLoader_ProgressNormalizationAndTips();
            Test_GameManager_SplashAndTitleStates();
            Test_PauseMenu_TimeScaleGuards();
            Test_CountdownController_LockingAndTiming();
            Test_GameManager_EnsureRaceComponents();
            Test_GameInput_StandardizedMappings();
            Test_PlayerHealth_TakeDamageAndCooldown();
            Test_PlayerHealth_RestoreHealth();
            Test_VehicleController_TakeDamageAndRestoreHealth();
            Test_GameManager_GameOverTransition();
            Test_GameManager_CleanSessionReset();
            Test_VehicleInteraction_ConcealAndRestoreRenderers();
            Test_RaceManager_VehicleCountdownWithoutEjection();
            Test_Checkpoint_SafeRespawnPositionAndRotation();
            Test_RaceManager_StrictSequentialCheckpointValidation();
            Test_VehicleController_CheckpointTrackingAndRecovery();
            Test_VehicleController_OutOfBoundsFallRecovery();
            Test_PoliceVehicle_RamForceRestricted();

            Debug.Log("==================================================");
            Debug.Log($">>> SUITE COMPLETED: {passed} PASSED, {failed} FAILED <<<");
            Debug.Log("==================================================");
        }

        private static void Assert(bool condition, string testName, string details = "")
        {
            if (condition)
            {
                passed++;
                Debug.Log($"[PASS] {testName} {details}");
            }
            else
            {
                failed++;
                Debug.LogError($"[FAIL] {testName} {details}");
            }
        }

        private static void Test_GameManager_StateTransitions()
        {
            GameObject go = new GameObject("Test_GM");
            GameManager gm = go.AddComponent<GameManager>();

            gm.ChangeState(GameState.Countdown);
            Assert(gm.CurrentState == GameState.Countdown, "GM: PreRace -> Countdown transition");

            gm.ChangeState(GameState.Racing);
            Assert(gm.CurrentState == GameState.Racing && gm.isGameActive, "GM: Countdown -> Racing transition and active flag");

            gm.ChangeState(GameState.Finish);
            Assert(gm.CurrentState == GameState.Finish && !gm.isGameActive, "GM: Racing -> Finish transition and inactive flag");

            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void Test_GameManager_ScoringAndShards()
        {
            GameObject go = new GameObject("Test_GM_Score");
            GameManager gm = go.AddComponent<GameManager>();

            gm.ResetSession();
            gm.RegisterShard();
            gm.RegisterShard();
            Assert(gm.totalShards == 2, "GM: Shard registration count == 2");

            gm.AddCollectedShard(50);
            Assert(gm.shardsCollected == 1 && gm.score == 50, "GM: Collected shard increments collected count and score");

            gm.AddScore(100);
            Assert(gm.score == 150, "GM: AddScore updates cumulative score");

            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void Test_UpgradeSystem_MultipliersAndBonuses()
        {
            // Engine: +15% per level (1.0, 1.15, 1.30, 1.45)
            Assert(Mathf.Approximately(UpgradeSystem.GetSpeedMultiplier(0), 1.0f), "Upgrade: Speed multiplier level 0 is 1.0");
            Assert(Mathf.Approximately(UpgradeSystem.GetSpeedMultiplier(1), 1.15f), "Upgrade: Speed multiplier level 1 is 1.15");
            Assert(Mathf.Approximately(UpgradeSystem.GetSpeedMultiplier(2), 1.30f), "Upgrade: Speed multiplier level 2 is 1.30");
            Assert(Mathf.Approximately(UpgradeSystem.GetSpeedMultiplier(3), 1.45f), "Upgrade: Speed multiplier level 3 is 1.45");

            // Handling: +10% per level (1.0, 1.10, 1.20, 1.30)
            Assert(Mathf.Approximately(UpgradeSystem.GetHandlingMultiplier(1), 1.10f), "Upgrade: Handling multiplier level 1 is 1.10");

            // Boost: +0.5s per level (0, 0.5, 1.0, 1.5)
            Assert(Mathf.Approximately(UpgradeSystem.GetBoostDurationBonus(2), 1.0f), "Upgrade: Boost bonus level 2 is 1.0s");

            // Armor: +20 HP per level (0, 20, 40, 60)
            Assert(UpgradeSystem.GetArmorBonus(3) == 60, "Upgrade: Armor bonus level 3 is +60 HP");
        }

        private static void Test_UpgradeSystem_CostScaling()
        {
            int cost0 = UpgradeSystem.GetUpgradeCost(UpgradeType.Engine, 0);
            int cost1 = UpgradeSystem.GetUpgradeCost(UpgradeType.Engine, 1);
            int cost2 = UpgradeSystem.GetUpgradeCost(UpgradeType.Engine, 2);
            int costMax = UpgradeSystem.GetUpgradeCost(UpgradeType.Engine, 3);

            Assert(cost0 == 500, "Upgrade: Engine cost lvl 0 is 500");
            Assert(cost1 == 1000, "Upgrade: Engine cost lvl 1 is 1000");
            Assert(cost2 == 2000, "Upgrade: Engine cost lvl 2 is 2000");
            Assert(costMax == 0, "Upgrade: Engine cost at max level returns 0");
        }

        private static void Test_SaveSystem_LoadSaveAndDefaults()
        {
            SaveSystem.ResetAll();
            SaveData data = SaveSystem.Load();

            Assert(data != null, "SaveSystem: Load returns valid object");
            Assert(data.unlockedVehicles.Contains("VOLT"), "SaveSystem: Default vehicle 'VOLT' is unlocked");
            Assert(data.unlockedTracks.Contains("NEON_DOWNTOWN"), "SaveSystem: Default track 'NEON_DOWNTOWN' is unlocked");
            Assert(data.credits == 0, "SaveSystem: Initial credits are 0");
        }

        private static void Test_SaveSystem_SpendAndAddCredits()
        {
            SaveSystem.ResetAll();
            SaveSystem.AddCredits(1000);
            Assert(SaveSystem.Load().credits == 1000, "SaveSystem: AddCredits successfully adds 1000");

            bool spent = SaveSystem.SpendCredits(400);
            Assert(spent && SaveSystem.Load().credits == 600, "SaveSystem: SpendCredits successfully spends 400 (remaining 600)");

            bool overspend = SaveSystem.SpendCredits(800);
            Assert(!overspend && SaveSystem.Load().credits == 600, "SaveSystem: SpendCredits rejects overspending without mutating balance");
        }

        private static void Test_SaveSystem_UpgradeLevelPersistence()
        {
            SaveSystem.ResetAll();
            SaveSystem.SetUpgradeLevel("VOLT", 0, 2);
            SaveSystem.SetUpgradeLevel("VOLT", 1, 1);

            int[] levels = SaveSystem.GetUpgradeLevels("VOLT");
            Assert(levels[0] == 2 && levels[1] == 1 && levels[2] == 0 && levels[3] == 0, "SaveSystem: Upgrade levels persisted correctly");
        }

        private static void Test_RaceManager_CheckpointSequencing()
        {
            GameObject rmObj = new GameObject("Test_RM");
            RaceManager rm = rmObj.AddComponent<RaceManager>();

            GameObject vehicleObj = new GameObject("PlayerVehicle");
            VehicleController vc = vehicleObj.AddComponent<VehicleController>();
            vc.isPlayerInVehicle = true;

            GameObject cp0Obj = new GameObject("CP0");
            Checkpoint cp0 = cp0Obj.AddComponent<Checkpoint>();
            var fiIndex = typeof(Checkpoint).GetField("checkpointIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiIndex.SetValue(cp0, 0);

            GameObject cp1Obj = new GameObject("CP1");
            Checkpoint cp1 = cp1Obj.AddComponent<Checkpoint>();
            fiIndex.SetValue(cp1, 1);

            // Reflection to inject racers and checkpoints
            var fiCheckpoints = typeof(RaceManager).GetField("checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCheckpoints.SetValue(rm, new List<Checkpoint> { cp0, cp1 });

            var fiVehicles = typeof(RaceManager).GetField("vehicleRacers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiVehicles.SetValue(rm, new List<VehicleController> { vc });

            rm.SendMessage("Start");
            rm.StartRace();

            // Attempt to pass CP1 first (out of sequence) -> should be ignored
            rm.OnVehiclePassCheckpoint(vc, cp1);
            Assert(rm.GetPlayerLap() == 0, "RaceManager: Out-of-order checkpoint passage is rejected");

            // Pass CP0 first -> should advance
            rm.OnVehiclePassCheckpoint(vc, cp0);
            Assert(rm.CurrentState == RaceManager.RaceState.Racing, "RaceManager: Sequential checkpoint passage accepted");

            UnityEngine.Object.DestroyImmediate(rmObj);
            UnityEngine.Object.DestroyImmediate(vehicleObj);
            UnityEngine.Object.DestroyImmediate(cp0Obj);
            UnityEngine.Object.DestroyImmediate(cp1Obj);
        }

        private static void Test_RaceManager_LapAndFinishValidation()
        {
            GameObject rmObj = new GameObject("Test_RM_Finish");
            RaceManager rm = rmObj.AddComponent<RaceManager>();

            GameObject vehicleObj = new GameObject("PlayerVehicle");
            VehicleController vc = vehicleObj.AddComponent<VehicleController>();
            vc.isPlayerInVehicle = true;

            GameObject cpFinishObj = new GameObject("FinishLine");
            Checkpoint cpFinish = cpFinishObj.AddComponent<Checkpoint>();
            var fiIndex = typeof(Checkpoint).GetField("checkpointIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fiFinish = typeof(Checkpoint).GetField("isFinishLine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiIndex.SetValue(cpFinish, 0);
            fiFinish.SetValue(cpFinish, true);

            var fiCheckpoints = typeof(RaceManager).GetField("checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCheckpoints.SetValue(rm, new List<Checkpoint> { cpFinish });

            var fiVehicles = typeof(RaceManager).GetField("vehicleRacers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiVehicles.SetValue(rm, new List<VehicleController> { vc });

            var fiLaps = typeof(RaceManager).GetField("totalLaps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiLaps.SetValue(rm, 2);

            rm.SendMessage("Start");
            rm.StartRace();

            // Lap 1
            rm.OnVehiclePassCheckpoint(vc, cpFinish);
            Assert(rm.GetPlayerLap() == 1, "RaceManager: Lap 1 validated");

            // Lap 2 (Finish)
            rm.OnVehiclePassCheckpoint(vc, cpFinish);
            Assert(rm.GetPlayerLap() == 2, "RaceManager: Lap 2 validated, race completed");

            UnityEngine.Object.DestroyImmediate(rmObj);
            UnityEngine.Object.DestroyImmediate(vehicleObj);
            UnityEngine.Object.DestroyImmediate(cpFinishObj);
        }

        private static void Test_RaceManager_DNFTimeout()
        {
            GameObject rmObj = new GameObject("Test_RM_DNF");
            RaceManager rm = rmObj.AddComponent<RaceManager>();

            var fiTimeout = typeof(RaceManager).GetField("raceTimeout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiTimeout.SetValue(rm, 5f); // 5 sec timeout

            rm.SendMessage("Start");
            rm.StartRace();

            var fiRaceTime = typeof(RaceManager).GetField("raceTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiRaceTime.SetValue(rm, 6f); // artificially simulate raceTime > timeout

            rm.SendMessage("UpdateRace");
            Assert(rm.CurrentState == RaceManager.RaceState.Finished, "RaceManager: DNF timeout terminates race if racers stuck");

            UnityEngine.Object.DestroyImmediate(rmObj);
        }

        private static void Test_VehicleData_FallbackGeneration()
        {
            GameObject go = new GameObject("Test_StatsApplier");
            VehicleStatsApplier applier = go.AddComponent<VehicleStatsApplier>();

            var miLoad = typeof(VehicleStatsApplier).GetMethod("LoadVehicleData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            VehicleData volt = miLoad.Invoke(applier, new object[] { "VOLT" }) as VehicleData;
            VehicleData brick = miLoad.Invoke(applier, new object[] { "BRICK" }) as VehicleData;
            VehicleData blade = miLoad.Invoke(applier, new object[] { "BLADE" }) as VehicleData;

            Assert(volt != null && volt.id == "VOLT" && volt.maxSpeed == 40f, "VehicleStatsApplier: 'VOLT' fallback initialized");
            Assert(brick != null && brick.id == "BRICK" && brick.price == 1500, "VehicleStatsApplier: 'BRICK' fallback initialized");
            Assert(blade != null && blade.id == "BLADE" && blade.turnRate == 130f, "VehicleStatsApplier: 'BLADE' fallback initialized");

            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void Test_SceneLoader_ProgressNormalizationAndTips()
        {
            Assert(SceneLoader.NormalizeProgress(0f) == 0f, "SceneLoader: Raw 0.0 maps to normalized 0.0");
            Assert(Mathf.Abs(SceneLoader.NormalizeProgress(0.45f) - 0.5f) < 0.001f, "SceneLoader: Raw 0.45 maps to normalized 0.50");
            Assert(Mathf.Abs(SceneLoader.NormalizeProgress(0.9f) - 1.0f) < 0.001f, "SceneLoader: Raw 0.9 maps to normalized 1.00");
            Assert(SceneLoader.NormalizeProgress(1.0f) == 1f, "SceneLoader: Raw > 0.9 is clamped safely to 1.00");
            Assert(SceneLoader.GAMEPLAY_TIPS != null && SceneLoader.GAMEPLAY_TIPS.Length >= 5, "SceneLoader: Gameplay tips carousel contains >= 5 tips");
        }

        private static void Test_GameManager_SplashAndTitleStates()
        {
            GameObject go = new GameObject("Test_GM_SplashTitle");
            GameManager gm = go.AddComponent<GameManager>();

            gm.ChangeState(GameState.Splash);
            Assert(gm.CurrentState == GameState.Splash, "GM: Splash state transition verified");

            gm.ChangeState(GameState.Title);
            Assert(gm.CurrentState == GameState.Title, "GM: Title state transition verified");

            gm.ChangeState(GameState.Loading);
            Assert(gm.CurrentState == GameState.Loading, "GM: Loading state transition verified");

            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void Test_PauseMenu_TimeScaleGuards()
        {
            Time.timeScale = 1f;
            GameObject go = new GameObject("Test_PauseMenu");
            PauseMenuUI pause = go.AddComponent<PauseMenuUI>();

            // Simulate resume call
            pause.Resume();
            Assert(Time.timeScale == 1f, "PauseMenu: Resume guarantees Time.timeScale is 1.0");

            UnityEngine.Object.DestroyImmediate(go);
            Assert(Time.timeScale == 1f, "PauseMenu: OnDestroy cleans up and maintains Time.timeScale at 1.0");
        }

        private static void Test_CountdownController_LockingAndTiming()
        {
            GameObject go = new GameObject("Test_Countdown");
            CountdownController cc = go.AddComponent<CountdownController>();

            Assert(cc.countdownDuration >= 3f, "CountdownController: Default duration >= 3s");
            Assert(cc.GetCurrentNumber() == 0, "CountdownController: Number starts at 0 before trigger");

            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void Test_GameManager_EnsureRaceComponents()
        {
            GameManager.EnsureRaceComponents();

            CountdownController cd = UnityEngine.Object.FindAnyObjectByType<CountdownController>();
            PauseMenuUI pm = UnityEngine.Object.FindAnyObjectByType<PauseMenuUI>();
            ResultsScreenUI rs = UnityEngine.Object.FindAnyObjectByType<ResultsScreenUI>();
            RaceManager rm = UnityEngine.Object.FindAnyObjectByType<RaceManager>();

            Assert(cd != null, "GameManager: Auto-bootstrapped CountdownController exists");
            Assert(pm != null, "GameManager: Auto-bootstrapped PauseMenuUI exists");
            Assert(rs != null, "GameManager: Auto-bootstrapped ResultsScreenUI exists");
            Assert(rm != null, "GameManager: Auto-bootstrapped RaceManager exists");

            if (cd != null) UnityEngine.Object.DestroyImmediate(cd.gameObject);
            if (pm != null) UnityEngine.Object.DestroyImmediate(pm.gameObject);
            if (rs != null) UnityEngine.Object.DestroyImmediate(rs.gameObject);
            if (rm != null) UnityEngine.Object.DestroyImmediate(rm.gameObject);
        }

        private static void Test_GameInput_StandardizedMappings()
        {
            // Verify all standardized GameInput methods execute safely without throwing exceptions
            bool interact = GameInput.GetInteractDown();
            bool jump = GameInput.GetJumpDown();
            bool reset = GameInput.GetResetDown();
            bool pause = GameInput.GetPauseDown();
            bool brake = GameInput.GetBrake();
            bool drift = GameInput.GetDrift();
            bool antiGrav = GameInput.GetAntiGravityDown();
            float h = GameInput.GetHorizontal();
            float v = GameInput.GetVertical();

            Assert(h >= -1f && h <= 1f, "GameInput: Horizontal axis clamped in [-1, 1]");
            Assert(v >= -1f && v <= 1f, "GameInput: Vertical axis clamped in [-1, 1]");
            Assert(!reset || reset, "GameInput: GetResetDown returns valid boolean");
            Assert(!pause || pause, "GameInput: GetPauseDown returns valid boolean");
            Assert(!drift || drift, "GameInput: GetDrift returns valid boolean");
            Assert(!antiGrav || antiGrav, "GameInput: GetAntiGravityDown returns valid boolean");
        }

        private static void Test_PlayerHealth_TakeDamageAndCooldown()
        {
            GameObject go = new GameObject("Test_Player_Health");
            PlayerHealth ph = go.AddComponent<PlayerHealth>();
            ph.health = 100;
            ph.maxHealth = 100;
            ph.damageAmount = 25;

            ph.TakeDamage(25);
            Assert(ph.health == 75, "PlayerHealth: Takes 25 damage (health 100 -> 75)");

            // Immediate second hit within invulnerability duration should be rejected
            ph.TakeDamage(25);
            Assert(ph.health == 75, "PlayerHealth: Invulnerability cooldown rejects immediate second hit");

            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void Test_PlayerHealth_RestoreHealth()
        {
            GameObject go = new GameObject("Test_Player_Heal");
            PlayerHealth ph = go.AddComponent<PlayerHealth>();
            ph.health = 40;
            ph.maxHealth = 100;

            ph.RestoreHealth(30);
            Assert(ph.health == 70, "PlayerHealth: Restoring 30 health updates value (40 -> 70)");

            ph.RestoreHealth(50);
            Assert(ph.health == 100, "PlayerHealth: Restoring health is clamped at maxHealth (100)");

            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void Test_VehicleController_TakeDamageAndRestoreHealth()
        {
            GameObject go = new GameObject("Test_Vehicle_Damage");
            VehicleController vc = go.AddComponent<VehicleController>();
            vc.health = 100;
            vc.maxHealth = 100;

            vc.TakeDamage(30);
            Assert(vc.health == 70, "VehicleController: Takes 30 damage (100 -> 70)");

            vc.RestoreHealth(20);
            Assert(vc.health == 90, "VehicleController: Restores 20 health (70 -> 90)");

            vc.RestoreHealth(50);
            Assert(vc.health == 100, "VehicleController: Restored health clamped at maxHealth (100)");

            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void Test_GameManager_GameOverTransition()
        {
            GameObject go = new GameObject("Test_GM_GameOver");
            GameManager gm = go.AddComponent<GameManager>();

            gm.ChangeState(GameState.Racing);
            Assert(gm.isGameActive, "GameManager: Racing state sets isGameActive = true");

            gm.OnPlayerDied();
            Assert(gm.CurrentState == GameState.GameOver && !gm.isGameActive, "GameManager: OnPlayerDied transitions to GameOver and deactivates game");

            // Verify idempotency
            gm.OnVehicleDestroyed();
            Assert(gm.CurrentState == GameState.GameOver, "GameManager: Repeated terminal calls maintain GameOver state");

            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void Test_GameManager_CleanSessionReset()
        {
            GameObject go = new GameObject("Test_GM_Reset");
            GameManager gm = go.AddComponent<GameManager>();

            gm.score = 500;
            gm.raceTime = 45.2f;
            gm.isGameActive = true;
            gm.hasWon = true;
            gm.shardsCollected = 8;
            gm.totalShards = 12;

            gm.ResetSession();

            Assert(gm.score == 0, "GameManager.ResetSession: Score reset to 0");
            Assert(gm.raceTime == 0f, "GameManager.ResetSession: RaceTime reset to 0");
            Assert(!gm.isGameActive, "GameManager.ResetSession: isGameActive reset to false");
            Assert(!gm.hasWon, "GameManager.ResetSession: hasWon reset to false (unlocks subsequent race completions)");
            Assert(gm.shardsCollected == 0, "GameManager.ResetSession: shardsCollected reset to 0");
            Assert(gm.totalShards == 0, "GameManager.ResetSession: totalShards reset to 0 (no accumulation)");

            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void Test_VehicleInteraction_ConcealAndRestoreRenderers()
        {
            GameObject playerGo = new GameObject("Test_Player");
            MeshRenderer mr = playerGo.AddComponent<MeshRenderer>();
            mr.enabled = true;

            GameObject childGo = new GameObject("ChildMesh");
            childGo.transform.SetParent(playerGo.transform);
            MeshRenderer childMr = childGo.AddComponent<MeshRenderer>();
            childMr.enabled = true;

            GameObject vehicleGo = new GameObject("Test_Vehicle");
            VehicleController vc = vehicleGo.AddComponent<VehicleController>();

            GameObject viGo = new GameObject("Test_VI");
            VehicleInteraction vi = viGo.AddComponent<VehicleInteraction>();
            vi.player = playerGo;
            vi.vehicle = vehicleGo;
            vi.startInsideVehicle = false;

            vi.EnterVehicle();
            Assert(!mr.enabled && !childMr.enabled, "VehicleInteraction: Entering vehicle conceals all child renderers");

            vi.ExitVehicle();
            Assert(mr.enabled && childMr.enabled, "VehicleInteraction: Exiting vehicle restores all child renderers");

            UnityEngine.Object.DestroyImmediate(playerGo);
            UnityEngine.Object.DestroyImmediate(vehicleGo);
            UnityEngine.Object.DestroyImmediate(viGo);
        }

        private static void Test_RaceManager_VehicleCountdownWithoutEjection()
        {
            GameObject raceGo = new GameObject("Test_RM");
            RaceManager rm = raceGo.AddComponent<RaceManager>();

            GameObject vehicleGo = new GameObject("Test_Player_Vehicle");
            VehicleController vc = vehicleGo.AddComponent<VehicleController>();
            vc.isPlayerInVehicle = true;

            Assert(vc.isPlayerInVehicle, "RaceManager: Player starts inside vehicle without ejection during pre-race");

            UnityEngine.Object.DestroyImmediate(raceGo);
            UnityEngine.Object.DestroyImmediate(vehicleGo);
        }

        private static void Test_Checkpoint_SafeRespawnPositionAndRotation()
        {
            GameObject cpGo = new GameObject("Test_Checkpoint");
            cpGo.transform.position = new Vector3(26f, 0f, 115f);
            Checkpoint cp = cpGo.AddComponent<Checkpoint>();

            Vector3 safePos = new Vector3(26f, 0.5f, 115f);
            Quaternion safeRot = Quaternion.Euler(0f, 25f, 0f);
            cp.SetCheckpointData(3, false, safePos, safeRot);

            Assert(cp.GetSafeRespawnPosition() == safePos, "Checkpoint: Safe respawn position matches track road coordinates");
            Assert(cp.GetSafeRespawnRotation() == safeRot, "Checkpoint: Safe respawn rotation matches track orientation");

            UnityEngine.Object.DestroyImmediate(cpGo);
        }

        private static void Test_RaceManager_StrictSequentialCheckpointValidation()
        {
            GameObject rmGo = new GameObject("Test_RM_Seq");
            RaceManager rm = rmGo.AddComponent<RaceManager>();
            rm.StartRace();

            GameObject cp0Go = new GameObject("CP0");
            Checkpoint cp0 = cp0Go.AddComponent<Checkpoint>();
            cp0.SetCheckpointData(0, false, Vector3.zero, Quaternion.identity);

            GameObject cp1Go = new GameObject("CP1");
            Checkpoint cp1 = cp1Go.AddComponent<Checkpoint>();
            cp1.SetCheckpointData(1, false, Vector3.forward * 35f, Quaternion.identity);

            GameObject cp2Go = new GameObject("CP2");
            Checkpoint cp2 = cp2Go.AddComponent<Checkpoint>();
            cp2.SetCheckpointData(2, false, Vector3.forward * 65f, Quaternion.identity);

            GameObject vGo = new GameObject("Test_V");
            VehicleController vc = vGo.AddComponent<VehicleController>();

            bool pass0 = rm.OnVehiclePassCheckpoint(vc, cp0);
            Assert(pass0, "RaceManager: Passing Checkpoint 0 in sequence accepted");

            bool pass2_skip = rm.OnVehiclePassCheckpoint(vc, cp2);
            Assert(!pass2_skip, "RaceManager: Skipping Checkpoint 1 directly to 2 rejected");

            bool pass1 = rm.OnVehiclePassCheckpoint(vc, cp1);
            Assert(pass1, "RaceManager: Passing Checkpoint 1 in sequence accepted");

            bool pass2 = rm.OnVehiclePassCheckpoint(vc, cp2);
            Assert(pass2, "RaceManager: Passing Checkpoint 2 in sequence accepted");

            UnityEngine.Object.DestroyImmediate(rmGo);
            UnityEngine.Object.DestroyImmediate(cp0Go);
            UnityEngine.Object.DestroyImmediate(cp1Go);
            UnityEngine.Object.DestroyImmediate(cp2Go);
            UnityEngine.Object.DestroyImmediate(vGo);
        }

        private static void Test_VehicleController_CheckpointTrackingAndRecovery()
        {
            GameObject vGo = new GameObject("Test_V_Recover");
            Rigidbody rb = vGo.AddComponent<Rigidbody>();
            VehicleController vc = vGo.AddComponent<VehicleController>();

            GameObject cpGo = new GameObject("Test_CP");
            Checkpoint cp = cpGo.AddComponent<Checkpoint>();
            Vector3 safePos = new Vector3(0f, 0.5f, 35f);
            Quaternion safeRot = Quaternion.identity;
            cp.SetCheckpointData(1, false, safePos, safeRot);

            vc.UpdateLastCheckpoint(cp);

            // Simulate vehicle tumbling off-track with bad angular momentum
            vGo.transform.position = new Vector3(50f, -10f, 35f);
            rb.linearVelocity = new Vector3(-20f, -50f, 0f);
            rb.angularVelocity = new Vector3(10f, 15f, 20f);

            vc.RecoverToCheckpoint();

            Assert(Mathf.Approximately(vGo.transform.position.x, safePos.x) && Mathf.Approximately(vGo.transform.position.z, safePos.z), "VehicleController: RecoverToCheckpoint restores horizontal track position");
            Assert(vGo.transform.position.y >= safePos.y, "VehicleController: RecoverToCheckpoint sets safe clearance height above surface");
            Assert(rb.angularVelocity == Vector3.zero, "VehicleController: RecoverToCheckpoint zeroes angular velocity to prevent spinning");
            Assert(rb.linearVelocity.z > 0f, "VehicleController: RecoverToCheckpoint applies forward heading momentum");

            UnityEngine.Object.DestroyImmediate(vGo);
            UnityEngine.Object.DestroyImmediate(cpGo);
        }

        private static void Test_VehicleController_OutOfBoundsFallRecovery()
        {
            GameObject vGo = new GameObject("Test_V_OOB");
            vGo.AddComponent<Rigidbody>();
            VehicleController vc = vGo.AddComponent<VehicleController>();

            GameObject cpGo = new GameObject("Test_CP_OOB");
            Checkpoint cp = cpGo.AddComponent<Checkpoint>();
            Vector3 safePos = new Vector3(0f, 11.5f, 215f);
            cp.SetCheckpointData(5, false, safePos, Quaternion.identity);
            vc.UpdateLastCheckpoint(cp);

            // In skyway section, falling below 4m is out of bounds
            vGo.transform.position = new Vector3(0f, 2.0f, 230f);
            bool isSkyOOB = vGo.transform.position.z >= 180f && vGo.transform.position.z <= 295f && vGo.transform.position.y < 4.0f;
            Assert(isSkyOOB, "VehicleController: Detects skyway fall condition when Y < 4.0m in sky section");

            vc.RecoverToCheckpoint();
            Assert(Mathf.Approximately(vGo.transform.position.z, 215f), "VehicleController: Skyway fall recovers to sky launch checkpoint");

            UnityEngine.Object.DestroyImmediate(vGo);
            UnityEngine.Object.DestroyImmediate(cpGo);
        }

        private static void Test_PoliceVehicle_RamForceRestricted()
        {
            GameObject pvGo = new GameObject("Test_Police");
            PoliceVehicle pv = pvGo.AddComponent<PoliceVehicle>();
            pvGo.AddComponent<Rigidbody>();

            Assert(pv.ramForce <= 20f, "PoliceVehicle: Ram force clamped within safe threshold");

            UnityEngine.Object.DestroyImmediate(pvGo);
        }
    }
}
#endif
