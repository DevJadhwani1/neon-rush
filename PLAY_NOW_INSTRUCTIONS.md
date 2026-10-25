# PLAY NOW - Simple 3-Step Fix

## The Problem
The scene needs a Vehicle object and initialization scripts to work.

## The Solution (30 seconds)

### Step 1: Open the Race Scene
In Unity Project window:
- Navigate to **Assets** folder
- Find the file named **,.unity** (yes, just a comma and dot!)
- Double-click to open it

### Step 2: Add the Starter Script
In Unity Hierarchy window (left panel):
1. Right-click in empty space
2. Select **Create Empty**
3. Rename it to **"RaceStarter"**
4. With RaceStarter selected, in Inspector (right panel):
   - Click **Add Component**
   - Type **"CompleteRaceStarter"**
   - Click to add it

### Step 3: Press Play
- Click the **Play** button (▶) at the top center of Unity
- Wait 2-3 seconds for initialization
- **Vehicle should appear and you can drive!**

---

## Controls Once Playing

**Driving:**
- **W** - Accelerate
- **S** - Brake/Reverse
- **A** - Steer Left
- **D** - Steer Right
- **SPACE** - Drift
- **SHIFT** - Anti-Gravity Boost (you start with 1 FREE charge!)

**Camera:**
- Should follow automatically
- If stuck, try moving the vehicle with WASD

---

## What You Should See

1. **Top of screen:** "NEON RUSH: Grid Runners - TEST MODE" in cyan
2. **Below that:** Controls reminder
3. **Stats line:** Speed, Health, Boost Charges
4. **In game:** 
   - A cyan/blue cube (your vehicle)
   - A neon-lit race track
   - Glowing pickups and hazards

---

## If It STILL Doesn't Work

### Check 1: Is the scene loaded?
- Look at the top of Unity's Hierarchy panel
- Should say ",.unity" or scene name

### Check 2: Is the script added?
- Click on "RaceStarter" in Hierarchy
- Inspector should show "Complete Race Starter (Script)"

### Check 3: Check Console for errors
- Press **Ctrl+Shift+C** (Windows) or **Cmd+Shift+C** (Mac)
- Console window opens
- Look for red error messages
- Share them if you see any

### Check 4: Is Play mode active?
- The Play button (▶) at top should be **blue/highlighted** when active
- If it's not blue, click it again

---

## Expected Behavior

**When you press Play:**
1. Console shows: "=== COMPLETE RACE STARTER INITIALIZING ==="
2. Vehicle spawns at start position
3. Track appears (if TrackBuilder exists)
4. HUD shows at top of screen
5. You can drive immediately with WASD

**If vehicle doesn't spawn:**
- The script will create one automatically
- Wait 2-3 seconds
- Look for a cyan cube in the scene

---

## What to Test

Once it's working:

1. **Drive forward** (W key) - vehicle should accelerate
2. **Steer** (A/D keys) - vehicle should turn
3. **Drift** (hold SPACE while turning) - underglow changes color
4. **Boost** (SHIFT key) - vehicle hovers and speeds up
5. **Look around** - camera should follow smoothly

---

## Next Steps After It Works

1. Complete a lap around the track
2. Collect glowing items (shards, energy cells)
3. Avoid red hazards (lasers, pits)
4. Try drifting around corners
5. Use boost to jump over obstacles

Then you can:
- Run **Tools > Neon Rush > Setup Everything** for the full game
- Test the main menu and other features
- Try different game modes

---

## Still Having Issues?

**Common problems:**

**"Nothing happens when I press Play"**
→ Scene might not be saved. Go to File > Save Scene, then try Play again.

**"I see the HUD but no vehicle"**
→ Check Console (Ctrl+Shift+C) for errors about missing prefabs. The script should create one automatically.

**"Vehicle exists but won't move"**
→ Check if `isPlayerInVehicle` is true in Vehicle's VehicleController component during Play mode.

**"Camera is stuck or weird angle"**
→ Make sure Main Camera has SimpleCameraFollow component and playerVehicle is set.

**"Compilation errors"**
→ Wait for Unity to finish compiling all scripts (watch bottom-right corner progress bar).

---

## Debug Mode

If you want more information, open the Console (Ctrl/Cmd + Shift + C) BEFORE pressing Play.

You should see messages like:
```
[✓] Created GameManager
[✓] Spawned Vehicle from prefab
[✓] Enabled vehicle control
[✓] Camera follow script ready
=== RACE INITIALIZATION COMPLETE ===
```

If any of these are missing or show errors, share them for specific help.
