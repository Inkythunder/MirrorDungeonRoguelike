using UnityEngine;

namespace Roguelike.Game
{
    /// <summary>
    /// Tints applied to the tilemaps and entity sprites to distinguish the primary and mirror worlds.
    /// </summary>

    public readonly struct WorldLook
    {
        public readonly Color tint;
        public readonly Color light_colour;
        public readonly float light_intensity;

        public WorldLook(Color tint, Color light_colour, float light_intensity)
        {
            this.tint = tint;
            this.light_colour = light_colour;
            this.light_intensity = light_intensity;
        }
    }
    
    public static class WorldPalette
    {
        public static readonly WorldLook primary_world = new WorldLook(
            Color.white, Color.white, 1f);

        public static readonly WorldLook mirror_world = new WorldLook(
            new Color(0.55f, 0.70f, 0.95f), new Color(0.75f, 0.85f, 1.00f), 0.65f);

        public static WorldLook For(bool in_mirror_world) => in_mirror_world ? mirror_world : primary_world;

    }
}