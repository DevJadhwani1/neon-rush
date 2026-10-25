// Module: Progression   Owner: Member 3   Purpose: PlayerPrefs-based save/load with JSON serialization

using UnityEngine;
using System;
using System.Collections.Generic;

namespace NeonRush
{
    [Serializable]
    public class IntArrayEntry
    {
        public string key;
        public int[] value;
    }
    
    [Serializable]
    public class FloatEntry
    {
        public string key;
        public float value;
    }
    
    [Serializable]
    public class IntEntry
    {
        public string key;
        public int value;
    }
    
    [Serializable]
    public class StringListEntry
    {
        public string key;
        public List<string> value;
    }

    [Serializable]
    public class SaveData
    {
        public int credits = 0;
        public List<string> unlockedVehicles = new List<string>() { "VOLT" }; // Free starter car
        public List<string> unlockedTracks = new List<string>() { "NEON_DOWNTOWN" }; // Free starter track
        public List<IntArrayEntry> upgradelevels = new List<IntArrayEntry>(); // vehicleId -> [engine, handling, boost, armor]
        public List<FloatEntry> bestTimes = new List<FloatEntry>(); // trackId_mode -> time
        public List<IntEntry> trackStars = new List<IntEntry>(); // trackId_mode -> stars
        public List<StringListEntry> leaderboards = new List<StringListEntry>(); // trackId_mode -> ["AAA:45.2", "BBB:47.1", ...]
        public List<string> achievements = new List<string>();
        public bool[] tutorialSeen = new bool[10]; // flags for tutorial hints
        public float masterVolume = 1f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.8f;
        public int qualityLevel = 1; // 0=Low, 1=Medium, 2=High
        public bool cameraShake = true;
        public bool showFPS = false;
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public bool fullscreen = true;
        public bool vsync = true;
    }

    public static class SaveSystem
    {
        private const string SAVE_KEY = "NeonRushSaveData";
        private static SaveData cachedData = null;

        public static SaveData Load()
        {
            if (cachedData != null) return cachedData;
            
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                cachedData = JsonUtility.FromJson<SaveData>(json);
            }
            else
            {
                cachedData = new SaveData();
            }
            
            return cachedData;
        }

        public static void Save(SaveData data)
        {
            cachedData = data;
            string json = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public static void Save()
        {
            if (cachedData != null)
            {
                Save(cachedData);
            }
        }

        public static void ResetAll()
        {
            cachedData = new SaveData();
            Save(cachedData);
            ResultsScreenUI.ResetRewardSession();
            Debug.Log("Save data reset to default.");
        }

        // Convenience methods
        public static void AddCredits(int amount)
        {
            SaveData data = Load();
            data.credits += amount;
            Save(data);
        }

        public static bool SpendCredits(int amount)
        {
            SaveData data = Load();
            if (data.credits >= amount)
            {
                data.credits -= amount;
                Save(data);
                return true;
            }
            return false;
        }

        public static bool IsVehicleUnlocked(string vehicleId)
        {
            return Load().unlockedVehicles.Contains(vehicleId);
        }

        public static bool IsTrackUnlocked(string trackId)
        {
            return Load().unlockedTracks.Contains(trackId);
        }

        public static void UnlockVehicle(string vehicleId)
        {
            SaveData data = Load();
            if (!data.unlockedVehicles.Contains(vehicleId))
            {
                data.unlockedVehicles.Add(vehicleId);
                Save(data);
            }
        }

        public static void UnlockTrack(string trackId)
        {
            SaveData data = Load();
            if (!data.unlockedTracks.Contains(trackId))
            {
                data.unlockedTracks.Add(trackId);
                Save(data);
            }
        }

        public static int[] GetUpgradeLevels(string vehicleId)
        {
            SaveData data = Load();
            foreach (var entry in data.upgradelevels)
            {
                if (entry.key == vehicleId)
                {
                    return entry.value;
                }
            }
            return new int[4] { 0, 0, 0, 0 }; // engine, handling, boost, armor
        }

        public static void SetUpgradeLevel(string vehicleId, int upgradeIndex, int level)
        {
            SaveData data = Load();
            
            // Find existing entry
            IntArrayEntry existing = null;
            foreach (var entry in data.upgradelevels)
            {
                if (entry.key == vehicleId)
                {
                    existing = entry;
                    break;
                }
            }
            
            // Create new if not found
            if (existing == null)
            {
                existing = new IntArrayEntry { key = vehicleId, value = new int[4] };
                data.upgradelevels.Add(existing);
            }
            
            existing.value[upgradeIndex] = level;
            Save(data);
        }
        
        public static float GetBestTime(string key)
        {
            SaveData data = Load();
            foreach (var entry in data.bestTimes)
            {
                if (entry.key == key)
                {
                    return entry.value;
                }
            }
            return float.MaxValue;
        }
        
        public static void SetBestTime(string key, float time)
        {
            SaveData data = Load();
            
            // Find existing
            FloatEntry existing = null;
            foreach (var entry in data.bestTimes)
            {
                if (entry.key == key)
                {
                    existing = entry;
                    break;
                }
            }
            
            if (existing == null)
            {
                data.bestTimes.Add(new FloatEntry { key = key, value = time });
            }
            else
            {
                existing.value = time;
            }
            
            Save(data);
        }
        
        public static int GetTrackStars(string key)
        {
            SaveData data = Load();
            foreach (var entry in data.trackStars)
            {
                if (entry.key == key)
                {
                    return entry.value;
                }
            }
            return 0;
        }
        
        public static void SetTrackStars(string key, int stars)
        {
            SaveData data = Load();
            
            IntEntry existing = null;
            foreach (var entry in data.trackStars)
            {
                if (entry.key == key)
                {
                    existing = entry;
                    break;
                }
            }
            
            if (existing == null)
            {
                data.trackStars.Add(new IntEntry { key = key, value = stars });
            }
            else
            {
                existing.value = stars;
            }
            
            Save(data);
        }
        
        public static List<string> GetLeaderboard(string key)
        {
            SaveData data = Load();
            foreach (var entry in data.leaderboards)
            {
                if (entry.key == key)
                {
                    return entry.value;
                }
            }
            return new List<string>();
        }
        
        public static void SetLeaderboard(string key, List<string> entries)
        {
            SaveData data = Load();
            
            StringListEntry existing = null;
            foreach (var entry in data.leaderboards)
            {
                if (entry.key == key)
                {
                    existing = entry;
                    break;
                }
            }
            
            if (existing == null)
            {
                data.leaderboards.Add(new StringListEntry { key = key, value = entries });
            }
            else
            {
                existing.value = entries;
            }
            
            Save(data);
        }
    }
}
