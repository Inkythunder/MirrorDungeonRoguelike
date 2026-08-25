using System;
using System.Collections.Generic;
using UnityEngine;

namespace Roguelike.Core
{
    
    public sealed class Room
    {
        public RectInt bounds;
        public Vector2Int centre;
        public List<Vector2Int> floor_tiles = new List<Vector2Int>();
    }

    public sealed class LevelData
    {
        public DungeonMap map;
        public BspNode bsp_root;
        public List<Room> rooms = new List<Room>();
        public Vector2Int player_spawn;
        public Vector2Int stairs_position;
        public bool stairs_locked = true;
        public Vector2Int glyph_position;

        public int depth;
        public int seed;
        public EntityState player;

        public WorldContents primary = new WorldContents();
        public WorldContents mirror = new WorldContents();
        public bool in_mirror_world;

        // Store a list of all of the hostile enemies that became petrified upon entering the mirror world.
        public readonly List<EntityState> petrified_enemies = new List<EntityState>();
        
        // Turns the player has spent in the mirror world on this level. Does not reset when they step out.
        // Hopping back and forth does not prevent guardians from waking.
        public int mirror_turns;
        
        // How many guardians have woken up.
        public int guardians_awakened;

        // How many turns it takes to wake up another guardian.
        public int turns_per_awakening = 50;

        // 'active_world' means "the world we're in now". It is 'mirror' if we're in the mirror world
        // or 'primary' if we're not.
        public WorldContents active_world => in_mirror_world ? mirror : primary;

        // Forwarders so we can call these functions in WorldContents on the currently active world.
        // Previously there was only one world so these were all defined here in LevelData.
        public EntityState EntityAt(Vector2Int position) => active_world.EntityAt(position);
        public void AddEntity(EntityState entity) => active_world.AddEntity(entity);
        public void RemoveEntity(EntityState entity) => active_world.RemoveEntity(entity);
        public void MoveEntity(EntityState entity, Vector2Int to) => active_world.MoveEntity(entity, to);
        public ItemState ItemAt(Vector2Int position) => active_world.ItemAt(position);
        public void AddItem(Vector2Int position, ItemState item) => active_world.AddItem(position, item);
        public void RemoveItemAt(Vector2Int position) => active_world.RemoveItemAt(position);
        public IReadOnlyList<EntityState> _all_entities => active_world.AllEntities;
        // Checks if cell is a floor and nothing is currently standing on it.
        public bool CanEnter(Vector2Int position) => map.IsWalkable(position) && EntityAt(position) == null;
        
        
        /// <summary>
        /// For logging messages to the screen
        /// </summary>
        public event Action<string> MessageLogged;
        public void Log(string message) => MessageLogged?.Invoke(message);

        /// <summary>
        /// Will allow us to toggle between mirror and primary worlds.
        /// </summary>
        public event Action<bool> WorldToggled;

        /// <summary>
        /// Where damage landed and how much.
        /// </summary>
        public event Action<Vector2Int, int> DamageDealt;
        public void ReportDamage(Vector2Int position, int amount) => DamageDealt?.Invoke(position, amount);

        /// <summary>
        /// To keep track of how many enemies have been killed this run.
        /// </summary>
        public event Action<EntityState> EnemyKilled;
        public void CountKill(EntityState enemy) => EnemyKilled?.Invoke(enemy);

        public void ToggleWorld()
        {
            in_mirror_world = !in_mirror_world;
            WorldToggled?.Invoke(in_mirror_world);
        }

        /// <summary>
        /// Stairs unlock when the key enemy is placed on the glyph. Either as a statue
        /// or by walking onto it by itself.
        /// Stairs remain unlocked for the remainder of the level because the statue wanders off.
        /// </summary>
        public void CheckGlyph()
        {
            if (!stairs_locked) return;

            EntityState occupant = mirror.EntityAt(glyph_position);
            if (occupant == null || !occupant.is_key) return;

            stairs_locked = false;
            Log("The glyph flares and the stairs are unlocked...");
        }
    }
}