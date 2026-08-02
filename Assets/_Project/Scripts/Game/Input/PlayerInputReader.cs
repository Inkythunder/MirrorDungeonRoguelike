using Roguelike.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Roguelike.Game
{
    /// <summary>
    /// Ensures we have a maximum of one movement request per frame and auto-repeat if a direction is being held.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        [Tooltip("How long a key must be held for auto-repeat")] 
        [SerializeField] float repeat_delay = 0.25f;
        
        [Tooltip("Time between auto-repeats once started.")] 
        [SerializeField] float repeat_interval = 0.11f;

        Vector2Int last_direction = Vector2Int.zero;
        float next_repeat_time;

        public bool WaitRequested()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return false;

            return keyboard.spaceKey.wasPressedThisFrame;
        }

        public bool InteractRequested()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return false;

            return keyboard.eKey.wasPressedThisFrame;
        }
        
        public bool WorldSwapRequested()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return false;

            return keyboard.qKey.wasPressedThisFrame;
        }

        public bool PotionRequested()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return false;

            return keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame;
        }

        public bool TryGetDirection(out Vector2Int direction)
        {
            direction = Vector2Int.zero;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return false;

            Vector2Int held = ReadHeldDirection(keyboard);

            if (held == Vector2Int.zero)
            {
                last_direction = Vector2Int.zero;
                return false;
            }
            
            // New direction fires immediately
            if (held != last_direction)
            {
                last_direction = held;
                next_repeat_time = Time.time + repeat_delay;
                direction = held;
                return true;
            }
            
            // Currently held
            if (Time.time >= next_repeat_time)
            {
                next_repeat_time = Time.time + repeat_interval;
                direction = held;
                return true;
            }

            return false;
        }

        static Vector2Int ReadHeldDirection(Keyboard keyboard)
        {
            // WASD as well as arrow-keys for controlling movement.
            // Movement is 4-directional only, cannot move diagonally.
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) return Direction.Up;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) return Direction.Left;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) return Direction.Down;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) return Direction.Right;

            return Vector2Int.zero;
        }
    }
}
