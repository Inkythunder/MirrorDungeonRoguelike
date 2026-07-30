using System;
using UnityEngine;

namespace Roguelike.Core
{
    [Serializable]
    public class DifficultySettings
    {
        public int base_enemy_count = 5;
        public int enemies_per_depth = 1;

        [Tooltip("Enemies never spawn closer than this to the player spawn")]
        public int minimum_spawn_distance_from_player = 12;

        public int enemy_count_for_depth(int depth)
        {
            return base_enemy_count + enemies_per_depth * (depth - 1);
        }

        [Tooltip("Items placed on each level.")]
        public int base_item_count = 4;
        public int items_per_depth = 0;

        public int item_count_for_depth(int depth)
        {
            return base_item_count + items_per_depth * (depth - 1);
        }
    }
    
}