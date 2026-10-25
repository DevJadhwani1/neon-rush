# NEON RUSH OVERHAUL LOG

## MILESTONE 1 — VERTICAL SLICE
- [x] 1.1 Clean up: archive messy/duplicate/broken scripts
- [x] 1.2 Add packages: Splines, Cinemachine, URP (removed ProBuilder)
- [ ] 1.3 Assets: Kenney packs (manual download required)
- [x] 1.4 Ships: 4 teams with ShipData ScriptableObjects + prefab builder
- [x] 1.5 Track system: TrackBuilder with Splines
- [x] 1.6 Build Track 1 "Neon Downtown" (editor tool created)
- [x] 1.7 Visual quality: URP setup tool with post-processing
- [x] 1.8 Cameras with Cinemachine (chase, cockpit, cinematic)
- [x] 1.9 Race flow: RaceManager with grid start, 3 laps, finish
- [x] 1.10 HUD: fully integrated with RaceManager (speed, energy, position, lap time)
- [x] 1.11 Audio: ShipAudio (engine, boost, impact) + MusicManager
- [ ] 1.12 Verify: playtest via MCP, build scene, 0 errors

## MILESTONE 2 — AI & COMBAT
- [ ] 2.1 AI pilots with racing line following
- [ ] 2.2 AI names and teams
- [ ] 2.3 Weapons fully working with VFX
- [ ] 2.4 Helix police drones and enforcer

## MILESTONE 3 — MORE TRACKS
- [ ] 3.1 Track 2: Foundry Inferno
- [ ] 3.2 Track 3: Sky Highway
- [ ] 3.3 Track 4: Orbital Ring

## MILESTONE 4 — GAME STRUCTURE
- [ ] 4.1 Main Menu
- [ ] 4.2 Campaign: 4 chapters, 12 events
- [ ] 4.3 Quick Race
- [ ] 4.4 Time Trial with ghost
- [ ] 4.5 Hangar: ship selection, upgrades
- [ ] 4.6 Settings menu
- [ ] 4.7 Save system
- [ ] 4.8 Loading screen and pause menu

## MILESTONE 5 — POLISH
- [ ] 5.1 Race intro: flyover
- [ ] 5.2 Finish: slow-motion, podium
- [ ] 5.3 Juice: effects and animations
- [ ] 5.4 Performance optimization
- [ ] 5.5 Build settings and tools menu
- [ ] 5.6 Final MCP playtest

## PROGRESS SUMMARY
**STATUS:** Milestone 1 systems complete (90% of Milestone 1) - ready for scene assembly

**COMPLETED (Milestone 1):**
- ✅ Archived old/duplicate scripts to _Archive/ folder
- ✅ Added Unity packages: Cinemachine, URP, Splines (removed ProBuilder)
- ✅ Converted entire project to old Input Manager (no Input System)
- ✅ Created ShipData ScriptableObject system
- ✅ Built ShipController with hover physics and magnetic track adhesion
- ✅ Created ShipInput handler for ship controls (old Input Manager)
- ✅ Built TrackBuilder using Unity Splines for procedural generation
- ✅ Created RaceHUD fully integrated with RaceManager
- ✅ Created editor tools: ShipDataCreator, ShipBuilder, Track1Builder, URPSetup, CameraSetup
- ✅ Built race systems: RaceManager with lap counting, Checkpoint system
- ✅ Created Cinemachine camera rigs (chase, cockpit, cinematic)
- ✅ Built audio systems: ShipAudio (procedural engine/boost/impact), MusicManager
- ✅ Created URP setup tool with post-processing profile (bloom, tonemapping, vignette, chromatic aberration)
- ✅ Created Track 1 "Neon Downtown" builder tool with spline and environment

**NEW SCRIPTS CREATED (Session 2):**
- Assets/Scripts/Editor/URPSetup.cs - URP rendering and post-processing setup
- Assets/Scripts/Editor/ShipBuilder.cs - Build 4 team ship prefabs
- Assets/Scripts/Editor/CameraSetup.cs - Cinemachine virtual cameras
- Assets/Scripts/Editor/Track1Builder.cs - Track 1 "Neon Downtown" scene builder
- Assets/Scripts/Race/RaceManager.cs - Race state, lap counting, positions
- Assets/Scripts/Race/Checkpoint.cs - Track checkpoints for lap detection
- Assets/Scripts/Audio/ShipAudio.cs - Procedural engine, boost, impact sounds
- Assets/Scripts/Audio/MusicManager.cs - Background race music

