// Module: Data   Owner: Member 1   Purpose: ScriptableObject for ship stats and properties

using UnityEngine;

namespace NeonRush
{
    [CreateAssetMenu(fileName = "NewShip", menuName = "Neon Rush/Ship Data")]
    public class ShipData : ScriptableObject
    {
        [Header("Team Info")]
        public string shipName = "Prototype";
        public string teamName = "GRIDLOCK";
        public Color teamColor = Color.cyan;
        
        [Header("Performance Stats")]
        [Range(200f, 450f)]
        public float maxSpeed = 350f;
        
        [Range(50f, 200f)]
        public float acceleration = 100f;
        
        [Range(0.5f, 3f)]
        public float handling = 1.5f;
        
        [Range(50f, 200f)]
        public float energyCapacity = 100f;
        
        [Range(200f, 500f)]
        public float weight = 300f;
        
        [Header("Visual")]
        public Mesh shipMesh;
        public Material shipMaterial;
        
        [Header("Audio")]
        public AudioClip engineSound;
        public AudioClip boostSound;
    }
}
