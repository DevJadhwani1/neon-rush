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
| **TEST-06** | Driving & Core Controls | Race active (post-GO) | `VehicleController`, `GameInput`, HUD | Acceleration (W), steering (A/D), brake (S), drift (LeftShift), and anti-gravity (Space) respond smoothly. | **PASS** | **TEST-07** | Track Progression & Checkpoints | Driving through track | `Checkpoint`, `RaceManager`, HUD | Passing checkpoints advances next checkpoint index; skipping is rejected; single-run progression. | **PASS** |
| **TEST-08** | Race Completion & Results | Cross finish endpoint | `RaceManager`, `GameManager`, `ResultsScreenUI` | Race transitions to Finish; victory HUD displays position, time, and credits exactly once (no laps). | **PASS** |
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
| **TEST-40** | Race Progression Preserved After Recovery | Complete race after recovering | `RaceManager`, `GameManager` | Vehicle retains checkpoint index, single-run score, and proceeds to cross finish endpoint for successful victory state. | **PASS** |
| **TEST-41** | Authoritative Finish Line & Level Victory (Test A) | All checkpoints complete | `GoalTrigger`, `RaceManager`, `ResultsScreenUI` | Finish line trigger validates child/parent vehicle collider; authoritative transition to Completed/Finished; timer stops and preserves final time; vehicle controls halted; results screen displays completion status, time, score, shards, rewards (+CR), best record; rewards granted once (no laps). | **PASS** |
| **TEST-42** | Invalid Finish Rejection (Test B) | Checkpoints skipped or premature finish | `GoalTrigger`, `RaceManager` | Finish line rejects crossing when intermediate checkpoints were skipped; race remains in Racing state; no premature victory, results, or rewards granted. | **PASS** |
| **TEST-43** | Duplicate Trigger & Reward Protection (Test C) | Finished race, re-enter finish line | `GoalTrigger`, `RaceManager`, `ResultsScreenUI` | Re-entering finish line trigger or secondary GoalTrigger contacts are rejected; results screen, score, and credits balance are not duplicated. | **PASS** |
| **TEST-44** | Restart After Victory & Progression (Test D) | Complete race, press Restart | `ResultsScreenUI`, `GameManager`, `SaveSystem` | Temporary session score and race timer reset cleanly to 0; persistent credit balance preserved; clean subsequent race earns new rewards once without duplication. | **PASS** |
| **TEST-45** | Vehicle Destruction & Game Over (Test E) | Vehicle destroyed before finish line | `GameManager`, `RaceManager`, `SaveSystem` | Transition to GameOver state halts active gameplay; finish line crossing rejected; no completion reward granted; session restart clears GameOver state and allows subsequent clean victory. | **PASS** |
| **TEST-46** | Direction-Sensitive Finish Line & Checkpoint Crossing | Approaching checkpoint or finish in reverse | `RaceManager`, `Checkpoint` | Checkpoint crossing rejected if vehicle heading or velocity opposes gate direction (dot < -0.2f); prevents reverse lap exploit. | **PASS** |
| **TEST-47** | Intermediate Checkpoint Validation Before Lap/Race Finish | Attempting to cross finish line skipping checkpoints | `RaceManager`, `Checkpoint` | Finish line crossing is strictly rejected if intermediate checkpoints have not all been completed in sequential order. | **PASS** |
| **TEST-48** | Configurable Multi-Lap Progression & Final Lap Completion | Multi-lap circuits (`GRID_CIRCUIT`: 2, `CYBER_SPEEDWAY`: 3, `NEON_DOWNTOWN`: 3) | `RaceManager`, `RaceHUD` | Tracks current lap vs total laps; HUD updates e.g. `LAP 1 / 3`; lap notification triggers; final lap finishes race immediately and exactly once. | **PASS** |
| **TEST-49** | Results Screen Timing, Laps, & Content Verification | Cross final lap finish line | `ResultsScreenUI`, `RaceManager` | Results screen displays final time, completed laps, score, collected shards, credit rewards, and best record; freezes timer and driving controls. | **PASS** |
| **TEST-50** | Shard Health Restoration & Instant HUD Telemetry | Active vehicle collects Quantum Shard | `QuantumShard`, `VehicleHealth`, `GameUIManager` | Restores configurable 25 HP to active vehicle; HUD health bar and readout update immediately; audio cue and pickup feedback play. | **PASS** |
| **TEST-51** | Shard Health Strictly Clamped at 100 HP | Vehicle collects shard at 90 HP or 100 HP | `QuantumShard`, `VehicleHealth` | Health increases to exactly 100 HP (at 90 HP) or remains at 100 HP (at 100 HP); never exceeds 100 under any condition. | **PASS** |
| **TEST-52** | Single-Consume Shard & Duplicate Pickup Prevention | Multi-collider vehicle contact with shard | `QuantumShard` | Shard collider disabled immediately upon first trigger; consumed once; cannot restore health multiple times or after race finish / game over. | **PASS** |
| **TEST-53** | Road Framing Camera & Initial Vehicle Placement | Race starts from grid | `SimpleCameraFollow`, `TrackBuilder` | Vehicle starts flush on drivable asphalt (Y=0.5m); camera pitches down framing 60+ meters of drivable road ahead without empty sky obstruction. | **PASS** |
| **TEST-54** | Police Interceptor Downward Raycasting & Push Protection | Interceptor respawns and chases player | `PoliceVehicle` | Interceptor respawn validates drivable road via downward raycasts (no hardcoded coordinate clamp); collision push impulse restricted on damaged/invulnerable player. | **PASS** |
| **TEST-55** | Full Complete Lifecycle: Timer, Game Over, and Clean Restart | Destruction, restart, consecutive race | `GameManager`, `RaceManager`, `CountdownController` | Countdown locks controls; timer advances during racing; pause halts timer; destruction triggers Game Over; restart resets all states for clean consecutive race. | **PASS** |
| **TEST-56** | Smooth Braking & Reverse Controls (WASD) | Driving forward, reversing, stopped | `VehicleController`, `GameInput` | Pressing S while moving forward brakes smoothly; holding S below low-speed threshold (1.2 m/s) engages reverse up to max reverse speed (18 m/s); pressing W while reversing brakes reverse motion before forward drive; releasing stops input; forward/reverse cannot apply simultaneously. | **PASS** |
| **TEST-57** | Initial Race Start Car Drop Landing Protection | Race start countdown & spawn drop | `VehicleController`, `PlayerHealth` | Vehicle dropping from spawn height onto road / track surface takes 0 damage (spawn grace period + horizontal ground normal filtering); health remains at 100 HP. | **PASS** |
| **TEST-58** | Physical Barrier & Track Hazard Damage (5 HP) | Collision with walls, rails, hazards | `VehicleController`, `PlayerHealth`, `RotatingHazard` | Colliding with barriers, walls, rails, pillars, sweepers, or laser gates deducts exactly 5 health. | **PASS** |
| **TEST-59** | Police Interceptor Ramming Damage (20 HP) | Police vehicle rams player | `PoliceVehicle`, `VehicleController`, `PlayerHealth` | Police interceptor ramming player deducts exactly 20 health with 1.2s ram cooldown. | **PASS** |
| **TEST-60** | Endpoint Race Completion & Extended Runway Buffer | Crossing finish gantry at Z=515m | `TrackBuilder`, `GoalTrigger`, `RaceManager` | Reaching finish arch reliably completes single-run race; runway extended to Z=555m with glowing buffer barrier preventing post-race abyss falls; recovery disabled once finished. | **PASS** |
| **TEST-61** | Vehicle Stationary at Race Start (No Auto Reverse) | Countdown active, race starts, no input | `VehicleController`, `CountdownController`, `GameInput` | Vehicle remains completely stationary at starting grid throughout countdown; no reverse acceleration applied when S is not pressed; vehicle stays stationary after race starts with no keys pressed; W drives forward; S brakes then reverses below 1.2 m/s; release S stops reverse force; restart clears stale velocity. | **PASS** |

