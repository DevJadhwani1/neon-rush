# NEON RUSH OVERHAUL - STATUS REPORT

## 🎯 PROJECT VISION
Transform Neon Rush into a polished, advanced anti-gravity combat racer inspired by Wipeout/F-Zero with:
- Proper hover physics with magnetic track adhesion
- Energy-based health/boost system
- Procedural spline-based tracks
- Full campaign with story
- Combat racing with weapons
- 4 teams, 4 tracks, multiple game modes

---

## ✅ PHASE 1 COMPLETE: FOUNDATION (30% of Milestone 1)

### Scripts Archived to `_Archive/Scripts/`
Old/duplicate/broken systems removed from compilation:
- Root GameManager.cs
- SimpleCameraFollow.cs  
- EnemyController.cs
- SpawnManager.cs
- CompleteRaceStarter.cs, DirectRaceStarter.cs, SimpleRaceStarter.cs
- GameConfig.cs
- Modes/ folder (will rebuild)

### Packages Added (Unity 6 compatible)
```json
"com.unity.cinemachine": "3.1.2"
"com.unity.probuilder": "6.0.2"
"com.unity.render-pipelines.universal": "17.0.3"
"com.unity.splines": "2.7.1"
```

### Core Systems Built

**1. Ship Data System**
- `Assets/Scripts/Data/ShipData.cs` - ScriptableObject for ship stats
- 4 teams planned: GRIDLOCK (cyan), HELIX (red), NOVA DYNAMICS (purple), RAZOR WORKS (yellow)
- Editor tool: `Tools > Neon Rush > Create Default Ships`

**2. Ship Physics** (`Assets/Scripts/Vehicles/ShipController.cs`)
- Hover physics using 4 raycast points
- Magnetic track adhesion (gravity follows surface normal)
- Enables banked turns, loops, corkscrews
- Energy system: boost drains energy, wall scrapes damage
- Responsive steering with tilt/lean
- Air brake support

**3. Player Input** (`Assets/Scripts/Core/PlayerInput.cs`)
- Integrated with Unity Input System
- Steering, air brakes (Q/E), boost (Space/Shift)
- Fallback to legacy input if needed

**4. Track Builder** (`Assets/Scripts/Race/TrackBuilder.cs`)
- Procedural generation using Unity Splines
- Extrudes road mesh along spline path
- Auto-places: checkpoints, boost pads, weapon pads, energy cells
- Supports banking (roll), tunnels, pillars
- Context menu: Right-click component → "Build Track"

**5. Race HUD** (`Assets/Scripts/UI/RaceHUD.cs`)
- Speed display (km/h)
- Energy bar with color shift
- Position, lap counter, lap times
- Weapon display panel
- Mini-map placeholder
- Notifications (wrong way, pickups, etc.)

---

## 📁 NEW FOLDER STRUCTURE

```
Assets/
├── Data/
│   └── Ships/                    # Ship ScriptableObject assets
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs        # [EXISTS] State machine
│   │   ├── PlayerInput.cs        # [NEW] Ship input handler
│   │   └── ...
│   ├── Data/
│   │   └── ShipData.cs           # [NEW] Ship stats SO
│   ├── Vehicles/
│   │   └── ShipController.cs     # [NEW] Hover physics
│   ├── Race/
│   │   └── TrackBuilder.cs       # [NEW] Spline track generator
│   ├── UI/
│   │   └── RaceHUD.cs            # [NEW] In-race HUD
│   └── Editor/
│       └── ShipDataCreator.cs    # [NEW] Ship creation tool
└── ThirdParty/
    └── Kenney/                    # [EMPTY] Manual download required
```

---

## ⚠️ MANUAL ACTIONS REQUIRED

### 1. Download Kenney Asset Packs (CC0 License)
Visit **kenney.nl** and download these packs to `Assets/ThirdParty/Kenney/`:
- **Space Kit** (ships, space structures)
- **Racing Kit** (track props)
- **City Kit Commercial** (buildings, cityscape)
- **Sci-fi Sounds** (engines, weapons, UI)
- **Impact Sounds** (collisions, hits)
- **Interface Sounds** (UI interactions)

### 2. Continue Overhaul
Paste this into a **new Kiro chat** (Autopilot ON):
```
CONTINUE THE NEON RUSH OVERHAUL. No questions. Read OVERHAUL_LOG.md, find the first unticked item, continue from there through all remaining milestones. Verify with Unity MCP after each milestone. Final report in chat only.
```

