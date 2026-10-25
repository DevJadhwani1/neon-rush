# NEON RUSH: Grid Runners

> **Cyberpunk Anti-Gravity Arcade Racing Game**  
> Developed in **Unity 6 LTS (6000.6.0f1)** for macOS Standalone / Universal.

---

## 1. Quick Start & Setup Instructions

### Prerequisites
- **Unity Version:** Unity `6000.6.0f1` (or compatible Unity 6 LTS release)
- **Render Pipeline:** Universal Render Pipeline (URP 17.x)
- **Platform:** macOS Standalone (Apple Silicon / Intel) or Windows Standalone

### Project Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/DevJadhwani1/neon-rush.git
   cd neon-rush
   ```
2. Open the project in Unity Hub using Unity `6000.6.0f1`.
3. In the Editor, open **`Assets/Scenes/MainMenu.unity`** (Build Index 0) or **`Assets/Scenes/GravShiftNeonCircuit.unity`** (Build Index 1).
4. Press **Play** in the Unity Editor to start racing.

---

## 2. Controls & Gameplay

| Action | Primary Key | Secondary Key | Description |
|---|---|---|---|
| **Accelerate** | `W` | `Up Arrow` | Drive forward (55 m/s² acceleration) |
| **Steer** | `A` / `D` | `Left` / `Right Arrow` | Responsive cornering (140°/s) |
| **Brake / Reverse** | `S` | `Down Arrow` | Powerful braking (70 N force) |
| **Drift Mode** | `Left Shift` | `Right Shift` | Charge drift nitro meter on turns |
| **Anti-Gravity** | `Spacebar` | — | Activate hover mode (3.5s duration) |
| **Restart Race** | `R` | — | Restart the current race circuit |

---

## 3. How to Run Automated Tests

The project includes an automated test suite verifying state machines, race progression, checkpoint sequencing, DNF timeouts, upgrade mathematics, and save persistence.

### Option A: Via Unity Editor Menu
1. In the top menu bar, select:
   **`AntiGravity` -> `Run All Automated Tests`** (or `Tools` -> `Neon Rush` -> `Run All Automated Tests`).
2. Open the Unity Console window (`Window` -> `General` -> `Console`) to view the detailed results.

### Option B: Via Command Line (Batchmode)
Run the automated test runner headlessly from terminal:
```bash
/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit \
  -projectPath "$(pwd)" \
  -executeMethod NeonRush.Tests.AutomatedTestRunner.RunAllTests \
  -logFile automated_tests.log
```
Check `automated_tests.log` for the summary (`>>> SUITE COMPLETED: 33 PASSED, 0 FAILED <<<`).

---

## 4. Standalone Build Instructions

To produce a development standalone build for macOS:
```bash
/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit \
  -projectPath "$(pwd)" \
  -executeMethod NeonRush.EditorTools.BuildAutomation.BuildStandaloneOSX \
  -logFile build_pipeline.log
```
The output executable will be generated at `Builds/OSX/NeonRush.app`.

---

## 5. Project Documentation Map
- **[AUDIT_BASELINE.md](file:///Users/dev/GAME/AUDIT_BASELINE.md):** Complete pre-fix audit report and diagnostic findings.
- **[BUGS_AND_FIXES.md](file:///Users/dev/GAME/BUGS_AND_FIXES.md):** Complete issue register with root causes and verified fixes.
- **[TEST_PLAN.md](file:///Users/dev/GAME/TEST_PLAN.md):** End-to-end 10-step testing scenarios and acceptance criteria.
- **[TEST_RESULTS.md](file:///Users/dev/GAME/TEST_RESULTS.md):** Actual execution evidence and automated test logs.
- **[TEAM_CONTRIBUTIONS.md](file:///Users/dev/GAME/TEAM_CONTRIBUTIONS.md):** Three-person collaboration matrix and evidence-backed task log.
- **[ARCHITECTURE.md](file:///Users/dev/GAME/ARCHITECTURE.md):** Complete system architecture and class relationships.

---

## 6. Known Limitations
- When opening the project in Unity Editor for the very first time on a new machine, allow 1–2 minutes for the initial URP shader and texture cache compilation.
