# NEON RUSH: Grid Runners - Project Map

## Unity Environment
- **Unity Version**: 6000.0.6f1 (Unity 6)
- **Render Pipeline**: Built-in (no URP/HDRP packages)
- **Input System**: New Input System (com.unity.inputsystem 1.20.0 installed)
- **TextMeshPro**: NOT installed (use UnityEngine.UI.Text)
- **Post Processing**: NOT installed (skip post-processing effects)
- **UGUI**: Installed (com.unity.ugui 2.6.0)

## Existing Scripts (24 files in Assets/Scripts)

### Core Game Systems
- **GameManager.cs** - Singleton, manages game state (isGameActive, hasWon), tracks score/shards/time, handles victory/game-over
- **GameUIManager.cs** - OnGUI-based HUD rendering, health/speed/timer/score/shards display, notifications, victory/game-over screens
- **GameInput.cs** - Input abstraction supporting both old and new Input System, handles all player controls
- **SoundManager.cs** - Audio singleton (currently placeholder with empty methods)
- **CameraSetup.cs** - Camera initialization helper
- **SpawnManager.cs** - Dynamic prefab instantiation for Player and Enemies at runtime

### Player Systems
- **PlayerController.cs** - On-foot movement (WASD, 7 m/s), health (100 HP), collision damage, respawn on fall
- **PlayerHealth.cs** - Assignment-compliant health component (Start init, OnCollisionEnter, Destroy at 0)
- **PlayerVisuals.cs** - Astronaut visual model with golden visor

### Vehicle Systems
- **VehicleController.cs** - Complete anti-gravity racing physics (686 lines):
  - Arcade physics: acceleration 48, maxSpeed 40, turnSpeed 115
  - 4-corner raycast suspension (rideHeight 0.65m, suspensionForce 65)
  - Anti-gravity boost system (3.5s duration, starts with 1 charge, max 3)
  - Drift & mini-turbo (Mario Kart style, builds charge over time)
  - Health system (100 HP, 25 damage from hazards)
  - Downforce, upright torque, brake force
  - Underglow light (cyan normal, magenta during anti-gravity)
- **VehicleInteraction.cs** - Enter/exit vehicle system (E key), camera switching, player parenting
- **CarVisuals.cs** - Vehicle visual components

### Track & Environment
- **TrackBuilder.cs** - Procedural 550m circuit generator (800+ lines):
  - Entry: Awake() calls LoadMaterials() then BuildFullCircuit()
  - Builds 5 sectors sequentially using helper functions
  - Places hazards, pickups, checkpoints via Create* helper methods
  - Track segments positioned with Vector3 coordinates and rotation angles
  - Does NOT currently expose centre line waypoints (needs modification)
- **GoalTrigger.cs** - Finish line detection, calls GameManager.OnCourseCompleted()

### Hazards
- **RotatingHazard.cs** - Spinning laser sweepers (60-75 RPM, 25 damage), checks anti-gravity for immunity
- **PulsingLaserGate.cs** - Timed laser barriers (3s ON/OFF cycle, 25 damage)

### Pickups
- **BoostPad.cs** - Speed boost zones (26-35 m/s impulse, 50 points, 1.5s cooldown)
- **EnergyPickup.cs** - Anti-gravity recharge cells (100 points, adds 1 charge, max 3)
- **QuantumShard.cs** - Rotating collectible shards (50 points, registers with GameManager)
- **SpeedRing.cs** - Mid-air boost rings (22 m/s boost, 150 points, single-use)

### Enemies
- **EnemyController.cs** - Ground combat drones (50 HP, 25 damage, patrol/chase AI, 5 m/s, 25m detection)
- **PoliceVehicle.cs** - Police interceptor (450 kg, 18-32 m/s, pursuit AI, rubber-banding, dynamic repositioning)

### Camera
- **SimpleCameraFollow.cs** - Smart follow camera (position/rotation damping, wall clipping prevention, speed-based FOV 62-72°)