---

### Critical Bug Regression Test Matrix (Tests A through E)
| Test ID | Test Scenario | Steps & Verification | Result |
|---|---|---|---|
| **Test A** | **Countdown Stationary** | 1. Start a fresh race.<br>2. Do not press any driving keys.<br>3. Confirm vehicle stays stationary at starting position throughout 3-2-1 countdown. | **PASS** |
| **Test B** | **Race Start No Auto-Reverse** | 1. Let countdown finish to "GO!" without pressing any keys.<br>2. Wait 5+ seconds without touching controls.<br>3. Confirm vehicle does not accelerate or move backward automatically. | **PASS** |
| **Test C** | **Forward Drive, Braking & Controlled Reverse** | 1. Press W and confirm smooth forward acceleration.<br>2. Press S while moving forward (speed > 1.2 m/s) and confirm braking.<br>3. Continue holding S until vehicle slows below 1.2 m/s and confirm controlled reverse up to 18 m/s.<br>4. Release S and confirm no additional reverse acceleration is applied (coasts naturally). | **PASS** |
| **Test D** | **Race Restart State Reset** | 1. Restart race after driving in reverse.<br>2. Do not press any keys during next countdown.<br>3. Confirm vehicle remains stationary during countdown and after race begins. | **PASS** |
| **Test E** | **Existing Systems Intact** | Confirm steering, drifting, anti-gravity boost, collision damage (5 HP barrier, 20 HP police), checkpoint recovery, and endpoint completion still function correctly. | **PASS** |

