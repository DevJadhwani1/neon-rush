// Module: Core   Owner: Member 1   Purpose: Handle player input and feed to ship controller

using UnityEngine;

namespace NeonRush
{
    public class ShipInput : MonoBehaviour
    {
        private ShipController shipController;
        
        void Awake()
        {
            shipController = GetComponent<ShipController>();
        }
        
        void Update()
        {
            if (shipController == null) return;
            
            float steering = 0f;
            float airBrake = 0f;
            bool boost = false;
            
            // Keyboard/Gamepad steering (Horizontal axis: A/D or Left/Right arrows)
            steering = Input.GetAxis("Horizontal");
            
            // Air brakes (Q/E keys)
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
            {
                airBrake = 1f;
            }
            
            // Boost (Space/Shift)
            boost = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift);
            
            shipController.SetInput(steering, airBrake, boost);
        }
    }
}
