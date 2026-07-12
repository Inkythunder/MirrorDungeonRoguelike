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

        public int depth;
        public int seed;

        public EntityState player;

        /// <summary>
        /// For logging messages to the screen
        /// </summary>
        public event Action<string> MessageLogged;

        readonly List<EntityState> all_entities = new List<EntityState>();
        readonly Dictionary<Vector2Int, EntityState> cell_occupancy = new Dictionary<Vector2Int, EntityState>();

        public IReadOnlyList<EntityState> AllEntities => all_entities;

        public void Log(string message) => MessageLogged?.Invoke(message);

        public void AddEntity(EntityState entity)
        {
            all_entities.Add(entity);
            cell_occupancy[entity.position] = entity;
        }

        public void RemoveEntity(EntityState entity)
        {
            all_entities.Remove(entity);

            // Only clear the cell if this entity is the one actually recorded there
            if (cell_occupancy.TryGetValue(entity.position, out EntityState occupant) && occupant == entity)
            {
                cell_occupancy.Remove(entity.position);
            }
        }

        public EntityState EntityAt(Vector2Int position) 
            => cell_occupancy.TryGetValue(position, out EntityState entity) ? entity : null;

        /// <summary>
        /// Changes position of an entity by removing it from a cell's occupancy
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="destination"></param>
        public void MoveEntity(EntityState entity, Vector2Int destination)
        {
            if (cell_occupancy.TryGetValue(entity.position, out EntityState occupant) && occupant == entity)
            {
                cell_occupancy.Remove(entity.position);
            }

            entity.position = destination;
            cell_occupancy[destination] = entity;
        }

        // Checks if cell is a floor and nothing is currently standing on it.
        public bool CanEnter(Vector2Int position)
            => map.IsWalkable(position) && EntityAt(position) == null;
    }
}