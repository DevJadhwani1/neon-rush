# NEON RUSH: Grid Runners

**A cyberpunk anti-gravity racing game built in Unity 6**

*Developed by Gridlock Studios (3-member team)*

---

## Overview

NEON RUSH is a complete racing game featuring:
- **3 Vehicles** - VOLT (free), BRICK (2000cr), BLADE (3500cr)
- **3 Tracks** - Neon Downtown, Cyber Wasteland, Quantum Vortex
- **4 Game Modes** - Time Trial, Grand Prix, Police Chase, Shard Hunt
- **Upgrade System** - 4 stats × 3 levels per vehicle
- **Combat & Power-Ups** - Shield, EMP, Pulse Blaster, Magnet
- **Racing Mechanics** - Drift-to-nitro (3 tiers), slipstream, perfect start, anti-gravity boost

---

## Quick Start

### Requirements
- Unity 6000.0.6f1
- Built-in Render Pipeline
- New Input System (installed)

### Setup Instructions

1. Open project in Unity 6
2. Go to **Tools > Neon Rush > Setup Everything**
3. Wait for scene and asset creation
4. Open **MainMenu** scene (Assets/Scenes/MainMenu.unity)
5. Press **Play**

---

## File Structure

```
Assets/
├── Scripts/
│   ├── Core/          - GameManager, SceneLoader, GameBootstrap
│   ├── Vehicles/      - VehicleData, VehicleHealth, VehicleStatsApplier
│   ├── Combat/        - PowerUpSystem, StyleComboSystem
│   ├── Race/          - RaceManager, CheckpointSystem, CountdownController
│   ├── AI/            - AIRacer (waypoint-based)
│   ├── Modes/         - TimeTrialMode, GrandPrixMode, PoliceChaseMode, ShardHuntMode
│   ├── UI/            - MainMenuUI, GarageUI, ResultsScreenUI, PauseMenuUI
│   ├── Progression/   - SaveSystem, UpgradeSystem
│   ├── Audio/         - AudioManager, ProceduralAudio
│   ├── FX/            - VFXLibrary (particle effects)
│   └── Editor/        - NeonRushSetup (one-click setup)
├── Data/
│   ├── Vehicles/      - ScriptableObject vehicle data
│   └── Tracks/        - ScriptableObject track themes
├── Materials/         - Track, vehicle, hazard materials
├── Prefabs/           - Player, Enemy, Vehicle, PoliceCar
├── Resources/         - Runtime-loaded assets
└── Scenes/
    ├── MainMenu.unity - Main menu (index 0)
    └── ,.unity        - Race scene (index 1)
```

---

## Core Systems

### Vehicle System
- **VehicleController.cs** - Anti-gravity physics, drift, mini-turbo
- **VehicleHealth.cs** - Armor system, respawn at checkpoints
- **VehicleStatsApplier.cs** - Applies vehicle data + upgrades

### Progression
- **SaveSystem.cs** - Persistent save data (credits, unlocks, upgrades)
- **UpgradeSystem.cs** - 4-stat upgrade system (Engine, Handling, Boost, Armor)

### Combat
- **PowerUpSystem.cs** - 4 power-ups (Shield, EMP, Pulse Blaster, Magnet)
- **StyleComboSystem.cs** - Combo multiplier for style actions

### Race Management
- **RaceManager.cs** - Lap tracking, position calculation, race timing
- **CheckpointSystem.cs** - Lap validation, checkpoint gates
- **CountdownController.cs** - 3-2-1-GO countdown

### AI
- **AIRacer.cs** - Waypoint-following AI with rubber-banding
- 3 AI opponents in Grand Prix mode

---

## Game Modes

### Time Trial
- Complete 3 laps as fast as possible
- Earn credits based on finish time
- Personal best lap times tracked

### Grand Prix
- Race against 3 AI opponents
- Earn credits based on final position (1st: 1000cr, 2nd: 600cr, 3rd: 300cr, 4th: 100cr)

### Police Chase
- Survive 5 minutes of police pursuit
- 3 police vehicles chase the player
- EMP power-up can disable police temporarily

