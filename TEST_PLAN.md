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
| **TEST-11** | Studio Splash Sequence | Application startup (Scene 0) | `MainMenuUI` | Displays "HYPERION INTERACTIVE / VIRTUAL RACING ARCHITECTURE" with cyan/magenta accents; smooth fade-in and fade-out; skippable after 0.6s buffer. |
| **TEST-12** | Cinematic Title Screen & Safeguarded Input | Post-splash state | `MainMenuUI` | Displays "NEON RUSH — GRID RUNNERS" with pulsing "PRESS ANY KEY OR CLICK TO ENTER" prompt; rejects same-frame skip propagation (<0.4s buffer); transitions to Main Menu. |
| **TEST-13** | Real Asynchronous Loading Screen | Race launch or menu exit | `SceneLoader` | Branded loading overlay with neon cyan progress bar, dynamic percentage, status stream ("SYNCHRONIZING TELEMETRY STREAM...", etc.), and gameplay tips carousel; normalizes 0.0-0.9 to 0.0-1.0; prevents duplicate loads. |
| **TEST-14** | High-Visibility Animated Countdown | Race start | `CountdownController` | Scaled bouncing numbers (96-114pt) with dual-layer drop shadows for track contrast; distinct emerald green GO! indicator; audio beep triggers; unfreezes player upon GO. |
| **TEST-15** | Pause Menu TimeScale & Loading Guards | Race active & loading | `PauseMenuUI`, `SceneLoader` | Pausing is prevented during active scene loading; toggling pause updates Time.timeScale to 0f/1f; resume, restart, and OnDestroy guarantee Time.timeScale is restored to 1.0f. |
| **TEST-16** | PlayMode Start Scene Enforcement | Unity Editor Play Mode | `PlayModeStartSceneEnforcer` | Entering Play Mode from any open scene (including `GravShiftNeonCircuit.unity`) automatically initiates from `MainMenu.unity`. |
| **TEST-17** | Auto-Bootstrapped Race Systems | Scene loading / Race init | `GameManager` | Automatically instantiates missing `CountdownController`, `PauseMenuUI`, `ResultsScreenUI`, and `RaceManager` if absent in scene. |
| **TEST-18** | Permanent Race Controls Legend | Loading overlay active | `SceneLoader` | Renders dedicated, always-visible Race Controls card with visual keycaps for Drive, Steer, Brake, Drift, Anti-Grav, Pause, and Restart. |
| **TEST-19** | Standardized Input Mappings | Vehicle gameplay | `GameInput`, `VehicleController` | Drift maps to Left Shift/Right Shift/Control; Anti-Gravity maps to Space/F; Brake maps to S/Down Arrow; Restart maps to R. |
| **TEST-20** | In-Race Controls Reference & Quick Restart | In-race pause & race loop | `PauseMenuUI`, `GameManager` | Pause menu displays controls reference card; pressing 'R' triggers quick race reload via `SceneLoader`. |

