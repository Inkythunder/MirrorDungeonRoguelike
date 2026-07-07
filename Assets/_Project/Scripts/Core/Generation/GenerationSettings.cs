using System;

namespace Roguelike.Core
{
    /// <summary>
    /// Configure settings for level generation.
    /// </summary>
    [Serializable]
    public class GenerationSettings
    {
        public int width = 80;
        public int height = 60;
        public int minimum_partition_size = 14;
        public int maximum_depth = 4;
        public int room_margin = 2;
        public int walk_iterations = 10;
        public int walk_length = 25;
        public float minimum_room_fill = 0.45f;
        public int corridor_width = 3;
    }
}