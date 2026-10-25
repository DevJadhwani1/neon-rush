# TEST_PLAN.md — End-to-End Test Plan & Acceptance Matrix

## Test Suite Scope
The end-to-end test plan covers the complete player journey: clean launch, main menu, vehicle selection & upgrades, race initialization, vehicle controls & gameplay, track progression, race finish, persistence, automated test suite execution, checkpoint validation, fall recovery, and standalone build verification.

---

### Acceptance Matrix
| Test ID | Scenario | Preconditions | Target Systems | Success Criteria | Result |
|---|---|---|---|---|---|
| **TEST-01** | Clean Project Initialization | Clean checkout of repository | Package Manager, Compiler, Assets | 0 compiler errors, 0 compiler warnings, valid packages resolved. | **PASS** |
| **TEST-02** | Initial Launch | Unity opened, Play Mode started | Core / Scene Management / Cameras | Initial scene loads without null exceptions; camera renders viewport. | **PASS** |
| **TEST-03** | Main Menu & Navigation | MainMenu scene loaded | `MainMenuUI`, `SceneLoader` | PLAY, GARAGE, QUIT buttons responsive; scene transitions cleanly without loop. | **PASS** |
| **TEST-04** | Garage & Vehicle Selection | Garage UI active | `GarageUI`, `SaveSystem`, `UpgradeSystem` | Car cards cycle (`VOLT`, `BRICK`, `BLADE`), upgrades scale and deduct credits, selection persists. | **PASS** |
| **TEST-05** | Race Initialization | `GravShiftNeonCircuit.unity` loaded | `CountdownController`, `RaceManager` | Track, cyber vehicle, AI opponents, and HUD spawn; 3-2-1-GO countdown freezes vehicle until GO. | **PASS** |
| **TEST-06** | Driving & Core Controls | Race active (post-GO) | `VehicleController`, `GameInput`, HUD | Acceleration (W), steering (A/D), brake (S), drift (LeftShift), and anti-gravity (Space) respond smoothly. | **PASS** |
| **TEST-07** | Track Progression & Checkpoints | Driving through track | `Checkpoint`, `RaceManager`, HUD | Passing checkpoints advances next checkpoint index; skipping is rejected; finish line counts laps. | **PASS** |
| **TEST-08** | Race Completion & Results | Complete final lap | `RaceManager`, `GameManager`, `ResultsScreenUI` | Race transitions to Finish; victory HUD displays position, time, and credits exactly once. | **PASS** |
| **TEST-09** | Restart & Persistence | Post-race screen active | `ResultsScreenUI`, `SaveSystem` | Pressing 'R' or 'RACE AGAIN' resets race state; earned credits persist across restarts. | **PASS** |
| **TEST-10** | Standalone Development Build | StandaloneOSX build pipeline | Build Pipeline, Player | Development build creates `NeonRush.app` successfully without build errors. | **PASS** |
| **TEST-11** | Studio Splash Sequence | Application startup (Scene 0) | `MainMenuUI` | Displays "HYPERION INTERACTIVE / VIRTUAL RACING ARCHITECTURE" with cyan/magenta accents; smooth fade-in and fade-out; skippable after 0.6s buffer. | **PASS** |
| **TEST-12** | Cinematic Title Screen & Safeguarded Input | Post-splash state | `MainMenuUI` | Displays "NEON RUSH — GRID RUNNERS" with pulsing "PRESS ANY KEY OR CLICK TO ENTER" prompt; rejects same-frame skip propagation (<0.4s buffer); transitions to Main Menu. | **PASS** |
| **TEST-13** | Real Asynchronous Loading Screen | Race launch or menu exit | `SceneLoader` | Branded loading overlay with neon cyan progress bar, dynamic percentage, status stream, and gameplay tips carousel; normalizes 0.0-0.9 to 0.0-1.0; prevents duplicate loads. | **PASS** |
| **TEST-14** | High-Visibility Animated Countdown | Race start | `CountdownController` | Scaled bouncing numbers (96-114pt) with dual-layer drop shadows for track contrast; distinct emerald green GO! indicator; audio beep triggers; unfreezes player upon GO. | **PASS** |
| **TEST-15** | Pause Menu TimeScale & Loading Guards | Race active & loading | `PauseMenuUI`, `SceneLoader` | Pausing is prevented during active scene loading; toggling pause updates Time.timeScale to 0f/1f; resume, restart, and OnDestroy guarantee Time.timeScale is restored to 1.0f. | **PASS** |
| **TEST-16** | PlayMode Start Scene Enforcement | Unity Editor Play Mode | `PlayModeStartSceneEnforcer` | Entering Play Mode from any open scene (including `GravShiftNeonCircuit.unity`) automatically initiates from `MainMenu.unity`. | **PASS** |
| **TEST-17** | Auto-Bootstrapped Race Systems | Scene loading / Race init | `GameManager` | Automatically instantiates missing `CountdownController`, `PauseMenuUI`, `ResultsScreenUI`, and `RaceManager` if absent in scene. | **PASS** |
| **TEST-18** | Permanent Race Controls Legend | Loading overlay active | `SceneLoader` | Renders dedicated, always-visible Race Controls card with visual keycaps for Drive, Steer, Brake, Drift, Anti-Grav, Pause, and Restart. | **PASS** |
| **TEST-19** | Standardized Input Mappings | Vehicle gameplay | `GameInput`, `VehicleController` | Drift maps to Left Shift/Right Shift/Control; Anti-Gravity maps to Space/F; Brake maps to S/Down Arrow; Restart maps to R. | **PASS** |
| **TEST-20** | In-Race Controls Reference & Quick Restart | In-race pause & race loop | `PauseMenuUI`, `GameManager` | Pause menu displays controls reference card; pressing 'R' triggers quick race reload via `SceneLoader`. | **PASS** |
| **TEST-21** | Health / Hull Integrity HUD | In-race active gameplay | `GameUIManager` | Renders dedicated neon health bar with HP readout (e.g., "100 / 100 HP") and status ("OPTIMAL", "WARNING", "CRITICAL"). | **PASS** |
| **TEST-22** | Vehicle & Player Collision Damage | Impact with hazard or interceptor | `VehicleController`, `PlayerHealth` | Vehicle takes 25 damage per laser/sweeper collision; health decreases accurately on HUD. | **PASS** |
| **TEST-23** | Hazard Invulnerability Cooldown | Multi-collider hazard contact | `VehicleController`, `PlayerHealth` | Applies 1.0s invulnerability window preventing multiple damage triggers in the same frame or second. | **PASS** |
| **TEST-24** | Visible Hit & Damage Feedback | Hazard contact | `GameUIManager`, `SoundManager` | Triggers red screen vignette/flash and plays hazard sound effect when damage occurs. | **PASS** |
| **TEST-25** | Health Restoration & Cap | Repair / health pickup | `VehicleController`, `PlayerHealth` | `RestoreHealth(amount)` increases health but strictly caps at `maxHealth` (100 HP). | **PASS** |
| **TEST-26** | Quantum Shards & Score Accumulation | Collecting floating shards | `QuantumShard`, `GameManager` | Increments shard collection count (+1) and score (+50), updating HUD telemetry cleanly. | **PASS** |
| **TEST-27** | Vehicle Entry & Mesh Concealment | Player enters vehicle [E] | `VehicleInteraction` | Conceals all player child renderers and disables colliders so limbs never protrude outside the vehicle chassis. | **PASS** |
| **TEST-28** | Vehicle Exit & Mesh Restoration | Player exits vehicle [E] | `VehicleInteraction` | Fully restores original player renderer visibility, collider states, and places player safely on drivable ground. | **PASS** |
| **TEST-29** | Player / Vehicle Death & Game Over | Health reaches 0 | `GameManager`, `GameUIManager` | Transitions to `GameState.GameOver`, displays Game Over banner, halts motion, unlocks cursor. | **PASS** |
| **TEST-30** | Game Over Screen Quick Restart | Game Over banner visible | `GameUIManager`, `SceneLoader` | Pressing 'R' or clicking "RESTART RACE" reliably reloads the scene with restored health and state. | **PASS** |
| **TEST-31** | Clean Session Reset on Reload | Scene reload after race or death | `GameManager`, `ResultsScreenUI` | `ResetSession()` resets score, collected shards, race timer, and unfreezes `Time.timeScale = 1.0f`. | **PASS** |
| **TEST-32** | Normal Checkpoint Crossing | Vehicle crosses checkpoint gate | `Checkpoint`, `RaceManager` | Checkpoint trigger registers pass, updates next checkpoint index, and logs telemetry. | **PASS** |
| **TEST-33** | Strict Sequential Checkpoint Validation | Vehicle skips checkpoint (0 -> 2) | `RaceManager` | Out-of-sequence checkpoint is rejected (`false`); sequence does not advance until correct checkpoint is crossed. | **PASS** |
| **TEST-34** | Checkpoint Track Edge Containment | Crossing near road edges | `TrackBuilder`, `Checkpoint` | Checkpoint BoxCollider triggers span road width with neon pillars containing vehicle within track limits. | **PASS** |
| **TEST-35** | Police Interceptor Ramming Impulses | Police rams player near checkpoint | `PoliceVehicle` | Ramming force is clamped to forward plane impulse (4.5f), preventing vertical launch over track barriers. | **PASS** |
| **TEST-36** | Boost Pad Speed & Barrier Interaction | Vehicle boosts near checkpoint | `TrackBuilder`, `VehicleController` | Heightened 3.0m barriers with bounce physics material safely deflect high-speed boosted vehicles back to the road. | **PASS** |
| **TEST-37** | Checkpoint Safe Respawn Transform | Checkpoint definition | `Checkpoint` | Safe respawn position and track-aligned rotation are assigned to drivable surface coordinates. | **PASS** |
| **TEST-38** | Genuine Out-of-Bounds Fall Detection | Falling off road or skyway deck | `VehicleController` | Triggers recovery when Y < -3.5f or when Y < 4.0m in the elevated sky sector (Z = 180 to 295m). | **PASS** |
| **TEST-39** | Safe Checkpoint Recovery & Momentum Reset | Recovery after genuine fall | `VehicleController`, `Checkpoint` | Relocates vehicle to last safe checkpoint, restores forward track rotation, zeroes angular spin, adds 4 m/s heading speed. | **PASS** |
| **TEST-40** | Race Progression Preserved After Recovery | Complete race after recovering | `RaceManager`, `GameManager` | Vehicle retains checkpoint index, lap count, score, and proceeds to cross finish line for successful victory state. | **PASS** |

---

## Automated Test Suite Summary
- **Execution Command:** `/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath /Users/dev/GAME -executeMethod NeonRush.Tests.AutomatedTestRunner.RunAllTests -logFile -`
- **Total Tests Executed:** 87
- **Passed:** 87
- **Failed:** 0
- **Exit Code:** 0

## Standalone macOS Player Build
- **Execution Command:** `/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath /Users/dev/GAME -executeMethod NeonRush.EditorTools.BuildAutomation.BuildStandaloneOSX -logFile -`
- **Target Platform:** StandaloneOSX (Apple Silicon & Intel Universal)
- **Output Artifact:** `Builds/OSX/NeonRush.app` (173.1 MB)
- **Compilation Result:** Succeeded with 0 errors.


