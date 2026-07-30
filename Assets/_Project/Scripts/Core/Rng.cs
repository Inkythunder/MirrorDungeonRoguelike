using System.Collections.Generic;

namespace Roguelike.Core
{
    /// <summary>
    /// Seeded random number generator.
    /// </summary>
    public sealed class Rng
    {
        private readonly System.Random _random;
        
        public int Seed { get; }

        public Rng(int seed)
        {
            Seed = seed;
            _random = new System.Random(seed);
        }
        
        /// <summary>
        /// Random int.
        /// </summary>
        public int Range(int min_inclusive, int max_exclusive)
        {
            return _random.Next(min_inclusive, max_exclusive);
        }
        
        /// <summary>
        /// Random float between 0 and 1.
        /// </summary>
        public float Value => (float)_random.NextDouble();
        
        /// <summary>
        /// Returns true with the given probability. (0.25f returns true 25% of the time)
        /// </summary>
        public bool Chance(float probability) => _random.NextDouble() < probability;

        public T Pick<T>(IReadOnlyList<T> items) => items[Range(0, items.Count)];

        /// <summary>
        /// Creates a random number generator for each system of the game. For example, a random number generated
        /// for the floor plan using a seed, will always have the same random number even if we don't generate enemies
        /// on this run.
        /// </summary>
        public Rng Derive(string label) => new Rng(Seed ^ (int)Fnv1a(label));

        static uint Fnv1a(string text)
        {
            uint hash = 2166136261u;
            for (int i = 0; i < text.Length; i++)
            {
                hash ^= text[i];
                hash *= 16777619u;
            }

            return hash;
        }
    }
}