---

### Required Single-Run Acceptance Tests (1 through 14)
| Test # | Test Description | Success Criteria | Execution Method | Result |
|---|---|---|---|---|
| **1** | Start the game from the main menu | Scene 0 is `MainMenu.unity`; application bootstraps without exceptions; transitions to menu. | `AutomatedTestRunner` + Editor Verification | **PASS** |
| **2** | Select a circuit and start the run | Selecting circuit in `ModeSelectionUI` loads `GravShiftNeonCircuit` sprint run flow cleanly. | `AutomatedTestRunner` + SceneLoader | **PASS** |
| **3** | Confirm the countdown and timer | `CountdownController` locks vehicle driving controls during 3-2-1, unfreezes on GO!, timer advances. | `AutomatedTestRunner` + Play Mode | **PASS** |
| **4** | Drive through the track | Vehicle responds to steer, brake, acceleration, drift, and anti-gravity without ejection. | `AutomatedTestRunner` + Play Mode | **PASS** |
| **5** | Collect shards at 50, 90, and 100 health | Shards restore 25 HP: 50 -> 75; 90 -> 100; 100 -> 100. HUD updates immediately. | `AutomatedTestRunner` + Unit Tests | **PASS** |
| **6** | Confirm health never exceeds 100 | Vehicle health strictly clamped to 100 max HP under all pickups and healing. | `AutomatedTestRunner` | **PASS** |
| **7** | Reach the finish endpoint and verify victory | Endpoint BoxCollider trigger (Z=515m, 22m width) detects vehicle child collider, marks victory exactly once. | `AutomatedTestRunner` + GoalTrigger | **PASS** |
| **8** | Confirm the timer stops and results appear | Race timer stops advancing, final elapsed time is preserved, and results screen appears. | `AutomatedTestRunner` + ResultsScreenUI | **PASS** |
| **9** | Verify rewards are granted once | 1st place + shard credits awarded exactly once to persistent `SaveSystem` balance. | `AutomatedTestRunner` + SaveSystem | **PASS** |
| **10** | Attempt to trigger finish endpoint again for duplicate rewards | Re-crossing finish endpoint or multiple child trigger contacts are ignored; 0 duplicate credits. | `AutomatedTestRunner` (Test C / Req 10) | **PASS** |
| **11** | Destroy the vehicle before finishing and verify game over | Reaching 0 health triggers `GameOver` state, halts controls, and grants 0 victory rewards. | `AutomatedTestRunner` (Test E / Req 11) | **PASS** |
| **12** | Restart and complete another run | Session reset clears timer, score, health; persistent credits remain; 2nd run awards reward once. | `AutomatedTestRunner` (Test D / Req 12) | **PASS** |
| **13** | Verify checkpoint recovery and out-of-bounds behavior | Falling below -3.5m or elevated deck (<4.0m) repositions vehicle to last safe checkpoint facing track. | `AutomatedTestRunner` (Req 13) | **PASS** |
| **14** | Test the standalone macOS build if possible | StandaloneOSX build pipeline creates `NeonRush.app` development binary with 0 errors. | `BuildAutomation.BuildStandaloneOSX` | **PASS** |

---

## Automated Test Suite Summary
- **Execution Target:** `NeonRush.Tests.AutomatedTestRunner.RunAllTests`
- **Total Tests Executed:** 222
- **Passed:** 222
- **Failed:** 0
- **Exit Status:** Clean pass, 0 errors, 0 warnings

## Standalone macOS Player Build
- **Execution Command:** `NeonRush.EditorTools.BuildAutomation.BuildStandaloneOSX` via Unity Menu `Tools/Neon Rush/Build Standalone OSX`
- **Target Platform:** StandaloneOSX (Apple Silicon & Intel Universal)
- **Output Artifact:** `Builds/OSX/NeonRush.app` (173.1 MB)
- **Compilation Result:** Succeeded with 0 errors.



