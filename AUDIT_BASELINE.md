# AUDIT_BASELINE.md — Neon Rush Baseline Audit Report

## 1. Executive Summary & Project Identification
- **Project Name:** NEON RUSH: Grid Runners
- **Repository:** https://github.com/DevJadhwani1/neon-rush
- **Target Platform:** macOS Standalone / Apple Silicon & Intel
- **Unity Version Required:** `6000.6.0f1` (Unity 6.0 LTS Respin, Darwin arm64)
- **Baseline Audit Date:** 2026-10-25
- **Baseline Status:** Broken on checkout. The repository suffered from multiple severe C# compilation errors, archived essential camera and enemy assets, missing initial scenes in Build Settings, infinite UI reload loops, unhandled vehicle model divergence, and non-portable package manifest paths.

---

## 2. Environment & Dependency Analysis
| Component | Baseline Finding | Impact / Severity |
|---|---|---|
| **Unity Version** | Installed editor is `6000.6.0f1` located at `/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app`. Documentation reported `6000.0.6f1`. | Medium |
| **Packages Manifest** | `Packages/manifest.json` referenced `"org.christowm.unity.mcp": "file:/Users/dev/Documents/unity-mcp/Packages/org.christowm.unity.mcp"` (absolute machine-specific path). | Critical (breaks portability on clean clones) |
| **Build Settings** | `ProjectSettings/EditorBuildSettings.asset` contained an empty scene array (`m_Scenes: []`). | Critical (`SceneLoader.LoadRace()` index 1 threw runtime scene load exceptions) |
| **Missing Scenes** | `Assets/Scenes/MainMenu.unity` was missing; a corrupted typo scene `Assets/,.unity` was checked in. | Critical |

---

## 3. Baseline Compilation Errors & Diagnostics
Before repair, compiling `Assembly-CSharp.csproj` produced 22+ fatal C# compiler errors:

| Error Code | File & Line | Message Summary | Root Cause |
|---|---|---|---|
| **CS0103** | `GoalTrigger.cs:27`, `BoostPad.cs:31`, `PlayerController.cs:58`, `PlayerHealth.cs:33`, `VehicleController.cs:321`, `QuantumShard.cs:72`, `SpeedRing.cs:23`, `VehicleInteraction.cs:91`, `EnergyPickup.cs:52` | `The name 'GameManager' does not exist in the current context` | `GameManager.cs` was encapsulated under `namespace NeonRush`, whereas game scripts resided in the global namespace and omitted `using NeonRush;`. Additionally, `GameManager` lacked course completion and shard scoring methods. |
| **CS0246** | `CameraSetup.cs:20`, `VehicleInteraction.cs:22` | `The type or namespace name 'SimpleCameraFollow' could not be found` | `SimpleCameraFollow.cs` was archived into `_Archive/Scripts/` during a refactor pass, breaking references and scene GUID `0eb599eb6fee54528a5d250816472080`. |
| **CS0246** | `PowerUpSystem.cs:52` | `The type or namespace name 'EnemyController' could not be found` | `EnemyController.cs` was archived into `_Archive/Scripts/`, breaking the EMP combat system and `Enemy.prefab`. |
| **CS0117** | `VehicleController.cs:72, 85, 96` | `'GameInput' does not contain a definition for 'GetBrake', 'GetDrift', 'GetAntiGravityDown'` | `GameInput.cs` lacked mappings for braking, drifting, and anti-gravity activation. |
| **CS0122** | `CountdownController.cs:83` | `'RaceManager.StartRace()' is inaccessible due to its protection level` | `StartRace()` in `RaceManager.cs` was declared `private`. |
| **CS0122** | `ResultsScreenUI.cs:21` | `'RaceManager.raceTime' is inaccessible due to its protection level` | `ResultsScreenUI.cs` attempted to access `private float raceTime` instead of public property `RaceTime`. |
| **CS1061** | `Editor/CameraSetup.cs:52, 60` | Cinemachine 3.x API changes (`PositionDamping`, `RotationComposer.Damping`) | Cinemachine 3.1.2 changed Tracker and Composer data models. |
| **CS1061** | `Editor/Track1Builder.cs:87, 88` | `autoPlaceCheckpoints` not found on `TrackBuilder` | Property name mismatch between editor tool and runtime builder. |
| **CS0618** | `MusicManager.cs`, `TestMenu.cs`, `PlaytestValidator.cs` | `FindObjectsSortMode` deprecation warnings in Unity 6 | Obsolete Unity 6 API usage. |

---

## 4. Gameplay & Lifecycle Defects
1. **Infinite Scene Reload in MainMenuUI:**
   `MainMenuUI.cs` had commented-out GUI button conditionals, resulting in `SceneLoader.LoadMainMenu()` executing on every frame of `OnGUI()`.
2. **Infinite Scene Reload in ResultsScreenUI:**
   `ResultsScreenUI.cs` had commented-out button conditionals executing `SceneLoader.LoadMainMenu()` every `OnGUI()` frame.
3. **Vehicle vs Ship Divergence:**
   `GravShiftNeonCircuit.unity` featured `VehicleController` with anti-gravity hover, drift, boost pads, pickups, hazards, and upgrades. `RaceManager.cs` only tracked `ShipController` racers, preventing checkpoint passing, lap progression, and race finish for the active vehicle.
4. **Progression Math Discrepancy:**
   `UpgradeSystem.cs` defined +15% Speed, +10% Handling, +0.5s Boost, and +20 Armor, while `VehicleStatsApplier.cs` applied a hardcoded 8% flat multiplier.
5. **Reward Duplication:**
   `ResultsScreenUI.cs` awarded credits in `Start()` on every enable without session idempotency checks.
6. **AI DNF Race Blocker:**
   `RaceManager.UpdateRace()` waited for all racers to finish indefinitely (`racerDataMap.Values.All(data => data.finished)`), freezing race completion if an AI collided or became stuck.

