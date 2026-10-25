# TEST_PLAN.md — End-to-End Test Plan & Acceptance Matrix

## Test Suite Scope
The end-to-end test plan covers the complete player journey: clean launch, main menu, vehicle selection & upgrades, race initialization, vehicle controls & gameplay, track progression, race finish, persistence, automated test suite execution, and standalone build verification.

---

### Scenario Overview
| Test ID | Scenario | Preconditions | Target Systems | Success Criteria |
|---|---|---|---|---|
| **TEST-01** | Clean Project Initialization | Clean checkout of repository | Package Manager, Compiler, Assets | 0 compiler errors, 0 compiler warnings, valid packages resolved. |
| **TEST-02** | Initial Launch | Unity opened, Play Mode started | Core / Scene Management / Cameras | Initial scene loads without null exceptions; camera renders viewport. |
| **TEST-03** | Main Menu & Navigation | MainMenu scene loaded | `MainMenuUI`, `SceneLoader` | PLAY, GARAGE, QUIT buttons responsive; scene transitions cleanly without loop. |
| **TEST-04** | Garage & Vehicle Selection | Garage UI active | `GarageUI`, `SaveSystem`, `UpgradeSystem` | Car cards cycle (`VOLT`, `BRICK`, `BLADE`), upgrades scale and deduct credits, selection persists. |
| **TEST-05** | Race Initialization | `GravShiftNeonCircuit.unity` loaded | `CountdownController`, `RaceManager` | Track, cyber vehicle, AI opponents, and HUD spawn; 3-2-1-GO countdown freezes vehicle until GO. |
| **TEST-06** | Driving & Core Controls | Race active (post-GO) | `VehicleController`, `GameInput`, HUD | Acceleration (W), steering (A/D), brake (S), drift (LeftShift), and anti-gravity (Space) respond smoothly. |
| **TEST-07** | Track Progression & Checkpoints | Driving through track | `Checkpoint`, `RaceManager`, HUD | Passing checkpoints advances next checkpoint index; skipping is rejected; finish line counts laps. |
| **TEST-08** | Race Completion & Results | Complete final lap | `RaceManager`, `GameManager`, `ResultsScreenUI` | Race transitions to Finish; victory HUD displays position, time, and credits exactly once. |
| **TEST-09** | Restart & Persistence | Post-race screen active | `ResultsScreenUI`, `SaveSystem` | Pressing 'R' or 'RACE AGAIN' resets race state; earned credits persist across restarts. |
| **TEST-10** | Standalone Development Build | StandaloneOSX build pipeline | Build Pipeline, Player | Development build creates `NeonRush.app` successfully without build errors. |

