// Module: Core   Owner: Member 3   Purpose: Feature flags to enable/disable game systems

namespace NeonRush
{
    public static class GameConfig
    {
        // === ENABLED FEATURES ===
        public static readonly bool CARS = true;
        public static readonly bool UPGRADES = true;
        public static readonly bool GARAGE = true;
        public static readonly bool THREE_TRACKS = true;
        public static readonly bool AI_RIVALS = true;
        public static readonly bool BOOST_PADS = true;
        public static readonly bool ANTI_GRAVITY_BOOST = true;
        public static readonly bool POWER_UP_SHIELD = true;
        public static readonly bool POWER_UP_EMP = true;
        public static readonly bool MAIN_MENU = true;
        public static readonly bool HUD = true;
        public static readonly bool RESULTS = true;
        public static readonly bool AUDIO = true;
        public static readonly bool VFX = true;
        public static readonly bool MINIMAP = true;
        
        // === DISABLED FEATURES ===
        public static readonly bool POLICE_CHASE_MODE = false;
        public static readonly bool SHARD_HUNT_MODE = false;
        public static readonly bool CAREER_MODE = false;
        public static readonly bool TIME_TRIAL_MODE = false;
        public static readonly bool COMBOS = false;
        public static readonly bool PERFECT_START = false;
        public static readonly bool SLIPSTREAM = false;
        public static readonly bool DRIFT_NITRO = false;
        public static readonly bool ACHIEVEMENTS = false;
        public static readonly bool LEADERBOARD = false;
        public static readonly bool REPLAY = false;
        public static readonly bool PODIUM = false;
        public static readonly bool SPLASH = false;
        public static readonly bool INTRO_FLYOVER = false;
        public static readonly bool TUTORIAL = false;
        public static readonly bool CITY_TRAFFIC = false;
        public static readonly bool ARMOR_SYSTEM = false;
        public static readonly bool POWER_UP_BLASTER = false;
        public static readonly bool POWER_UP_MAGNET = false;
        public static readonly bool WEATHER = false;
    }
}
