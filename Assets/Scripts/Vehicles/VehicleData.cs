// Module: Vehicles   Owner: Member 1   Purpose: ScriptableObject defining vehicle stats and appearance

using UnityEngine;

namespace NeonRush
{
    public enum VehicleShape
    {
        Interceptor, // Balanced design
        Brick,       // Heavy, boxy
        Blade        // Sleek, aerodynamic
    }

    [CreateAssetMenu(fileName = "VehicleData", menuName = "Neon Rush/Vehicle Data")]
    public class VehicleData : ScriptableObject
    {
        [Header("Identity")]
        public string id = "VOLT";
        public string displayName = "Volt Interceptor";
        public string description = "Balanced all-rounder";
        
        [Header("Appearance")]
        public VehicleShape shapePreset = VehicleShape.Interceptor;
        public Color bodyColor = new Color(0f, 0.9f, 1f); // Cyan
        public Color glowColor = new Color(0f, 0.9f, 1f);
        
        [Header("Base Stats")]
        public float maxSpeed = 40f;
        public float acceleration = 48f;
        public float turnRate = 115f;
        public float boostPower = 1.0f; // Multiplier
        public float boostDuration = 3.5f;
        public int maxArmor = 100;
        
        [Header("Economics")]
        public int price = 0; // 0 = free starter vehicle
    }
}
