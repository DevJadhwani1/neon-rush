# NEON RUSH - Quick Start Guide

## First Time Setup (5 minutes)

### Step 1: Open Project
1. Launch Unity Hub
2. Click "Open" → Navigate to project folder
3. Confirm Unity version: **6000.0.6f1**
4. Wait for project to load

### Step 2: Run Setup Menu
1. In Unity Editor, click **Tools** menu at top
2. Select **Neon Rush > Setup Everything**
3. Click **"Yes"** when prompted
4. Wait for setup to complete (~30 seconds)
   - Creates MainMenu scene
   - Creates vehicle data assets (VOLT, BRICK, BLADE)
   - Creates track theme assets (3 tracks)
   - Sets build settings

### Step 3: Play the Game
1. Open **Assets/Scenes/MainMenu.unity** in Project window
2. Press **Play** button (▶) in Unity Editor
3. Click **PLAY** button in game
4. Race!

---

## Controls

### Driving
- **W / Up Arrow** - Accelerate
- **S / Down Arrow** - Reverse
- **A/D / Left/Right Arrows** - Steer
- **Space** - Brake / Drift
- **Shift** - Activate Anti-Gravity Boost
- **Q** - Use Power-Up

### On Foot
- **WASD** - Move
- **E** - Enter/Exit Vehicle

### Menu
- **Mouse** - Navigate UI
- **ESC or P** - Pause (during race)

---

## Game Flow

### Main Menu
- **PLAY** → Start race (TODO: add mode selection)
- **GARAGE** → Vehicle selection & upgrades
- **QUIT** → Exit game

### Racing
1. **Countdown** - Hold W for perfect start (green zone)
2. **Drive** - Complete 3 laps
3. **Boost** - Press Shift to activate anti-gravity (uses charge)
4. **Drift** - Hold Space + turn for nitro boost (3 tiers)
5. **Power-Ups** - Drive through glowing cubes, press Q to use

### After Race
- **Results Screen** shows:
  - Final position
  - Race time
  - Credits earned
- Click **RETRY** to race again
- Click **MAIN MENU** to return

---

## Progression System

### Credits
- Earn credits by:
  - Completing races (mode-dependent rewards)
  - Collecting shards (50 credits each)
  - Style actions (drift, near miss, air time)
  - Destroying enemies (200 credits)

### Unlocking Vehicles
- **VOLT INTERCEPTOR** - Free (starter vehicle)
- **BRICK FORTRESS** - 2000 credits (heavy tank)
- **BLADE PHANTOM** - 3500 credits (speed demon)

### Upgrades (Per Vehicle)
- **Engine** - +15% max speed (500cr, 1000cr, 2000cr)
- **Handling** - +10% turn rate (400cr, 800cr, 1600cr)
- **Boost** - +0.5s duration (600cr, 1200cr, 2400cr)
- **Armor** - +20 HP (300cr, 600cr, 1200cr)

---

## Power-Ups

### Shield (Cyan)
- 4 seconds invulnerability
- Absorbs all damage

### EMP (Yellow)
- Disables all enemies within 25m
- Lasts 3 seconds

### Pulse Blaster (Red)
- Fires 5 forward shots
- 50 damage per shot

### Magnet (Green)
- Pulls shards within 15m
- Lasts 6 seconds

---

## Game Modes

### Time Trial
- Complete 3 laps as fast as possible
- Rewards based on finish time
- Best lap time tracked

### Grand Prix
- Race against 3 AI opponents
- Rewards based on final position
  - 1st: 1000 credits
  - 2nd: 600 credits
  - 3rd: 300 credits
  - 4th: 100 credits

### Police Chase
- Survive 5 minutes of police pursuit
- 3 police vehicles spawn and chase
- Use EMP to disable temporarily
- Rewards: 800cr + 100cr per police evaded

### Shard Hunt
- Collect 50 shards within 5 minutes
- Extra shards spawned around track
- Rewards: 20cr per shard + time bonus

---

## Tips & Tricks

### Perfect Start
- During countdown (3-2-1-GO), hold accelerator in the green window
- Too early = "TOO EARLY!" message, no bonus
- Perfect timing = +25 m/s speed boost

### Drift-to-Nitro
- Hold Space while turning to drift
- Drift longer for bigger nitro tiers:
  - **Blue** (1.0s) = +8 m/s
  - **Orange** (2.0s) = +15 m/s
  - **Pink** (3.5s) = +25 m/s
- Release Space to activate nitro

### Slipstream
- Stay behind another vehicle (within 15m)
- +5 m/s speed boost while drafting

### Anti-Gravity Boost
- Immune to hazards (lasers, pits)
- Increased speed (40 → 58 m/s)
- Lasts 3.5 seconds
- Collect Energy Cells to recharge (max 3 charges)

### Combat
- Ram drones at high speed (15+ m/s) to destroy instantly
- Use EMP before dangerous sections to clear path
- Police vehicles rubber-band - they'll catch up if you're too far ahead

---

## Troubleshooting

### "No vehicle data found"
→ Run **Tools > Neon Rush > Setup Everything**

### "Scene not found" error
→ Check Build Settings (File > Build Settings)
→ Ensure MainMenu and race scene are in Scenes list

### Vehicle not responding
→ Check if you're in the vehicle (press E to enter)
→ Check if game is paused (press ESC)

### No audio
→ Check Volume Mixer in Unity (Window > Audio > Audio Mixer)
→ AudioManager generates clips at runtime

### Upgrades not working
→ Upgrades apply when race starts
→ Make sure vehicle is selected in Garage

---

## Testing Checklist

### Must Test Before Build
- [ ] MainMenu loads correctly
- [ ] PLAY button starts race
- [ ] Vehicle drives smoothly
- [ ] Anti-gravity boost works (Shift key)
- [ ] Power-ups can be collected and used
- [ ] Race completes and shows results
- [ ] Garage UI opens and shows vehicles
- [ ] Can purchase upgrades
- [ ] Save/load works (buy upgrade, restart game, check if persisted)

### Known Issues
- AI uses placeholder waypoints (linear path)
- HUD missing lap/position indicators
- Track themes not applied to procedural track yet
- Garage scene not in build flow (use editor to test)

---

## Next Steps

### For Players
1. Complete tutorial race
2. Earn credits
3. Unlock BRICK or BLADE
4. Upgrade your favorite vehicle
5. Try all 4 game modes
6. Challenge friends' lap times

### For Developers
1. Read **BUILD_STATUS.md** for incomplete features
2. Check **PROGRESS.md** for TODO list
3. Review **PROJECT_MAP.md** for system overview
4. See **TEAM_CONTRIBUTIONS.md** for code ownership

---

## Support

### Unity Version
- Required: **Unity 6000.0.6f1**
- Render Pipeline: Built-in (not URP/HDRP)
- Input System: New Input System (installed)

### Platform Support
- ✅ Windows (tested in editor)
- ✅ macOS (tested in editor)
- ⚠️ Standalone builds untested (expected to work)
- ❌ Mobile/WebGL (not configured)

### Contact
For questions or issues:
- Check documentation files (README, BUILD_STATUS, PROJECT_MAP)
- Review PROGRESS.md TODO list
- Consult TEAM_CONTRIBUTIONS for feature owners
