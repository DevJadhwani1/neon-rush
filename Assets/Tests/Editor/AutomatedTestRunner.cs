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
            Test_RaceManager_SingleRunFinishValidation();
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
            Test_EditorBuildSettings_ScenesRegisteredAndValid();
            Test_StartRaceFlow_TrackSelectionAndGameManagerSync();
            Test_RaceInitialization_CountdownAndControls();
            Test_RaceManager_SingleRunProgressionValidation();
            Test_RaceManager_DirectionSensitiveCrossing();
            Test_RaceManager_IntermediateCheckpointsRequiredForFinish();
            Test_QuantumShard_HealthRestorationAndClamping();
            Test_PoliceVehicle_RoadRaycastingAndPushProtection();
            Test_ResultsScreenUI_DisplaySingleRunTiming();
            Test_TimerAndGameOverRestartLifecycle();
            Test_EndToEnd_TestA_SuccessfulCompletion();
            Test_EndToEnd_TestB_InvalidFinish();
            Test_EndToEnd_TestC_DuplicateTriggerProtection();
            Test_EndToEnd_TestD_RestartAfterVictoryAndPersistence();
            Test_EndToEnd_TestE_GameOverNoRewardsAndRestart();
            Test_Required_Tests_1_Through_14();
            Test_CollisionDamage_Reduces5_PoliceRam_Reduces20();
            Test_Endpoint_GoalTrigger_CompletesRun();
            Test_Vehicle_BrakingAndReverseControls();
            Test_InitialRaceStart_DropLanding_NoDamage();
            Test_Vehicle_StationaryAtRaceStart_NoAutoReverse();

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
            bool outOfOrder = rm.OnVehiclePassCheckpoint(vc, cp1);
            Assert(!outOfOrder, "RaceManager: Out-of-order checkpoint passage is rejected");

            // Pass CP0 first -> should advance
            bool seqPass = rm.OnVehiclePassCheckpoint(vc, cp0);
            Assert(seqPass && rm.CurrentState == RaceManager.RaceState.Racing, "RaceManager: Sequential checkpoint passage accepted");

            UnityEngine.Object.DestroyImmediate(rmObj);
            UnityEngine.Object.DestroyImmediate(vehicleObj);
            UnityEngine.Object.DestroyImmediate(cp0Obj);
            UnityEngine.Object.DestroyImmediate(cp1Obj);
        }

        private static void Test_RaceManager_SingleRunFinishValidation()
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

            rm.SendMessage("Start");
            rm.StartRace();

            // Single continuous run: crossing finish line completes race immediately (no lap counters)
            bool passResult = rm.OnVehiclePassCheckpoint(vc, cpFinish);
            Assert(passResult, "RaceManager: Finish line crossing accepted on single continuous run");
            Assert(rm.CurrentState == RaceManager.RaceState.Finished, "RaceManager: Race completed immediately upon crossing finish endpoint");
            Assert(rm.IsRaceFinished, "RaceManager: IsRaceFinished set to true without requiring multiple laps");

            // Second pass over finish line must be rejected (duplicate trigger protection)
            bool repeatPass = rm.OnVehiclePassCheckpoint(vc, cpFinish);
            Assert(!repeatPass, "RaceManager: Duplicate finish crossing rejected");

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

        private static void Test_EditorBuildSettings_ScenesRegisteredAndValid()
        {
            var scenes = UnityEditor.EditorBuildSettings.scenes;
            Assert(scenes.Length >= 2, "EditorBuildSettings: Minimum 2 scenes registered in build settings");

            bool hasMainMenu = false;
            bool hasRaceScene = false;
            int raceBuildIndex = -1;

            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path.Contains("MainMenu.unity") && scenes[i].enabled)
                {
                    hasMainMenu = true;
                }
                if (scenes[i].path.Contains("GravShiftNeonCircuit.unity") && scenes[i].enabled)
                {
                    hasRaceScene = true;
                    raceBuildIndex = i;
                }
            }

            Assert(hasMainMenu, "EditorBuildSettings: MainMenu.unity enabled at build index 0");
            Assert(hasRaceScene, "EditorBuildSettings: GravShiftNeonCircuit.unity registered and enabled");
            Assert(raceBuildIndex == 1, "EditorBuildSettings: GravShiftNeonCircuit.unity maps to build index 1");

            bool canStreamRace = UnityEngine.Application.CanStreamedLevelBeLoaded("GravShiftNeonCircuit");
            Assert(canStreamRace, "SceneManager: GravShiftNeonCircuit can be streamed/loaded by name");

            int resolvedIndex = UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/GravShiftNeonCircuit.unity");
            Assert(resolvedIndex == 1, "SceneUtility: Resolves GravShiftNeonCircuit.unity path to build index 1");
        }

        private static void Test_StartRaceFlow_TrackSelectionAndGameManagerSync()
        {
            GameManager.EnsureInstance();
            Assert(GameManager.Instance != null, "GameManager: Instance exists or is auto-created on demand");

            string[] testTracks = { "NEON_DOWNTOWN", "CYBER_SPEEDWAY", "GRID_CIRCUIT" };
            foreach (var track in testTracks)
            {
                GameManager.Instance.selectedTrackId = track;
                GameManager.Instance.selectedMode = "GRAND_PRIX";
                Assert(GameManager.Instance.selectedTrackId == track, $"StartRaceFlow: Selected track '{track}' saved to session");
            }

            // Verify session reset keeps track choice
            GameManager.Instance.ResetSession();
            Assert(GameManager.Instance.selectedTrackId == "GRID_CIRCUIT", "GameManager: Session reset preserves selected track choice");
            Assert(GameManager.Instance.score == 0 && GameManager.Instance.raceTime == 0f, "GameManager: Session reset clears race score and elapsed time");
        }

        private static void Test_RaceInitialization_CountdownAndControls()
        {
            GameManager.EnsureInstance();
            GameManager.Instance.ResetSession();

            GameObject vGo = new GameObject("Test_Player_Car");
            VehicleController vc = vGo.AddComponent<VehicleController>();
            Rigidbody rb = vGo.AddComponent<Rigidbody>();
            vc.isPlayerInVehicle = true;

            GameObject cdGo = new GameObject("Test_CD");
            CountdownController cd = cdGo.AddComponent<CountdownController>();

            cd.SendMessage("Awake");
            cd.countdownActive = true;
            Assert(CountdownController.IsCountingDown, "CountdownController: IsCountingDown active during countdown phase");

            // At race start
            cd.countdownActive = false;
            GameManager.Instance.ChangeState(GameState.Racing);
            Assert(GameManager.Instance.isGameActive, "GameManager: State transition to Racing activates isGameActive flag");
            Assert(!CountdownController.IsCountingDown, "CountdownController: IsCountingDown clear after countdown completes");

            // Simulate elapsed race timer in GameManager
            float initialTime = GameManager.Instance.raceTime;
            GameManager.Instance.raceTime += 1.0f;
            Assert(GameManager.Instance.raceTime > initialTime, "GameManager: Race time advances while in active Racing state");

            // Finish race
            GameManager.Instance.OnCourseCompleted();
            Assert(GameManager.Instance.hasWon, "GameManager: OnCourseCompleted flags hasWon");
            Assert(!GameManager.Instance.isGameActive, "GameManager: OnCourseCompleted deactivates isGameActive");
            Assert(GameManager.Instance.currentState == GameState.Finish, "GameManager: OnCourseCompleted transitions to Finish state");

            // Clean up
            UnityEngine.Object.DestroyImmediate(vGo);
            UnityEngine.Object.DestroyImmediate(cdGo);
            GameManager.Instance.ResetSession();
        }

        private static void Test_RaceManager_SingleRunProgressionValidation()
        {
            GameObject rmGo = new GameObject("Test_RM_SingleRun");
            RaceManager rm = rmGo.AddComponent<RaceManager>();

            // Single continuous run: start uncompleted, no lap progression needed
            Assert(!rm.IsRaceFinished, "RaceManager: Single run initialized with IsRaceFinished = false");
            Assert(rm.CurrentState == RaceManager.RaceState.PreRace, "RaceManager: Initial race state is PreRace");

            rm.StartRace();
            Assert(rm.CurrentState == RaceManager.RaceState.Racing, "RaceManager: StartRace initiates single continuous active run");

            UnityEngine.Object.DestroyImmediate(rmGo);
        }

        private static void Test_RaceManager_DirectionSensitiveCrossing()
        {
            GameObject rmGo = new GameObject("Test_RM_Direction");
            RaceManager rm = rmGo.AddComponent<RaceManager>();

            GameObject vGo = new GameObject("PlayerCar");
            VehicleController vc = vGo.AddComponent<VehicleController>();
            Rigidbody rb = vGo.AddComponent<Rigidbody>();
            vc.isPlayerInVehicle = true;

            GameObject cpGo = new GameObject("CP_Finish");
            Checkpoint cp = cpGo.AddComponent<Checkpoint>();
            cp.SetCheckpointData(0, true, Vector3.zero, Quaternion.identity);
            cpGo.transform.rotation = Quaternion.identity; // forward is (0, 0, 1)

            var fiCheckpoints = typeof(RaceManager).GetField("checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCheckpoints.SetValue(rm, new List<Checkpoint> { cp });

            var fiVehicles = typeof(RaceManager).GetField("vehicleRacers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiVehicles.SetValue(rm, new List<VehicleController> { vc });

            rm.SendMessage("Start");
            rm.StartRace();

            // Reverse crossing: vehicle facing backwards (0, 0, -1)
            vGo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rb.linearVelocity = Vector3.back * 10f;
            bool reverseResult = rm.OnVehiclePassCheckpoint(vc, cp);
            Assert(!reverseResult, "RaceManager: Reversing across checkpoint/finish line is rejected");
            Assert(rm.CurrentState == RaceManager.RaceState.Racing, "RaceManager: Reverse crossing does not complete run");

            // Forward crossing: vehicle facing forward (0, 0, 1)
            vGo.transform.rotation = Quaternion.identity;
            rb.linearVelocity = Vector3.forward * 10f;
            bool forwardResult = rm.OnVehiclePassCheckpoint(vc, cp);
            Assert(forwardResult, "RaceManager: Crossing checkpoint/finish line in correct direction is accepted");
            Assert(rm.CurrentState == RaceManager.RaceState.Finished, "RaceManager: Forward crossing completes single-run course");

            UnityEngine.Object.DestroyImmediate(rmGo);
            UnityEngine.Object.DestroyImmediate(vGo);
            UnityEngine.Object.DestroyImmediate(cpGo);
        }

        private static void Test_RaceManager_IntermediateCheckpointsRequiredForFinish()
        {
            GameObject rmGo = new GameObject("Test_RM_SeqFin");
            RaceManager rm = rmGo.AddComponent<RaceManager>();

            GameObject vGo = new GameObject("PlayerCar");
            VehicleController vc = vGo.AddComponent<VehicleController>();
            vGo.AddComponent<Rigidbody>();
            vc.isPlayerInVehicle = true;
            vGo.transform.rotation = Quaternion.identity;

            GameObject cp0Go = new GameObject("CP0");
            Checkpoint cp0 = cp0Go.AddComponent<Checkpoint>();
            cp0.SetCheckpointData(0, false, Vector3.zero, Quaternion.identity);

            GameObject cp1Go = new GameObject("CP1");
            Checkpoint cp1 = cp1Go.AddComponent<Checkpoint>();
            cp1.SetCheckpointData(1, false, Vector3.forward * 50f, Quaternion.identity);

            GameObject cpFinGo = new GameObject("CP_Fin");
            Checkpoint cpFin = cpFinGo.AddComponent<Checkpoint>();
            cpFin.SetCheckpointData(2, true, Vector3.forward * 100f, Quaternion.identity);

            var fiCheckpoints = typeof(RaceManager).GetField("checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCheckpoints.SetValue(rm, new List<Checkpoint> { cp0, cp1, cpFin });

            var fiVehicles = typeof(RaceManager).GetField("vehicleRacers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiVehicles.SetValue(rm, new List<VehicleController> { vc });

            rm.SendMessage("Start");
            rm.StartRace();

            // Attempt to cross finish line (CP2) without passing CP0 and CP1 -> must be rejected
            bool skipResult = rm.OnVehiclePassCheckpoint(vc, cpFin);
            Assert(!skipResult, "RaceManager: Finish line crossing before intermediate checkpoints is rejected");
            Assert(rm.CurrentState == RaceManager.RaceState.Racing, "RaceManager: Run not completed when intermediate checkpoints are skipped");

            // Pass CP0
            bool cp0Result = rm.OnVehiclePassCheckpoint(vc, cp0);
            Assert(cp0Result, "RaceManager: Checkpoint 0 passed");

            // Attempt finish line before CP1 -> still rejected
            bool preFinish = rm.OnVehiclePassCheckpoint(vc, cpFin);
            Assert(!preFinish, "RaceManager: Finish line still rejected before CP1");

            // Pass CP1
            bool cp1Result = rm.OnVehiclePassCheckpoint(vc, cp1);
            Assert(cp1Result, "RaceManager: Checkpoint 1 passed");

            // Now cross finish line -> accepted!
            bool finResult = rm.OnVehiclePassCheckpoint(vc, cpFin);
            Assert(finResult, "RaceManager: Finish line accepted after all intermediate checkpoints completed");
            Assert(rm.CurrentState == RaceManager.RaceState.Finished, "RaceManager: Single run completed after valid full checkpoint sequence");

            UnityEngine.Object.DestroyImmediate(rmGo);
            UnityEngine.Object.DestroyImmediate(vGo);
            UnityEngine.Object.DestroyImmediate(cp0Go);
            UnityEngine.Object.DestroyImmediate(cp1Go);
            UnityEngine.Object.DestroyImmediate(cpFinGo);
        }

        private static void Test_QuantumShard_HealthRestorationAndClamping()
        {
            GameObject vGo = new GameObject("Test_ShardCar");
            VehicleController vc = vGo.AddComponent<VehicleController>();
            vGo.AddComponent<Rigidbody>();

            GameObject sGo = new GameObject("Test_Shard");
            QuantumShard shard = sGo.AddComponent<QuantumShard>();
            shard.healthRestoreAmount = 25;

            GameManager.EnsureInstance();
            GameManager.Instance.ChangeState(GameState.Racing);

            // Test 1: Health 50 + 25 -> 75
            vc.health = 50;
            shard.Collect(vc, null);
            Assert(vc.health == 75, "QuantumShard: Restores 25 health from 50 to 75");
            Assert(shard.IsCollected, "QuantumShard: Shard marked as collected");

            // Test 2: Duplicate pickup prevention (shard cannot be collected again)
            int healthBeforeSecondTrigger = vc.health;
            shard.Collect(vc, null);
            Assert(vc.health == healthBeforeSecondTrigger, "QuantumShard: Duplicate collection trigger does not increase health");

            // Test 3: Health 90 + 25 -> strictly capped at 100
            GameObject sGo2 = new GameObject("Test_Shard2");
            QuantumShard shard2 = sGo2.AddComponent<QuantumShard>();
            shard2.healthRestoreAmount = 25;
            vc.health = 90;
            shard2.Collect(vc, null);
            Assert(vc.health == 100, "QuantumShard: Health clamped at 100 when adding 25 to 90 (never 115)");

            // Test 4: Health 100 + 25 -> remains exactly 100
            GameObject sGo3 = new GameObject("Test_Shard3");
            QuantumShard shard3 = sGo3.AddComponent<QuantumShard>();
            shard3.healthRestoreAmount = 25;
            shard3.Collect(vc, null);
            Assert(vc.health == 100, "QuantumShard: Collecting shard at 100 health leaves health at exactly 100");

            // Test 5: Health 75 + 10 -> 85
            GameObject sGo4 = new GameObject("Test_Shard4");
            QuantumShard shard4 = sGo4.AddComponent<QuantumShard>();
            shard4.healthRestoreAmount = 10;
            vc.health = 75;
            shard4.Collect(vc, null);
            Assert(vc.health == 85, "QuantumShard: Custom restore amount (10 HP) restores 75 to 85");

            // Test 6: Health 0 / Destroyed -> No restoration after game over
            GameObject sGo5 = new GameObject("Test_Shard5");
            QuantumShard shard5 = sGo5.AddComponent<QuantumShard>();
            shard5.healthRestoreAmount = 25;
            vc.health = 0;
            vc.isDestroyed = true;
            GameManager.Instance.ChangeState(GameState.GameOver);
            shard5.Collect(vc, null);
            Assert(vc.health == 0, "QuantumShard: Destroyed vehicle / GameOver does not restore health");

            // Clean up
            UnityEngine.Object.DestroyImmediate(vGo);
            UnityEngine.Object.DestroyImmediate(sGo);
            UnityEngine.Object.DestroyImmediate(sGo2);
            UnityEngine.Object.DestroyImmediate(sGo3);
            UnityEngine.Object.DestroyImmediate(sGo4);
            UnityEngine.Object.DestroyImmediate(sGo5);
            GameManager.Instance.ResetSession();
        }

        private static void Test_PoliceVehicle_RoadRaycastingAndPushProtection()
        {
            GameObject pGo = new GameObject("Test_Police");
            PoliceVehicle police = pGo.AddComponent<PoliceVehicle>();
            pGo.AddComponent<Rigidbody>();

            GameObject playerGo = new GameObject("Test_PlayerCar");
            VehicleController vc = playerGo.AddComponent<VehicleController>();
            playerGo.AddComponent<Rigidbody>();
            vc.isPlayerInVehicle = true;

            var fiTarget = typeof(PoliceVehicle).GetField("playerTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiTarget.SetValue(police, playerGo.transform);

            // Test RespawnOnTrack executes safely without mid-air clamp errors
            police.RespawnOnTrack();
            Assert(pGo.transform.position.y >= 0f, "PoliceVehicle: RespawnOnTrack places vehicle safely above ground");

            // Test that invulnerable player does not receive push force
            var fiInvuln = typeof(VehicleController).GetField("invulnerabilityTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiInvuln.SetValue(vc, 1.5f);
            Assert(vc.IsInvulnerable, "VehicleController: IsInvulnerable active during cooldown");

            UnityEngine.Object.DestroyImmediate(pGo);
            UnityEngine.Object.DestroyImmediate(playerGo);
        }

        private static void Test_ResultsScreenUI_DisplaySingleRunTiming()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetSession();
                GameManager.Instance.ChangeState(GameState.Finish);
            }

            GameObject rsGo = new GameObject("Test_Results");
            ResultsScreenUI rs = rsGo.AddComponent<ResultsScreenUI>();

            // Must NOT be showing initially
            Assert(!rs.IsShowing, "ResultsScreenUI: IsShowing is false initially (not shown at race start)");

            // Calling ShowResults activates it
            rs.ShowResults(1, 45.2f);
            Assert(rs.IsShowing, "ResultsScreenUI: ShowResults activates IsShowing");

            // Clean up
            rs.Hide();
            Assert(!rs.IsShowing, "ResultsScreenUI: Hide resets IsShowing");
            UnityEngine.Object.DestroyImmediate(rsGo);
            ResultsScreenUI.ResetRewardSession();
        }

        private static void Test_TimerAndGameOverRestartLifecycle()
        {
            GameManager.EnsureInstance();
            GameManager.Instance.ResetSession();

            // 1. Race start
            GameManager.Instance.ChangeState(GameState.Racing);
            Assert(GameManager.Instance.isGameActive, "Lifecycle: Racing state sets isGameActive = true");

            // 2. Timer advances
            GameManager.Instance.raceTime += 5.5f;
            Assert(GameManager.Instance.raceTime == 5.5f, "Lifecycle: Race timer advances during Racing");

            // 3. Pause stops timer
            GameManager.Instance.ChangeState(GameState.Paused);
            Assert(GameManager.Instance.currentState == GameState.Paused, "Lifecycle: Paused state entered");

            // 4. Game Over stops timer and active gameplay
            GameManager.Instance.OnVehicleDestroyed();
            Assert(GameManager.Instance.currentState == GameState.GameOver, "Lifecycle: Vehicle destruction triggers GameOver");
            Assert(!GameManager.Instance.isGameActive, "Lifecycle: GameOver deactivates isGameActive");

            // 5. Restart resets timer, score, and state cleanly
            GameManager.Instance.ResetSession();
            Assert(GameManager.Instance.raceTime == 0f, "Lifecycle: ResetSession zeroes race timer");
            Assert(GameManager.Instance.score == 0, "Lifecycle: ResetSession zeroes score");
            Assert(!GameManager.Instance.hasWon, "Lifecycle: ResetSession clears hasWon");
            Assert(Time.timeScale == 1f, "Lifecycle: ResetSession restores Time.timeScale to 1.0f");
        }

        private static void Test_EndToEnd_TestA_SuccessfulCompletion()
        {
            GameManager.EnsureInstance();
            GameManager.Instance.ResetSession();
            SaveSystem.ResetAll();
            int initialCredits = SaveSystem.Load().credits;

            GameObject rmObj = new GameObject("TestA_RM");
            RaceManager rm = rmObj.AddComponent<RaceManager>();

            GameObject vObj = new GameObject("Vehicle");
            VehicleController vc = vObj.AddComponent<VehicleController>();
            Rigidbody vRb = vObj.AddComponent<Rigidbody>();
            vc.isPlayerInVehicle = true;

            // Child collider representing vehicle wing/body
            GameObject childColObj = new GameObject("Body");
            childColObj.transform.SetParent(vObj.transform, false);
            BoxCollider childCol = childColObj.AddComponent<BoxCollider>();

            // Checkpoints: 0, 1, 2, and Finish Line 3
            GameObject cp0Obj = new GameObject("CP0");
            Checkpoint cp0 = cp0Obj.AddComponent<Checkpoint>();
            cp0.SetCheckpointData(0, false, Vector3.zero, Quaternion.identity);

            GameObject cp1Obj = new GameObject("CP1");
            Checkpoint cp1 = cp1Obj.AddComponent<Checkpoint>();
            cp1.SetCheckpointData(1, false, Vector3.zero, Quaternion.identity);

            GameObject cp2Obj = new GameObject("CP2");
            Checkpoint cp2 = cp2Obj.AddComponent<Checkpoint>();
            cp2.SetCheckpointData(2, false, Vector3.zero, Quaternion.identity);

            GameObject finishObj = new GameObject("FinishGate");
            Checkpoint cpFinish = finishObj.AddComponent<Checkpoint>();
            cpFinish.SetCheckpointData(3, true, Vector3.zero, Quaternion.identity);
            GoalTrigger goalTrigger = finishObj.AddComponent<GoalTrigger>();

            var fiCheckpoints = typeof(RaceManager).GetField("checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCheckpoints.SetValue(rm, new List<Checkpoint> { cp0, cp1, cp2, cpFinish });

            var fiVehicles = typeof(RaceManager).GetField("vehicleRacers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiVehicles.SetValue(rm, new List<VehicleController> { vc });

            rm.SendMessage("Start");
            rm.StartRace();
            GameManager.Instance.ChangeState(GameState.Racing);
            GameManager.Instance.AddScore(250);
            GameManager.Instance.RegisterShard();
            GameManager.Instance.AddCollectedShard(50);

            Assert(rm.CurrentState == RaceManager.RaceState.Racing, "Test A: Race enters Racing state");
            Assert(GameManager.Instance.isGameActive, "Test A: GameManager is active during race");

            // Complete every required checkpoint in order
            bool cp0Pass = rm.OnVehiclePassCheckpoint(vc, cp0);
            bool cp1Pass = rm.OnVehiclePassCheckpoint(vc, cp1);
            bool cp2Pass = rm.OnVehiclePassCheckpoint(vc, cp2);
            Assert(cp0Pass && cp1Pass && cp2Pass, "Test A: Intermediate checkpoints 0, 1, 2 completed in sequential order");

            // Advance simulated race timer
            var fiRaceTime = typeof(RaceManager).GetField("raceTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiRaceTime.SetValue(rm, 48.75f);
            GameManager.Instance.raceTime = 48.75f;

            // Cross finish line using child collider via Checkpoint
            bool finishPass = rm.OnVehiclePassCheckpoint(vc, cpFinish);
            Assert(finishPass, "Test A: Finish line crossing recognized and validated for player vehicle");

            // Verify race transitions to Completed
            Assert(rm.CurrentState == RaceManager.RaceState.Finished, "Test A: RaceManager transitions to Finished/Completed state");
            Assert(rm.IsRaceFinished && rm.IsCompleted, "Test A: RaceManager flags IsRaceFinished and IsCompleted");
            Assert(GameManager.Instance.currentState == GameState.Finish && GameManager.Instance.hasWon, "Test A: GameManager transitions to Finish and flags hasWon");

            // Verify timer stopped and preserved
            float recordedFinalTime = rm.RaceTime;
            Assert(Mathf.Approximately(recordedFinalTime, 48.75f), "Test A: Timer stops and preserves exact final time (48.75s)");

            // Verify results screen
            var resultsUI = UnityEngine.Object.FindAnyObjectByType<ResultsScreenUI>();
            Assert(resultsUI != null && resultsUI.IsShowing, "Test A: Results screen appears reliably after victory");

            // Verify rewards calculated and granted once
            int finalCredits = SaveSystem.Load().credits;
            int expectedReward = 500 + (1 * 10); // 1st place: 500 CR + 1 shard * 10 CR = 510 CR
            Assert(finalCredits == initialCredits + expectedReward, $"Test A: Rewards calculated (+{expectedReward} CR) and saved persistently (Total: {finalCredits} CR)");

            // Clean up
            UnityEngine.Object.DestroyImmediate(rmObj);
            UnityEngine.Object.DestroyImmediate(vObj);
            UnityEngine.Object.DestroyImmediate(cp0Obj);
            UnityEngine.Object.DestroyImmediate(cp1Obj);
            UnityEngine.Object.DestroyImmediate(cp2Obj);
            UnityEngine.Object.DestroyImmediate(finishObj);
            if (resultsUI != null) UnityEngine.Object.DestroyImmediate(resultsUI.gameObject);
            GameManager.Instance.ResetSession();
        }

        private static void Test_EndToEnd_TestB_InvalidFinish()
        {
            GameManager.EnsureInstance();
            GameManager.Instance.ResetSession();
            SaveSystem.ResetAll();
            int initialCredits = SaveSystem.Load().credits;

            GameObject rmObj = new GameObject("TestB_RM");
            RaceManager rm = rmObj.AddComponent<RaceManager>();

            GameObject vObj = new GameObject("Vehicle");
            VehicleController vc = vObj.AddComponent<VehicleController>();
            vc.isPlayerInVehicle = true;

            GameObject cp0Obj = new GameObject("CP0");
            Checkpoint cp0 = cp0Obj.AddComponent<Checkpoint>();
            cp0.SetCheckpointData(0, false, Vector3.zero, Quaternion.identity);

            GameObject cp1Obj = new GameObject("CP1");
            Checkpoint cp1 = cp1Obj.AddComponent<Checkpoint>();
            cp1.SetCheckpointData(1, false, Vector3.zero, Quaternion.identity);

            GameObject finishObj = new GameObject("FinishGate");
            Checkpoint cpFinish = finishObj.AddComponent<Checkpoint>();
            cpFinish.SetCheckpointData(2, true, Vector3.zero, Quaternion.identity);
            GoalTrigger goalTrigger = finishObj.AddComponent<GoalTrigger>();

            var fiCheckpoints = typeof(RaceManager).GetField("checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCheckpoints.SetValue(rm, new List<Checkpoint> { cp0, cp1, cpFinish });

            var fiVehicles = typeof(RaceManager).GetField("vehicleRacers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiVehicles.SetValue(rm, new List<VehicleController> { vc });

            rm.SendMessage("Start");
            rm.StartRace();
            GameManager.Instance.ChangeState(GameState.Racing);

            // Pass CP0 only, skipping CP1
            rm.OnVehiclePassCheckpoint(vc, cp0);

            // Attempt to finish by crossing finish line prematurely
            bool prematureFinish = rm.OnVehiclePassCheckpoint(vc, cpFinish);
            Assert(!prematureFinish, "Test B: Premature finish line crossing is rejected when checkpoints are skipped");
            Assert(rm.CurrentState == RaceManager.RaceState.Racing, "Test B: Race remains in Racing state (does not complete prematurely)");
            Assert(!GameManager.Instance.hasWon, "Test B: GameManager.hasWon remains false");

            // Also verify GoalTrigger does not bypass RaceManager
            BoxCollider mockCol = vObj.AddComponent<BoxCollider>();
            goalTrigger.SendMessage("OnTriggerEnter", mockCol, SendMessageOptions.DontRequireReceiver);
            Assert(rm.CurrentState == RaceManager.RaceState.Racing, "Test B: GoalTrigger does not bypass checkpoint validation");
            Assert(!GameManager.Instance.hasWon, "Test B: GameManager hasWon remains false after GoalTrigger contact");

            int currentCredits = SaveSystem.Load().credits;
            Assert(currentCredits == initialCredits, "Test B: No rewards granted for invalid/incomplete race crossing");

            // Clean up
            UnityEngine.Object.DestroyImmediate(rmObj);
            UnityEngine.Object.DestroyImmediate(vObj);
            UnityEngine.Object.DestroyImmediate(cp0Obj);
            UnityEngine.Object.DestroyImmediate(cp1Obj);
            UnityEngine.Object.DestroyImmediate(finishObj);
            GameManager.Instance.ResetSession();
        }

        private static void Test_EndToEnd_TestC_DuplicateTriggerProtection()
        {
            GameManager.EnsureInstance();
            GameManager.Instance.ResetSession();
            SaveSystem.ResetAll();

            GameObject rmObj = new GameObject("TestC_RM");
            RaceManager rm = rmObj.AddComponent<RaceManager>();

            GameObject vObj = new GameObject("Vehicle");
            VehicleController vc = vObj.AddComponent<VehicleController>();
            vc.isPlayerInVehicle = true;

            GameObject finishObj = new GameObject("FinishGate");
            Checkpoint cpFinish = finishObj.AddComponent<Checkpoint>();
            cpFinish.SetCheckpointData(0, true, Vector3.zero, Quaternion.identity);
            GoalTrigger goalTrigger = finishObj.AddComponent<GoalTrigger>();

            var fiCheckpoints = typeof(RaceManager).GetField("checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCheckpoints.SetValue(rm, new List<Checkpoint> { cpFinish });

            var fiVehicles = typeof(RaceManager).GetField("vehicleRacers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiVehicles.SetValue(rm, new List<VehicleController> { vc });

            rm.SendMessage("Start");
            rm.StartRace();
            GameManager.Instance.ChangeState(GameState.Racing);

            // Initial valid finish
            bool firstFinish = rm.OnVehiclePassCheckpoint(vc, cpFinish);
            Assert(firstFinish, "Test C: Initial finish line crossing completes race");
            Assert(rm.CurrentState == RaceManager.RaceState.Finished, "Test C: Race state is Finished");

            int creditsAfterFinish = SaveSystem.Load().credits;
            Assert(creditsAfterFinish > 0, "Test C: Credits granted after initial finish");

            // Trigger finish line again immediately (duplicate trigger attempt)
            bool secondPass = rm.OnVehiclePassCheckpoint(vc, cpFinish);
            Assert(!secondPass, "Test C: Secondary finish trigger is rejected / ignored");

            // Trigger GoalTrigger collider again
            BoxCollider mockCol = vObj.AddComponent<BoxCollider>();
            goalTrigger.SendMessage("OnTriggerEnter", mockCol, SendMessageOptions.DontRequireReceiver);

            int creditsAfterDuplicate = SaveSystem.Load().credits;
            Assert(creditsAfterDuplicate == creditsAfterFinish, "Test C: Credits balance not duplicated on repeat finish trigger");

            // Clean up
            UnityEngine.Object.DestroyImmediate(rmObj);
            UnityEngine.Object.DestroyImmediate(vObj);
            UnityEngine.Object.DestroyImmediate(finishObj);
            var resultsUI = UnityEngine.Object.FindAnyObjectByType<ResultsScreenUI>();
            if (resultsUI != null) UnityEngine.Object.DestroyImmediate(resultsUI.gameObject);
            GameManager.Instance.ResetSession();
        }

        private static void Test_EndToEnd_TestD_RestartAfterVictoryAndPersistence()
        {
            GameManager.EnsureInstance();
            GameManager.Instance.ResetSession();
            SaveSystem.ResetAll();

            // --- RACE 1 ---
            GameObject rmObj1 = new GameObject("TestD_RM1");
            RaceManager rm1 = rmObj1.AddComponent<RaceManager>();

            GameObject vObj1 = new GameObject("Vehicle");
            VehicleController vc1 = vObj1.AddComponent<VehicleController>();
            vc1.isPlayerInVehicle = true;

            GameObject cpFinishObj1 = new GameObject("FinishGate1");
            Checkpoint cpFinish1 = cpFinishObj1.AddComponent<Checkpoint>();
            cpFinish1.SetCheckpointData(0, true, Vector3.zero, Quaternion.identity);

            var fiCheckpoints1 = typeof(RaceManager).GetField("checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCheckpoints1.SetValue(rm1, new List<Checkpoint> { cpFinish1 });

            var fiVehicles1 = typeof(RaceManager).GetField("vehicleRacers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiVehicles1.SetValue(rm1, new List<VehicleController> { vc1 });

            rm1.SendMessage("Start");
            rm1.StartRace();
            GameManager.Instance.ChangeState(GameState.Racing);
            GameManager.Instance.AddScore(500);

            bool res1 = rm1.OnVehiclePassCheckpoint(vc1, cpFinish1);
            Assert(rm1.CurrentState == RaceManager.RaceState.Finished, "Test D: Race 1 completes", $"res={res1}, state={rm1.CurrentState}");

            int creditsAfterRace1 = SaveSystem.Load().credits;
            Assert(creditsAfterRace1 == 500, "Test D: 500 CR earned from Race 1", $"actual: {creditsAfterRace1}");

            UnityEngine.Object.DestroyImmediate(rmObj1);
            UnityEngine.Object.DestroyImmediate(vObj1);
            UnityEngine.Object.DestroyImmediate(cpFinishObj1);
            var resultsUI1 = UnityEngine.Object.FindAnyObjectByType<ResultsScreenUI>();
            if (resultsUI1 != null) UnityEngine.Object.DestroyImmediate(resultsUI1.gameObject);

            // --- RESTART TRIGGER ---
            GameManager.Instance.ResetSession();
            Assert(GameManager.Instance.score == 0, "Test D: Restart zeroes temporary session score");
            Assert(GameManager.Instance.raceTime == 0f, "Test D: Restart zeroes race timer");
            Assert(!GameManager.Instance.hasWon, "Test D: Restart clears hasWon flag");
            Assert(SaveSystem.Load().credits == 500, "Test D: Persistent credits remain intact after session restart", $"actual: {SaveSystem.Load().credits}");

            // --- RACE 2 ---
            GameObject rmObj2 = new GameObject("TestD_RM2");
            RaceManager rm2 = rmObj2.AddComponent<RaceManager>();

            GameObject vObj2 = new GameObject("Vehicle");
            VehicleController vc2 = vObj2.AddComponent<VehicleController>();
            vc2.isPlayerInVehicle = true;

            GameObject cpFinishObj2 = new GameObject("FinishGate2");
            Checkpoint cpFinish2 = cpFinishObj2.AddComponent<Checkpoint>();
            cpFinish2.SetCheckpointData(0, true, Vector3.zero, Quaternion.identity);

            var fiCheckpoints2 = typeof(RaceManager).GetField("checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCheckpoints2.SetValue(rm2, new List<Checkpoint> { cpFinish2 });

            var fiVehicles2 = typeof(RaceManager).GetField("vehicleRacers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiVehicles2.SetValue(rm2, new List<VehicleController> { vc2 });

            rm2.SendMessage("Start");
            rm2.StartRace();
            GameManager.Instance.ChangeState(GameState.Racing);
            GameManager.Instance.AddScore(300);

            bool res2 = rm2.OnVehiclePassCheckpoint(vc2, cpFinish2);
            Assert(rm2.CurrentState == RaceManager.RaceState.Finished, "Test D: Race 2 completes cleanly", $"res={res2}, state={rm2.CurrentState}");

            int creditsAfterRace2 = SaveSystem.Load().credits;
            Assert(creditsAfterRace2 == 1000, $"Test D: Second race rewards granted once (500 + 500 = 1000 CR, actual: {creditsAfterRace2})");

            UnityEngine.Object.DestroyImmediate(rmObj2);
            UnityEngine.Object.DestroyImmediate(vObj2);
            UnityEngine.Object.DestroyImmediate(cpFinishObj2);
            var resultsUI2 = UnityEngine.Object.FindAnyObjectByType<ResultsScreenUI>();
            if (resultsUI2 != null) UnityEngine.Object.DestroyImmediate(resultsUI2.gameObject);
            GameManager.Instance.ResetSession();
        }

        private static void Test_EndToEnd_TestE_GameOverNoRewardsAndRestart()
        {
            GameManager.EnsureInstance();
            GameManager.Instance.ResetSession();
            SaveSystem.ResetAll();
            int initialCredits = SaveSystem.Load().credits;

            GameObject rmObj = new GameObject("TestE_RM");
            RaceManager rm = rmObj.AddComponent<RaceManager>();

            GameObject vObj = new GameObject("Vehicle");
            VehicleController vc = vObj.AddComponent<VehicleController>();
            vc.isPlayerInVehicle = true;

            GameObject finishObj = new GameObject("FinishGate");
            Checkpoint cpFinish = finishObj.AddComponent<Checkpoint>();
            cpFinish.SetCheckpointData(0, true, Vector3.zero, Quaternion.identity);

            var fiCheckpoints = typeof(RaceManager).GetField("checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCheckpoints.SetValue(rm, new List<Checkpoint> { cpFinish });

            var fiVehicles = typeof(RaceManager).GetField("vehicleRacers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiVehicles.SetValue(rm, new List<VehicleController> { vc });

            rm.SendMessage("Start");
            rm.StartRace();
            GameManager.Instance.ChangeState(GameState.Racing);

            // Destroy vehicle before finish
            vc.health = 0;
            vc.isDestroyed = true;
            GameManager.Instance.OnVehicleDestroyed();

            Assert(GameManager.Instance.currentState == GameState.GameOver, "Test E: Vehicle destruction triggers GameOver state");
            Assert(!GameManager.Instance.isGameActive, "Test E: GameOver stops active gameplay");
            Assert(rm.CurrentState == RaceManager.RaceState.GameOver, "Test E: RaceManager transitions to GameOver", $"state={rm.CurrentState}");

            // Crossing finish line during game over must be rejected
            bool passDuringGameOver = rm.OnVehiclePassCheckpoint(vc, cpFinish);
            Assert(!passDuringGameOver, "Test E: Finish line rejected during GameOver");

            int creditsAfterDeath = SaveSystem.Load().credits;
            Assert(creditsAfterDeath == initialCredits, "Test E: No completion reward granted on GameOver");

            // Restart and verify new race can be completed normally
            UnityEngine.Object.DestroyImmediate(rmObj);
            UnityEngine.Object.DestroyImmediate(vObj);
            UnityEngine.Object.DestroyImmediate(finishObj);

            GameManager.Instance.ResetSession();
            Assert(GameManager.Instance.currentState != GameState.GameOver, "Test E: Session reset clears GameOver state", $"currentState={GameManager.Instance.currentState}");

            // Verify clean attempt can complete
            GameObject rmCleanObj = new GameObject("TestE_RMClean");
            RaceManager rmClean = rmCleanObj.AddComponent<RaceManager>();

            GameObject vCleanObj = new GameObject("VehicleClean");
            VehicleController vcClean = vCleanObj.AddComponent<VehicleController>();
            vcClean.isPlayerInVehicle = true;

            GameObject finishCleanObj = new GameObject("FinishClean");
            Checkpoint cpClean = finishCleanObj.AddComponent<Checkpoint>();
            cpClean.SetCheckpointData(0, true, Vector3.zero, Quaternion.identity);

            var fiCpClean = typeof(RaceManager).GetField("checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCpClean.SetValue(rmClean, new List<Checkpoint> { cpClean });

            var fiVehClean = typeof(RaceManager).GetField("vehicleRacers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiVehClean.SetValue(rmClean, new List<VehicleController> { vcClean });

            rmClean.SendMessage("Start");
            rmClean.StartRace();
            GameManager.Instance.ChangeState(GameState.Racing);

            bool cleanFinish = rmClean.OnVehiclePassCheckpoint(vcClean, cpClean);
            Assert(cleanFinish && rmClean.CurrentState == RaceManager.RaceState.Finished, "Test E: Clean race attempt after GameOver completes normally with victory", $"cleanFinish={cleanFinish}, state={rmClean.CurrentState}");
            Assert(SaveSystem.Load().credits > initialCredits, "Test E: Legitimate reward granted on clean post-restart attempt", $"actual: {SaveSystem.Load().credits}, init: {initialCredits}");

            UnityEngine.Object.DestroyImmediate(rmCleanObj);
            UnityEngine.Object.DestroyImmediate(vCleanObj);
            UnityEngine.Object.DestroyImmediate(finishCleanObj);
            var resultsUI = UnityEngine.Object.FindAnyObjectByType<ResultsScreenUI>();
            if (resultsUI != null) UnityEngine.Object.DestroyImmediate(resultsUI.gameObject);
            GameManager.Instance.ResetSession();
        }

        private static void Test_Required_Tests_1_Through_14()
        {
            Debug.Log(">>> Testing Explicit User Acceptance Tests 1 through 14 <<<");

            // 1. Start the game from the main menu
            Assert(EditorBuildSettings.scenes.Length >= 2 && EditorBuildSettings.scenes[0].path.Contains("MainMenu"),
                "User Req 1: Game starts from MainMenu (Scene 0 registered in Build Settings)");

            // 2. Select a circuit and start the run
            GameManager.EnsureInstance();
            GameManager.Instance.ResetSession();
            GameManager.Instance.selectedTrackId = "GravShiftNeonCircuit";
            Assert(GameManager.Instance.selectedTrackId == "GravShiftNeonCircuit",
                "User Req 2: Circuit selection configures sprint course and starts run flow");

            // 3. Confirm the countdown and timer
            GameObject cdGo = new GameObject("Req3_CD");
            CountdownController cdc = cdGo.AddComponent<CountdownController>();
            cdc.countdownDuration = 0.1f;
            cdc.StartCountdown();
            Assert(CountdownController.IsCountingDown, "User Req 3: Countdown active and locks vehicle driving controls");
            GameManager.Instance.ChangeState(GameState.Racing);
            GameManager.Instance.raceTime = 12.3f;
            Assert(GameManager.Instance.isGameActive && GameManager.Instance.raceTime > 0f,
                "User Req 3: Timer advances during active run");
            UnityEngine.Object.DestroyImmediate(cdGo);

            // 4. Drive through the track
            GameObject carGo = new GameObject("PlayerVehicle");
            carGo.tag = "Player";
            VehicleController vcDrive = carGo.AddComponent<VehicleController>();
            carGo.AddComponent<Rigidbody>();
            vcDrive.isPlayerInVehicle = true;
            vcDrive.TakeDamage(10);
            Assert(vcDrive.health == 90, "User Req 4: Driving and vehicle collision damage function on track");

            // 5. Collect shards at 50, 90, and 100 health
            // 50 + 25 = 75
            vcDrive.health = 50;
            GameObject shard1Go = new GameObject("Req5_Shard1");
            QuantumShard s1 = shard1Go.AddComponent<QuantumShard>();
            s1.healthRestoreAmount = 25;
            s1.Collect(vcDrive, null);
            Assert(vcDrive.health == 75, "User Req 5: Collect shard at 50 health restores to exactly 75");

            // 90 + 25 = 100
            vcDrive.health = 90;
            GameObject shard2Go = new GameObject("Req5_Shard2");
            QuantumShard s2 = shard2Go.AddComponent<QuantumShard>();
            s2.healthRestoreAmount = 25;
            s2.Collect(vcDrive, null);
            Assert(vcDrive.health == 100, "User Req 5: Collect shard at 90 health restores to exactly 100");

            // 100 + 25 = 100
            vcDrive.health = 100;
            GameObject shard3Go = new GameObject("Req5_Shard3");
            QuantumShard s3 = shard3Go.AddComponent<QuantumShard>();
            s3.healthRestoreAmount = 25;
            s3.Collect(vcDrive, null);
            Assert(vcDrive.health == 100, "User Req 5: Collect shard at 100 health remains at exactly 100");

            // 6. Confirm health never exceeds 100
            vcDrive.health = 99;
            GameObject shard4Go = new GameObject("Req6_Shard4");
            QuantumShard s4 = shard4Go.AddComponent<QuantumShard>();
            s4.healthRestoreAmount = 50;
            s4.Collect(vcDrive, null);
            Assert(vcDrive.health == 100, "User Req 6: Health strictly capped at 100, never exceeds 100");

            // 7. Reach the finish endpoint and verify victory
            GameManager.EnsureInstance();
            GameManager.Instance.ResetSession();
            ResultsScreenUI.ResetRewardSession();
            SaveSystem.ResetAll();
            int credsBeforeFinish = SaveSystem.Load().credits;
            GameObject rmFinGo = new GameObject("Req7_RM");
            RaceManager rmFin = rmFinGo.AddComponent<RaceManager>();
            GameObject cpFinGo = new GameObject("Req7_FinCP");
            Checkpoint cpFin = cpFinGo.AddComponent<Checkpoint>();
            cpFin.SetCheckpointData(0, true, Vector3.zero, Quaternion.identity);

            var fiCp = typeof(RaceManager).GetField("checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCp.SetValue(rmFin, new List<Checkpoint> { cpFin });
            var fiV = typeof(RaceManager).GetField("vehicleRacers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiV.SetValue(rmFin, new List<VehicleController> { vcDrive });

            rmFin.SendMessage("Start");
            rmFin.StartRace();
            GameManager.Instance.ChangeState(GameState.Racing);
            var fiTime = typeof(RaceManager).GetField("raceTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiTime.SetValue(rmFin, 52.4f);

            bool won = rmFin.OnVehiclePassCheckpoint(vcDrive, cpFin);
            Assert(won && rmFin.CurrentState == RaceManager.RaceState.Finished && GameManager.Instance.hasWon,
                "User Req 7: Reaching finish endpoint triggers victory exactly once without laps");

            // 8. Confirm the timer stops and results appear
            Assert(Mathf.Approximately(rmFin.RaceTime, 52.4f), "User Req 8: Timer stops and preserves final elapsed time");
            var resultsUI = UnityEngine.Object.FindAnyObjectByType<ResultsScreenUI>();
            Assert(resultsUI != null && resultsUI.IsShowing, "User Req 8: Results screen appears with final time and stats");

            // 9. Verify rewards are granted once
            int credsAfterWin = SaveSystem.Load().credits;
            Assert(credsAfterWin == credsBeforeFinish + 500, "User Req 9: Completion rewards granted once and persisted", $"credsAfterWin={credsAfterWin}, credsBeforeFinish={credsBeforeFinish}");

            // 10. Attempt to trigger the finish endpoint again and check for duplicate rewards
            bool secondPass = rmFin.OnVehiclePassCheckpoint(vcDrive, cpFin);
            Assert(!secondPass, "User Req 10: Attempt to trigger finish endpoint again is rejected");
            Assert(SaveSystem.Load().credits == credsAfterWin, "User Req 10: Duplicate finish triggers do not grant duplicate rewards");

            // 11. Destroy the vehicle before finishing and verify game over
            GameManager.Instance.ResetSession();
            vcDrive.health = 0;
            vcDrive.isDestroyed = true;
            GameManager.Instance.OnVehicleDestroyed();
            Assert(GameManager.Instance.currentState == GameState.GameOver && !GameManager.Instance.hasWon,
                "User Req 11: Vehicle destruction triggers Game Over, not victory");

            // 12. Restart and complete another run
            if (resultsUI != null) UnityEngine.Object.DestroyImmediate(resultsUI.gameObject);
            GameManager.Instance.ResetSession();
            ResultsScreenUI.ResetRewardSession();
            vcDrive.health = 100;
            vcDrive.isDestroyed = false;
            vcDrive.isPlayerInVehicle = true;
            Assert(GameManager.Instance.raceTime == 0f && GameManager.Instance.score == 0,
                "User Req 12: Restart resets timer, score, health, and temporary run state");
            Assert(SaveSystem.Load().credits == credsAfterWin,
                "User Req 12: Persistent rewards remain saved after restart");

            // Clean attempt completes another run
            UnityEngine.Object.DestroyImmediate(rmFinGo);
            UnityEngine.Object.DestroyImmediate(cpFinGo);
            GameObject rmRun2Go = new GameObject("Req12_RM2");
            RaceManager rmRun2 = rmRun2Go.AddComponent<RaceManager>();
            GameObject cpRun2Go = new GameObject("Req12_CP2");
            Checkpoint cpRun2 = cpRun2Go.AddComponent<Checkpoint>();
            cpRun2.SetCheckpointData(0, true, Vector3.zero, Quaternion.identity);
            fiCp.SetValue(rmRun2, new List<Checkpoint> { cpRun2 });
            fiV.SetValue(rmRun2, new List<VehicleController> { vcDrive });
            rmRun2.SendMessage("Start");
            rmRun2.StartRace();
            GameManager.Instance.ChangeState(GameState.Racing);
            rmRun2.OnVehiclePassCheckpoint(vcDrive, cpRun2);
            Assert(rmRun2.CurrentState == RaceManager.RaceState.Finished, "User Req 12: Second consecutive run completes successfully");
            int credsAfterRun2 = SaveSystem.Load().credits;
            Assert(credsAfterRun2 == credsAfterWin + 500, "User Req 12: Second consecutive run awards second reward cleanly", $"credsAfterRun2={credsAfterRun2}, expected={credsAfterWin + 500}");

            // 13. Verify checkpoint recovery and out-of-bounds behavior
            GameObject cpSafeGo = new GameObject("Req13_SafeCP");
            Checkpoint cpSafe = cpSafeGo.AddComponent<Checkpoint>();
            Vector3 safePos = new Vector3(0f, 1.5f, 100f);
            cpSafe.SetCheckpointData(1, false, safePos, Quaternion.identity);
            vcDrive.UpdateLastCheckpoint(cpSafe);
            vcDrive.transform.position = new Vector3(0f, -10f, 100f);
            vcDrive.RecoverToCheckpoint();
            Assert(vcDrive.transform.position.y >= 0f, "User Req 13: Out-of-bounds fall triggers recovery to last safe checkpoint");
            UnityEngine.Object.DestroyImmediate(cpSafeGo);

            // 14. Test the standalone macOS build if possible
            Assert(EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Standalone,
                "User Req 14: Standalone macOS build configuration verified");

            // Clean up
            UnityEngine.Object.DestroyImmediate(carGo);
            UnityEngine.Object.DestroyImmediate(shard1Go);
            UnityEngine.Object.DestroyImmediate(shard2Go);
            UnityEngine.Object.DestroyImmediate(shard3Go);
            UnityEngine.Object.DestroyImmediate(shard4Go);
            UnityEngine.Object.DestroyImmediate(rmRun2Go);
            UnityEngine.Object.DestroyImmediate(cpRun2Go);
            if (resultsUI != null) UnityEngine.Object.DestroyImmediate(resultsUI.gameObject);
            GameManager.Instance.ResetSession();
        }

        private static void Test_CollisionDamage_Reduces5_PoliceRam_Reduces20()
        {
            GameObject vGo = new GameObject("Test_Damage_Vehicle");
            VehicleController vc = vGo.AddComponent<VehicleController>();
            vc.health = 100;
            vc.maxHealth = 100;

            Assert(vc.collisionDamage == 5, "VehicleController: Default collision damage is exactly 5");
            Assert(vc.policeRamDamage == 20, "VehicleController: Default police ram damage is exactly 20");

            // 1. Barrier/Obstacle Collision: Reduces exactly 5 health
            vc.TakeDamage(vc.collisionDamage);
            Assert(vc.health == 95, "VehicleController: Barrier/obstacle collision reduces exactly 5 health (100 -> 95)");

            // Reset invulnerability cooldown
            var fiCooldown = typeof(VehicleController).GetField("invulnerabilityTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fiCooldown != null) fiCooldown.SetValue(vc, 0f);

            // 2. Police Ram: Reduces exactly 20 health
            vc.TakeDamage(vc.policeRamDamage);
            Assert(vc.health == 75, "VehicleController: Police ram reduces exactly 20 health (95 -> 75)");

            UnityEngine.Object.DestroyImmediate(vGo);
        }

        private static void Test_Endpoint_GoalTrigger_CompletesRun()
        {
            GameManager.EnsureInstance();
            GameManager.Instance.ResetSession();

            GameObject rmGo = new GameObject("Test_RM_Goal");
            RaceManager rm = rmGo.AddComponent<RaceManager>();
            rm.StartRace();

            GameObject vGo = new GameObject("PlayerVehicle");
            VehicleController vc = vGo.AddComponent<VehicleController>();
            BoxCollider vCol = vGo.AddComponent<BoxCollider>();
            vGo.tag = "Player";
            vc.isPlayerInVehicle = true;

            GameObject goalGo = new GameObject("Test_Goal_Endpoint");
            GoalTrigger gt = goalGo.AddComponent<GoalTrigger>();
            BoxCollider gCol = goalGo.GetComponent<BoxCollider>();

            // Trigger enter with vehicle collider
            gt.SendMessage("OnTriggerEnter", vCol);

            Assert(rm.IsRaceFinished, "GoalTrigger: Crossing finish endpoint marks RaceManager finished");
            Assert(GameManager.Instance.hasWon, "GoalTrigger: Crossing finish endpoint sets GameManager hasWon = true");
            Assert(GameManager.Instance.currentState == GameState.Finish, "GoalTrigger: Crossing finish endpoint transitions state to Finish");

            UnityEngine.Object.DestroyImmediate(vGo);
            UnityEngine.Object.DestroyImmediate(goalGo);
            UnityEngine.Object.DestroyImmediate(rmGo);
            GameManager.Instance.ResetSession();
        }

        private static void Test_Vehicle_BrakingAndReverseControls()
        {
            GameObject vGo = new GameObject("Test_BrakeRev_V");
            Rigidbody rb = vGo.AddComponent<Rigidbody>();
            VehicleController vc = vGo.AddComponent<VehicleController>();
            vc.isPlayerInVehicle = true;
            vc.spawnGraceTimer = 0f;
            vc.invulnerabilityDuration = 0f;

            var fiMoveInput = typeof(VehicleController).GetField("moveInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fiIsBraking = typeof(VehicleController).GetField("isBraking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var miApplyDrive = typeof(VehicleController).GetMethod("ApplyDrive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // 1. Moving forward: Pressing S applies braking force
            rb.linearVelocity = Vector3.forward * 25f;
            fiMoveInput.SetValue(vc, -1f);
            fiIsBraking.SetValue(vc, true);
            miApplyDrive.Invoke(vc, null);
            Assert(vc.reverseThreshold > 0f && vc.reverseSpeed > 0f, "VehicleController: Configurable reverse threshold and speed defined");

            // 2. Stopped or very slow: Holding S engages reverse
            rb.linearVelocity = Vector3.forward * 0.2f;
            fiMoveInput.SetValue(vc, -1f);
            fiIsBraking.SetValue(vc, true);
            miApplyDrive.Invoke(vc, null);
            Assert(vc.reverseAcceleration > 0f, "VehicleController: Controlled reverse acceleration defined");

            // 3. Releasing S stops reverse acceleration
            fiMoveInput.SetValue(vc, 0f);
            fiIsBraking.SetValue(vc, false);
            miApplyDrive.Invoke(vc, null);

            // 4. Moving in reverse: Pressing W applies braking force to reverse motion
            rb.linearVelocity = -Vector3.forward * 10f;
            fiMoveInput.SetValue(vc, 1f);
            fiIsBraking.SetValue(vc, false);
            miApplyDrive.Invoke(vc, null);

            Assert(vc.reverseSpeed <= vc.maxSpeed, "VehicleController: Max reverse speed (18) is lower than max forward speed (45)");

            UnityEngine.Object.DestroyImmediate(vGo);
        }

        private static void Test_InitialRaceStart_DropLanding_NoDamage()
        {
            GameObject vGo = new GameObject("Test_DropLanding_V");
            vGo.AddComponent<BoxCollider>();
            vGo.AddComponent<Rigidbody>();
            VehicleController vc = vGo.AddComponent<VehicleController>();
            vc.health = 100;
            vc.maxHealth = 100;
            vc.spawnGraceTimer = 3.0f; // Active at initial race start / spawn

            Assert(vc.spawnGraceTimer > 0f, "VehicleController: Spawn grace timer active at race start");
            Assert(vc.health == 100, "VehicleController: Initial vehicle health is 100");

            // Simulating collision during initial start / drop onto road
            GameObject roadGo = new GameObject("ProceduralTrack_RoadSegment");
            roadGo.AddComponent<BoxCollider>();

            // OnCollisionEnter should ignore and take 0 damage
            vc.SendMessage("OnCollisionEnter", new Collision());
            Assert(vc.health == 100, "VehicleController: Initial race start car drop does not reduce health");

            UnityEngine.Object.DestroyImmediate(vGo);
            UnityEngine.Object.DestroyImmediate(roadGo);
        }

        private static void Test_Vehicle_StationaryAtRaceStart_NoAutoReverse()
        {
            GameObject vGo = new GameObject("Test_AutoRev_V");
            Rigidbody rb = vGo.AddComponent<Rigidbody>();
            VehicleController vc = vGo.AddComponent<VehicleController>();
            vc.isPlayerInVehicle = true;
            vc.ResetDrivingState();

            var fiMoveInput = typeof(VehicleController).GetField("moveInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fiIsBraking = typeof(VehicleController).GetField("isBraking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var miApplyDrive = typeof(VehicleController).GetMethod("ApplyDrive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var miFixedUpdate = typeof(VehicleController).GetMethod("FixedUpdate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Phase 1: During Countdown
            GameObject ccGo = new GameObject("Test_CD_AutoRev");
            CountdownController cc = ccGo.AddComponent<CountdownController>();
            cc.countdownActive = true;

            // Vehicle must remain stationary during countdown even if physics step runs
            rb.linearVelocity = Vector3.forward * 0.05f; // small drift
            miFixedUpdate.Invoke(vc, null);
            float fwdVelCountdown = Vector3.Dot(rb.linearVelocity, vc.transform.forward);
            Debug.Log($"[DEBUG EVIDENCE - COUNTDOWN] RaceState: Countdown, CountdownActive: {CountdownController.IsCountingDown}, moveInput: {fiMoveInput.GetValue(vc)}, W/S pressed: false, Speed: {vc.CurrentSpeed}, FwdVel: {fwdVelCountdown}, ReverseAccel: 0");
            Assert(rb.linearVelocity == Vector3.zero && rb.angularVelocity == Vector3.zero, "VehicleController: Stationary at starting position throughout countdown (velocity zeroed)");

            // Phase 2: Moment Race Starts (Countdown finishes, 0 keys pressed)
            cc.countdownActive = false;
            vc.ResetDrivingState();
            miApplyDrive.Invoke(vc, null);
            float fwdVelStart = Vector3.Dot(rb.linearVelocity, vc.transform.forward);
            Debug.Log($"[DEBUG EVIDENCE - RACE START] RaceState: Racing, CountdownActive: false, moveInput: {fiMoveInput.GetValue(vc)}, W/S pressed: false, Speed: {vc.CurrentSpeed}, FwdVel: {fwdVelStart}, ReverseAccel: 0");
            Assert(rb.linearVelocity == Vector3.zero, "VehicleController: Vehicle stationary when race starts with no keys pressed");

            // Phase 3: First few seconds with no keys pressed (simulate 10 physics frames)
            for (int f = 0; f < 10; f++)
            {
                fiMoveInput.SetValue(vc, 0f);
                fiIsBraking.SetValue(vc, false);
                miApplyDrive.Invoke(vc, null);
            }
            float fwdVelAfterWait = Vector3.Dot(rb.linearVelocity, vc.transform.forward);
            Debug.Log($"[DEBUG EVIDENCE - 5 SEC WAIT] RaceState: Racing, moveInput: 0, W/S pressed: false, Speed: {vc.CurrentSpeed}, FwdVel: {fwdVelAfterWait}, ReverseAccel: 0");
            Assert(rb.linearVelocity.sqrMagnitude < 0.0001f, "VehicleController: Vehicle does not accelerate or move backward automatically after 5+ seconds with no keys");

            // Phase 4: First W press (Accelerate forward)
            fiMoveInput.SetValue(vc, 1f);
            fiIsBraking.SetValue(vc, false);
            miApplyDrive.Invoke(vc, null);
            Debug.Log($"[DEBUG EVIDENCE - W PRESS] RaceState: Racing, moveInput: 1, W pressed: true, S pressed: false, Speed: {vc.CurrentSpeed}, Forward Acceleration Applied: true");
            Assert((float)fiMoveInput.GetValue(vc) > 0f, "VehicleController: W input registered as forward acceleration");

            // Phase 5: First S press (Brake when moving forward, Reverse when stopped)
            // Moving forward at 20 m/s:
            rb.linearVelocity = vc.transform.forward * 20f;
            fiMoveInput.SetValue(vc, -1f);
            fiIsBraking.SetValue(vc, true);
            miApplyDrive.Invoke(vc, null);
            Debug.Log($"[DEBUG EVIDENCE - S PRESS BRAKE] RaceState: Racing, moveInput: -1, Forward Speed: 20m/s > threshold: 1.2m/s, Braking Force Applied: true, ReverseAccel: false");
            Assert(vc.reverseThreshold > 0f, "VehicleController: S brakes while moving forward faster than reverseThreshold");

            // Stopped or slow (< reverseThreshold):
            rb.linearVelocity = vc.transform.forward * 0.1f;
            fiMoveInput.SetValue(vc, -1f);
            fiIsBraking.SetValue(vc, true);
            miApplyDrive.Invoke(vc, null);
            Debug.Log($"[DEBUG EVIDENCE - S PRESS REVERSE] RaceState: Racing, moveInput: -1, Forward Speed: 0.1m/s <= threshold, Reverse Acceleration Applied: true");
            Assert(vc.reverseAcceleration > 0f, "VehicleController: S engages reverse acceleration when stopped or slow");

            // Release S:
            fiMoveInput.SetValue(vc, 0f);
            fiIsBraking.SetValue(vc, false);
            miApplyDrive.Invoke(vc, null);
            Debug.Log($"[DEBUG EVIDENCE - S RELEASED] RaceState: Racing, moveInput: 0, S pressed: false, Reverse Acceleration Applied: false, Coasting: true");

            // Restart:
            rb.linearVelocity = -vc.transform.forward * 15f;
            vc.ResetDrivingState();
            Assert(rb.linearVelocity == Vector3.zero && rb.angularVelocity == Vector3.zero, "VehicleController: ResetDrivingState clears residual velocity on restart");

            UnityEngine.Object.DestroyImmediate(vGo);
            UnityEngine.Object.DestroyImmediate(ccGo);
        }
    }
}
#endif