---

## 🚧 NEXT STEPS (Priority Order)

### Immediate (Complete Milestone 1 - Vertical Slice)
1. **URP Setup**
   - Convert project to URP if not already
   - Create Volume with Bloom, Tonemapping (ACES), Color Adjustments
   - Set up neon color palette (cyan/magenta/purple)

2. **Ship Prefabs**
   - Create 4 ship prefabs using Kenney Space Kit models
   - Add ShipController, PlayerInput components
   - Set up team colors, engine trails, lights
   - Link to ShipData assets

3. **Track 1: Neon Downtown**
   - Create new scene `Track_NeonDowntown`
   - Build 1.5-2km circuit using TrackBuilder
   - Add City Kit buildings around track
   - Place boost pads, weapon pads, energy cells
   - Add holographic billboards, fog

4. **Cinemachine Cameras**
   - Chase cam (smooth follow, FOV by speed)
   - Cockpit cam
   - Rear view
   - Camera switch key

5. **Race Systems**
   - CheckpointSystem.cs (lap tracking, wrong way detection)
   - RaceManager.cs (8 racers, grid start, 3-2-1-GO, finish)
   - Lap timing, best lap recording

6. **Audio**
   - Engine sound layering (pitch by speed)
   - Boost, pickup, weapon sounds
   - Synthwave music loop (procedural or file)

### Medium (Milestones 2-3)
- AI racer controller with racing line following
- Weapon system (rockets, mines, EMP, shield, turbo)
- 3 additional tracks (Foundry, Sky Highway, Orbital Ring)

### Long-term (Milestones 4-5)
- Main Menu, Campaign, Quick Race, Time Trial, Hangar
- Save system, upgrades, story dialogue
- Polish: VFX, screen shake, speed lines, podium
- Performance optimization

---

## 🛠️ HOW TO USE EXISTING SYSTEMS

### Create Ship Data Assets
1. In Unity: `Tools > Neon Rush > Create Default Ships`
2. Assets appear in `Assets/Data/Ships/`
3. Customize in Inspector

### Build a Track
1. Create empty GameObject
2. Add `SplineContainer` component (Splines package)
3. Edit spline knots in Scene view
4. Add `TrackBuilder` component
5. Assign materials, prefabs
6. Right-click TrackBuilder → "Build Track"

### Set Up a Racing Ship
1. Create GameObject
2. Add: Rigidbody, BoxCollider, ShipController
3. Assign ShipData asset to ShipController
4. For player ship: add PlayerInput component
5. Add trail renderers, lights as children

---

## 📊 COMPILATION STATUS
**✅ 0 Errors** - All new scripts compile successfully  
**⚠️ Warnings** - Expected (missing prefab/material references until scene work is done)

---

## 📞 TROUBLESHOOTING

### If compilation fails:
1. Check Unity version is 6.x
2. Verify packages installed in Package Manager
3. Delete `Library/` folder and reopen project
4. Check `_Archive/` folder isn't being compiled (should be outside Assets)

### If MCP connection fails:
1. Check Unity MCP server is running
2. Verify `.kiro/settings/mcp.json` configuration
3. Restart Unity Editor if needed

---

## 🎮 STORY RECAP (For Implementation)
**Year 2089, Nova Prime mega-city.**  
Anti-gravity racing banned by Helix Corp, but underground league still races.  
**You are KAI**, rookie pilot for **RHEA's team GRIDLOCK**.  
**VEX** (Helix champion) dominates.  
Goal: Win races, survive Helix police raids, beat Vex on Orbital Ring to free the league.

**Characters:**
- **Kai** - Player (Gridlock pilot)
- **Rhea** - Mentor (Gridlock mechanic)
- **Vex** - Rival (Helix champion)
- **Commander Sato** - Antagonist (Helix police)

---

## 📝 IMPORTANT NOTES

1. **DO NOT** delete scripts in `_Archive/` - they're preserved for reference
2. **DO** use Unity 6 APIs only (linearVelocity, not velocity)
3. **DO** test track generation before building complex tracks
4. **DO** verify each milestone compiles with 0 errors before continuing
5. **DO** use MCP tools to inspect scene hierarchy and test in Play mode

---

**Last Updated:** October 25, 2026  
**Phase:** Foundation Complete (30%)  
**Status:** Ready for continuation
