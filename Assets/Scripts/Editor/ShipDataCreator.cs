// Module: Editor   Owner: Member 1   Purpose: Editor utility to create ship data assets

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace NeonRush
{
    public class ShipDataCreator : EditorWindow
    {
        [MenuItem("Tools/Neon Rush/Create Default Ships")]
        static void CreateDefaultShips()
        {
            // GRIDLOCK - Player team (Cyan)
            CreateShipData("GRIDLOCK_Pulse", "GRIDLOCK", new Color(0f, 1f, 1f), 350f, 100f, 1.5f, 100f, 300f);
            
            // HELIX - Vex's team (Red)
            CreateShipData("HELIX_Viper", "HELIX", new Color(1f, 0.2f, 0.2f), 380f, 90f, 1.3f, 90f, 280f);
            
            // NOVA DYNAMICS (Purple)
            CreateShipData("NOVA_Phantom", "NOVA DYNAMICS", new Color(0.8f, 0.2f, 1f), 340f, 110f, 1.7f, 110f, 320f);
            
            // RAZOR WORKS (Yellow)
            CreateShipData("RAZOR_Blade", "RAZOR WORKS", new Color(1f, 1f, 0.2f), 360f, 95f, 1.6f, 95f, 290f);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("NeonRush: Created 4 team ship data assets in Assets/Data/Ships");
        }
        
        static void CreateShipData(string shipName, string teamName, Color teamColor, 
            float maxSpeed, float acceleration, float handling, float energyCapacity, float weight)
        {
            ShipData ship = ScriptableObject.CreateInstance<ShipData>();
            ship.shipName = shipName;
            ship.teamName = teamName;
            ship.teamColor = teamColor;
            ship.maxSpeed = maxSpeed;
            ship.acceleration = acceleration;
            ship.handling = handling;
            ship.energyCapacity = energyCapacity;
            ship.weight = weight;
            
            string path = $"Assets/Data/Ships/{shipName}.asset";
            AssetDatabase.CreateAsset(ship, path);
        }
    }
}
#endif