### Shard Hunt
- Collect 50 shards within 5 minutes
- Extra shards spawned around track
- Bonus credits for remaining time

---

## Controls

**Vehicle Driving:**
- W/S or Up/Down - Accelerate / Reverse
- A/D or Left/Right - Steer
- Space - Brake / Drift
- Shift - Activate Anti-Gravity Boost (consumes charge)
- Q - Use Power-Up

**On Foot:**
- WASD - Move
- E - Enter/Exit Vehicle

**UI:**
- ESC or P - Pause Menu
- Mouse - Navigate menus

---

## Upgrade System

Each vehicle has 4 upgradeable stats:
1. **Engine** - +15% max speed per level (500cr, 1000cr, 2000cr)
2. **Handling** - +10% turn rate per level (400cr, 800cr, 1600cr)
3. **Boost** - +0.5s boost duration per level (600cr, 1200cr, 2400cr)
4. **Armor** - +20 HP per level (300cr, 600cr, 1200cr)

Access upgrades in the **Garage** (TODO: add garage scene to build settings)

---

## Power-Ups

- **Shield** - 4s invulnerability (cyan glow)
- **EMP** - Disable all enemies within 25m for 3s (yellow)
- **Pulse Blaster** - Fire 5 forward shots (red-orange)
- **Magnet** - Pull shards within 15m for 6s (lime)

---

## Racing Techniques

### Perfect Start
- Hold accelerator during countdown (in the green window before GO)
- +25 m/s speed boost at race start

### Drift-to-Nitro (3 tiers)
- **Blue Nitro** - Drift 1.0s+ (+8 m/s boost)
- **Orange Nitro** - Drift 2.0s+ (+15 m/s boost)
- **Pink Nitro** - Drift 3.5s+ (+25 m/s boost)

### Slipstream
- Draft behind another vehicle within 15m
- +5 m/s speed boost while in slipstream

### Anti-Gravity Boost
- Press Shift to activate (consumes 1 charge)
- 3.5s hover mode: immune to hazards, increased speed
- Recharge by collecting Energy Cells

---

## Development Notes

### Existing Systems (Pre-Neon Rush)
- VehicleController (686 lines) - already had anti-grav, drift, mini-turbo
- TrackBuilder (800+ lines) - procedural 550m circuit generator
- EnemyController, PoliceVehicle - ground drones and police interceptor
- GameManager, GameUIManager - basic HUD and game state

### Neon Rush Additions
- **30+ new scripts** across 9 modules
- **GameModeBase** abstract class for extensibility
- **PowerUpSystem** with 4 unique power-ups
- **RaceManager** with lap/position tracking
- **SaveSystem** with persistent progression
- **Procedural audio** clips generated in code
- **Particle VFX library** for boost trails, explosions, collectibles

### Known TODOs
- TrackBuilder needs to expose `centerLineWaypoints` list for AI
- Create track theme data assets via editor menu
- Extend TrackBuilder to apply theme colors
- Add position/lap/lap time to HUD
- Camera shake on collisions
- Speed lines VFX during high speed
- Minimap (RenderTexture)
- Living city background
- Achievements system
- Instant replay

---

## Build Instructions

### Standalone Build

1. **macOS:**
   - Build from Unity: File > Build Settings > Mac > Build
   - Or use menu: Tools > Neon Rush > Build macOS

2. **Windows:**
   - Build from Unity: File > Build Settings > Windows > Build
   - Or use menu: Tools > Neon Rush > Build Windows

### Build Settings
- Scenes in build:
  1. MainMenu (index 0)
  2. Race Scene (,.unity or GravShiftNeonCircuit.unity) (index 1)

---

## Credits

**Gridlock Studios** - 3-member student team

### Team Contributions
- **Member 1** - Vehicles, Combat, Power-Ups, Police Chase, Shard Hunt
- **Member 2** - Race Systems, AI, Checkpoints, Time Trial, Grand Prix
- **Member 3** - Core Architecture, UI, Audio/VFX, Progression, Build Pipeline

---

## License

Educational project - Not for commercial use
