// Module: Race   Owner: Member 2   Purpose: ScriptableObject defining track theme colors and properties

using UnityEngine;

namespace NeonRush
{
    [CreateAssetMenu(fileName = "TrackTheme", menuName = "Neon Rush/Track Theme")]
    public class TrackTheme : ScriptableObject
    {
        [Header("Identity")]
        public string id = "NEON_DOWNTOWN";
        public string displayName = "Neon Downtown";
        
        [Header("Visual Theme")]
        public Color primaryNeonColor = new Color(0f, 0.9f, 1f); // Cyan
        public Color secondaryNeonColor = new Color(1f, 0.1f, 0.85f); // Magenta
        public Color fogColor = new Color(0.05f, 0.05f, 0.15f);
        
        [Header("Generation")]
        public int seed = 12345;
        public float hazardDensityMultiplier = 1.0f;
        public float lengthMultiplier = 1.0f;
        
        [Header("Economics")]
        public int unlockPrice = 0; // 0 = free starter track
    }
}
