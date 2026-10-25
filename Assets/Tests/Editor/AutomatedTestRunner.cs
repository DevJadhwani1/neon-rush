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
    }
}
#endif