**NEXT STEPS:**
1. **SCENE ASSEMBLY:** Use editor tools to build Track 1 scene
   - Run Tools > Neon Rush > Setup URP Rendering
   - Run Tools > Neon Rush > Create Post-Processing Volume
   - Run Tools > Neon Rush > Create Neon Skybox
   - Run Tools > Neon Rush > Build Ship Prefabs
   - Run Tools > Neon Rush > Build Track 1 - Neon Downtown
   - Instantiate ship prefabs on track
   - Run Tools > Neon Rush > Setup Race Cameras
   - Add RaceManager component to scene

2. **PLAYTEST:** Test race in Play mode via Unity MCP
3. **MILESTONE 2:** Start AI pilots and combat systems

**IMMEDIATE ACTIONS:**
- Execute editor tools to assemble Track 1 scene
- Verify compilation (0 errors)
- Playtest and iterate
- [ ] 1.7 Visual quality: URP lighting and post-processing
- [ ] 1.8 Cameras with Cinemachine
- [ ] 1.9 Race flow: grid start, 3 laps, finish
- [ ] 1.10 HUD: speed, energy, position, lap, map, etc.
- [ ] 1.11 Audio: engine, boost, weapons, music
- [ ] 1.12 Verify: playtest via MCP, 0 errors

## MILESTONE 2 — AI & COMBAT
- [ ] 2.1 AI pilots with racing line following
- [ ] 2.2 AI names and teams
- [ ] 2.3 Weapons fully working with VFX
- [ ] 2.4 Helix police drones and enforcer

## MILESTONE 3 — MORE TRACKS
- [ ] 3.1 Track 2: Foundry Inferno
- [ ] 3.2 Track 3: Sky Highway
- [ ] 3.3 Track 4: Orbital Ring

## MILESTONE 4 — GAME STRUCTURE
- [ ] 4.1 Main Menu
- [ ] 4.2 Campaign: 4 chapters, 12 events
- [ ] 4.3 Quick Race
- [ ] 4.4 Time Trial with ghost
- [ ] 4.5 Hangar: ship selection, upgrades
- [ ] 4.6 Settings menu
- [ ] 4.7 Save system
- [ ] 4.8 Loading screen and pause menu

## MILESTONE 5 — POLISH
- [ ] 5.1 Race intro: flyover
- [ ] 5.2 Finish: slow-motion, podium
- [ ] 5.3 Juice: effects and animations
- [ ] 5.4 Performance optimization
- [ ] 5.5 Build settings and tools menu
- [ ] 5.6 Final MCP playtest

## DECISIONS LOG
**Archived Scripts** (moved to _Archive/Scripts/):
- Old root GameManager.cs (replaced by Core/GameManager.cs with proper namespace)
- SimpleCameraFollow.cs (will use Cinemachine)
- EnemyController.cs (will rebuild as AI racer)
- SpawnManager.cs (will integrate into race systems)
- CompleteRaceStarter.cs, DirectRaceStarter.cs, SimpleRaceStarter.cs (old race flow)
- GameConfig.cs (configuration flags - will use ScriptableObjects)
- Modes/ folder (old game modes - will rebuild)

**Packages Added**:
- com.unity.cinemachine: 3.1.2
- com.unity.probuilder: 6.0.2
- com.unity.render-pipelines.universal: 17.0.3
- com.unity.splines: 2.7.1

**New Core Systems Created**:
1. **ShipData.cs** - ScriptableObject for ship stats (speed, acceleration, handling, energy, weight)
2. **ShipController.cs** - Advanced hover physics with:
   - Magnetic track adhesion (gravity follows surface normal)
   - 4-point hover raycasting
   - Smooth steering with tilt/lean
   - Energy system (boost drains energy, wall scrapes damage)
   - Air brake support
3. **PlayerInput.cs** - Input handler for ship controls
4. **TrackBuilder.cs** - Procedural track generation using Unity Splines:
   - Extrudes road mesh along spline
   - Auto-places checkpoints, boost pads, weapon pads, energy cells
   - Supports banking and track features
5. **RaceHUD.cs** - Race UI system (speed, energy, position, lap time, notifications)
6. **ShipDataCreator.cs** - Editor tool to create team ship assets

**Design Decisions**:
- Using Unity 6 APIs exclusively (linearVelocity, linearDamping, FindAnyObjectByType)
- All scripts in plain `namespace NeonRush` (no sub-namespaces)
- ScriptableObject-driven architecture for data (ships, tracks, weapons)
- Ship energy serves dual purpose: health and boost fuel (Wipeout-style)
- Hover physics uses 4 raycast points for stability
- Track gravity follows surface normal for banking/loops/corkscrews

**Kenney Assets**:
- Created Assets/ThirdParty/Kenney folder
- Packs needed (CC0, manual download from kenney.nl):
  * Space Kit
  * Racing Kit
  * City Kit Commercial
  * Sci-fi Sounds
  * Impact Sounds
  * Interface Sounds
