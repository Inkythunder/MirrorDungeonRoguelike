using UnityEngine;

namespace Roguelike.Game
{
    /// <summary>
    /// Carries the player's menu choices into the game.
    /// </summary>
    public class RunConfig
    {
        public static bool has_seed { get; private set; }
        public static int seed { get; private set; }

        public static void SetSeed(int value)
        {
            seed = value;
            has_seed = true;
        }

        public static void ClearSeed()
        {
            seed = 0;
            has_seed = false;
        }
    }
}
