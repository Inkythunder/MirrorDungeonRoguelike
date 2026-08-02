using System;
using System.Collections.Generic;
using UnityEngine;

namespace Roguelike.Core
{
    // Contains all the entities, the occupancy index, and items of a world.
    // Both worlds share a single Dungeonmap.
    public sealed class WorldContents
    {
        public event Action<Vector2Int> item_removed;
        
        readonly List<EntityState> all_entities = new List<EntityState>();
        readonly Dictionary<Vector2Int, EntityState> cell_occupancy = new Dictionary<Vector2Int, EntityState>();
        readonly Dictionary<Vector2Int, ItemState> items = new Dictionary<Vector2Int, ItemState>();

        public IReadOnlyList<EntityState> AllEntities => all_entities;
        
        
        // ----- Add, remove, and track entities on the map -----
        
        /// <summary>
        /// Called when we want to add the player or an enemy to the world.
        /// </summary>
        public void AddEntity(EntityState entity)
        {
            all_entities.Add(entity);
            cell_occupancy[entity.position] = entity;
        }
        
        /// <summary>
        /// Call this when an enemy or player dies to remove them from the world.
        /// </summary>
        public void RemoveEntity(EntityState entity)
        {
            all_entities.Remove(entity);

            // Only clear the cell if this entity is the one actually recorded there.
            if (cell_occupancy.TryGetValue(entity.position, out EntityState occupant) && occupant == entity)
            {
                cell_occupancy.Remove(entity.position);
            }
        }
        
        /// <summary>
        ///  Get the entity occupying a given cell position.
        /// </summary>
        public EntityState EntityAt(Vector2Int position)
        {
            return cell_occupancy.GetValueOrDefault(position);
        }
        
        /// <summary>
        /// Changes position of an entity by removing it from a cell's occupancy.
        /// </summary>
        public void MoveEntity(EntityState entity, Vector2Int destination)
        {
            if (cell_occupancy.TryGetValue(entity.position, out EntityState occupant) && occupant == entity)
            {
                cell_occupancy.Remove(entity.position);
            }

            entity.position = destination;
            cell_occupancy[destination] = entity;
        }
        
        
        // ----- Add, remove, and track items on the map -----
        
        /// <summary>
        /// Add an item (potion, weapon, armour) to the world at a given position.
        /// </summary>
        public void AddItem(Vector2Int position, ItemState item)
        {
            items[position] = item;
        }

        /// <summary>
        /// Get the item that exists at a given position
        /// </summary>
        public ItemState ItemAt(Vector2Int position)
        {
            return items.TryGetValue(position, out ItemState item) ? item : null;
        }

        /// <summary>
        /// Called when the player equips an item to remove it from the world.
        /// </summary>
        public void RemoveItemAt(Vector2Int position)
        {
            if (items.Remove(position)) item_removed?.Invoke(position);
        }
    }
}