### UI Helpers
- **TurnSign.cs** - Chevron warning signs for sharp turns

## Key System Ownership

| System | Script | Key Methods |
|--------|--------|-------------|
| Vehicle Driving | VehicleController | ApplyDrive(), ApplySteering(), ApplySuspensionCushion() |
| Boost/Anti-Gravity | VehicleController | ActivateAntiGravity(), DeactivateAntiGravity(), RechargeAntiGravity() |
| Enter/Exit Vehicle | VehicleInteraction | EnterVehicle(), ExitVehicle(), TryEnterVehicle() |
| Camera | SimpleCameraFollow | LateUpdate(), UpdateCameraPositionAndRotation(), UpdateCameraFOV() |
| Drones | EnemyController | HandleMovement(), ChasePlayer(), PatrolArea() |
| Police | PoliceVehicle | FixedUpdate(), RespawnOnTrack(), RespawnBehindPlayer() |
| Shards | QuantumShard | Start() registers, OnTriggerEnter() collects |
| Energy Cells | EnergyPickup | Collect() calls VehicleController.RechargeAntiGravity() |
| Boost Pads | BoostPad | OnTriggerEnter() applies force |
| Speed Ring | SpeedRing | OnTriggerEnter() applies directional boost |
| Laser Sweepers | RotatingHazard | Update() rotates, HandleDamage() checks anti-gravity |
| Laser Gates | PulsingLaserGate | Update() cycles state, HandleDamage() applies damage |
| HUD | GameUIManager | OnGUI() renders all UI, Update* methods set data |
| Victory/Game-Over | GameManager | OnCourseCompleted(), OnPlayerDied(), OnVehicleDestroyed() |
| Track Generation | TrackBuilder | BuildFullCircuit(), Create* helper methods (18 functions) |

## Track Generator Details

### Entry Point
- `TrackBuilder.Awake()` → `BuildFullCircuit()`

### Segment Positioning
- Uses Vector3 coordinates directly (e.g., `new Vector3(0f, -0.5f, -15f)`)
- Rotation via `rotY` and `rotX` parameters in Create* methods
- Sequential Z-axis progression: Sector 1 (-35 to 65), Sector 2 (65 to 175), Sector 3 (175 to 295), Sector 4 (295 to 420), Sector 5 (420 to 520)

### Hazard Spawning
- `CreateRotatingSweeper(parent, name, pivotPos, armLength, speed)` - 5 locations
- `CreateLaserGate(parent, name, pos, width)` - 3 locations
- `CreateHazard(parent, name, pos, size)` - 1 bottomless pit

### Pickup Spawning
- `CreateBoostPad(parent, pos, boostPower, rotY)` - 8 locations
- `CreateEnergyZone(parent, pos)` - 2 locations (both in Sector 1)
- `CreateShard(parent, pos)` - ~30 locations throughout
- `CreateSpeedRing(parent, pos, rot)` - 1 location (mid-air in Sector 3)

### Centre Line Waypoints
**Current State**: NOT exposed. Track segments are created independently without a unified waypoint list.
**Needs**: Add a public List<Vector3> centerLineWaypoints that stores key positions during BuildFullCircuit() for AI to follow.

## Scenes
- **Assets/,.unity** (current race scene, unusual name)
- **Assets/Scenes/GravShiftNeonCircuit.unity** (exists but may not be in Build Settings)
- **Assets/Scenes/SampleScene.unity** (legacy test scene)

## Existing Warnings/Bugs
- Scene file named ",.unity" (unusual, should rename)
- Vehicle mass discrepancy: scene shows 300kg, code sets 400kg (auto-corrects on start)
- No compile errors detected
- SoundManager methods are empty placeholders

## Modifications Needed for NEON RUSH
1. GameManager needs state machine (Splash, Title, MainMenu, etc.)
2. VehicleController needs stat setters for vehicle customization
3. TrackBuilder needs waypoint generation and theme support
4. Health system needs armor concept separate from instant death
5. New folder structure needs creation
6. All new scripts go in namespace NeonRush
