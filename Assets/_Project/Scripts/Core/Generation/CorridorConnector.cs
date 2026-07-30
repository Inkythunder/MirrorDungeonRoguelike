using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Roguelike.Core
{
    public static class CorridorConnector
    {

        public static HashSet<Vector2Int> Connect(List<Vector2Int> room_centres, Rng rng)
        {
            var corridors = new HashSet<Vector2Int>();
            if (room_centres.Count < 2) return corridors;
            
            var remaining_rooms = new List<Vector2Int>(room_centres);

            var current_room = remaining_rooms[rng.Range(0, remaining_rooms.Count)];
            remaining_rooms.Remove(current_room);

            while (remaining_rooms.Count > 0)
            {
                var closest_room = FindClosest(current_room, remaining_rooms);
                remaining_rooms.Remove(closest_room);
                CarveCorridor(current_room, closest_room, corridors, rng);
                current_room = closest_room;
            }

            return corridors;
        }

        static Vector2Int FindClosest(Vector2Int source, List<Vector2Int> potential_destinations)
        {
            var closest_destination = potential_destinations[0];
            int shortest_distance = int.MaxValue;

            foreach (var destination in potential_destinations)
            {
                int dx = destination.x - source.x;
                int dy = destination.y - source.y;
                int distance = dx * dx + dy * dy;

                if (distance < shortest_distance)
                {
                    shortest_distance = distance;
                    closest_destination = destination;
                }
            }

            return closest_destination;
        }

        /// <summary>
        /// Carves an L shaped corridor. Randomly chooses to dig the X or Y axis first fully then digs the other.
        /// </summary>
        static void CarveCorridor(
            Vector2Int source, Vector2Int destination, HashSet<Vector2Int> corridor, Rng rng)
        {
            var position = source;
            corridor.Add(position);

            // Randomly chooses to carve the corridor on the X or Y axis first.
            if (rng.Chance(0.5f))
            {
                StepTowardsX(ref position, destination, corridor);
                StepTowardsY(ref position, destination, corridor);
            }
            else
            {
                StepTowardsY(ref position, destination, corridor);
                StepTowardsX(ref position, destination, corridor);
            }
        }

        static void StepTowardsX(ref Vector2Int position, Vector2Int destination, HashSet<Vector2Int> corridor)
        {
            while (position.x != destination.x)
            {
                position.x += position.x < destination.x ? 1 : -1;
                corridor.Add(position);
            }
        }
        
        static void StepTowardsY(ref Vector2Int position, Vector2Int destination, HashSet<Vector2Int> corridor)
        {
            while (position.y != destination.y)
            {
                position.y += position.y < destination.y ? 1 : -1;
                corridor.Add(position);
            }
        } 
    }
}