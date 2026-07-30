using System.Collections.Generic;
using UnityEngine;

namespace Roguelike.Core
{
    public static class MapGenerator
    {
        public static LevelData Generate(GenerationSettings settings, int seed, int depth)
        {
            // Each depth generates a new map layout and each seed produces the same layout at
            // the same depths.
            var layout_rng = new Rng(seed).Derive($"layout depth {depth}");

            var level = new LevelData
            {
                seed = seed,
                depth = depth,
                map = new DungeonMap(settings.width, settings.height)
            };

            // Partition the given space.
            var area = new RectInt(1, 1, settings.width - 2, settings.height - 2);
            level.bsp_root = BspPartitioner.Partition(
                area, 
                settings.minimum_partition_size, 
                settings.maximum_depth, 
                layout_rng
            );

            // Carve a cave using random walk in each leaf of the BSP tree.
            foreach (var leaf in level.bsp_root.Leaves())
            {
                RectInt room_area = Inset(leaf.bounds, settings.room_margin);
                if (room_area.width < 4 || room_area.height < 4) continue;

                HashSet<Vector2Int> tiles = RandomWalkCarver.Carve(
                    room_area,
                    settings.walk_iterations,
                    settings.walk_length,
                    settings.minimum_room_fill,
                    layout_rng
                );

                var room = new Room
                {
                    bounds = room_area
                };

                foreach (var tile in tiles)
                {
                    level.map.Carve(tile);
                    room.floor_tiles.Add(tile);
                }

                room.centre = ClosestTileTo(GeometricCentre(room_area), room.floor_tiles);
                level.rooms.Add(room);
            }
            
            // Connect each cave with an L-shaped corridor.
            var centres = new List<Vector2Int>();
            foreach (var room in level.rooms)
            {
                centres.Add(room.centre);
            }

            HashSet<Vector2Int> corridors = CorridorConnector.Connect(centres, layout_rng);
            foreach (var tile in corridors)
            {
                CarveBrush(level.map, tile, settings.corridor_width);
            }

            // Discard any isolated rooms.
            KeepLargestRegion(level);
            
            // Set the player and exit stairs spawn points
            ChooseSpawnAndStairs(level, layout_rng);

            return level;
        }

        static RectInt Inset(RectInt bounds, int room_margin)
        {
            return new RectInt(
                bounds.xMin + room_margin,
                bounds.yMin + room_margin,
                Mathf.Max(0, bounds.width - room_margin * 2),
                Mathf.Max(0, bounds.height - room_margin * 2)
            );
        }

        static Vector2Int GeometricCentre(RectInt room_area)
        {
            return new Vector2Int(
                room_area.xMin + room_area.width / 2, 
                room_area.yMin + room_area.height / 2
            );
        }

        static Vector2Int ClosestTileTo(Vector2Int room_centre, List<Vector2Int> tiles)
        {
            var closest_tile = tiles[0];
            int shortest_distance = int.MaxValue;
            foreach (var tile in tiles)
            {
                int dx = tile.x - room_centre.x;
                int dy = tile.y - room_centre.y;
                int distance = dx * dx + dy * dy;
                if (distance < shortest_distance)
                {
                    shortest_distance = distance;
                    closest_tile = tile;
                }
            }
            return closest_tile;
        }

        static void CarveBrush(DungeonMap map, Vector2Int centre, int width)
        {
            int half = width / 2;
            for (int dx = -half; dx <= half; dx++)
            {
                for (int dy = -half; dy <= half; dy++)
                {
                    var point = new Vector2Int(centre.x + dx, centre.y + dy);
                    if (point.x <= 0 || 
                        point.y <= 0 || 
                        point.x >= map.width - 1 || 
                        point.y >= map.height - 1)
                    {
                        continue;
                    }
                    map.Carve(point);
                }
            }
        }

        static void KeepLargestRegion(LevelData level)
        {
            List<Vector2Int> all_floor = level.map.FloorTiles();
            var unvisited = new HashSet<Vector2Int>(all_floor);
            HashSet<Vector2Int> largest = null;

            while (unvisited.Count > 0)
            {
                Vector2Int seed_tile = default;
                foreach (var tile in unvisited)
                {
                    seed_tile = tile;
                    break;
                }

                HashSet<Vector2Int> region = level.map.ReachableFrom(seed_tile);
                if (largest == null || region.Count > largest.Count)
                {
                    largest = region;
                }

                foreach (var tile in region)
                {
                    unvisited.Remove(tile);
                }
            }

            if (largest == null) return;

            foreach (var tile in all_floor)
            {
                if (!largest.Contains(tile))
                {
                    level.map[tile] = TileType.Wall;
                }
            }

            // Discard rooms that got filled in and re-paint the centres of the remaining rooms.
            for (int i = level.rooms.Count -1; i >= 0; i--)
            {
                Room room = level.rooms[i];
                room.floor_tiles.RemoveAll(tile => !largest.Contains(tile));

                if (room.floor_tiles.Count == 0)
                {
                    level.rooms.RemoveAt(i);
                    continue;
                }

                room.centre = ClosestTileTo(GeometricCentre(room.bounds), room.floor_tiles);
            }
        }

        static void ChooseSpawnAndStairs(LevelData level, Rng rng)
        {
            if (level.rooms.Count == 0)
            {
                // Theoretically unreachable but game will always start with a valid spawn point.
                var fallback = new Vector2Int(level.map.width / 2, level.map.height / 2);
                level.map.Carve(fallback);
                level.player_spawn = level.stairs_position = fallback;
                return;
            }

            Room spawn_room = level.rooms[rng.Range(0, level.rooms.Count)];
            level.player_spawn = spawn_room.centre;
            
            // Stairs spawn in room farthest from spawn room
            Room farthest_room = spawn_room;
            int farthest_distance = -1;
            foreach (Room room in level.rooms)
            {
                int distance = Mathf.Abs(room.centre.x - level.player_spawn.x) +
                               Mathf.Abs(room.centre.y - level.player_spawn.y);
                if (distance > farthest_distance)
                {
                    farthest_distance = distance;
                    farthest_room = room;
                }
            }
            
            level.stairs_position = farthest_room.centre;
        }

        public static EntityState CreatePlayer(LevelData level, EntityState carried = null)
        {
            var player = new EntityState
            {
                name = "Player 1",
                faction = Faction.Player,
                position = level.player_spawn,
                max_hp = 20,
                hp = 20,
                attack = 4,
                defence = 1
            };

            if (carried != null)
            {
                // Descending carries over stats and inventory from previous level but position resets.
                player.hp = carried.hp;
                player.max_hp = carried.max_hp;
                player.attack = carried.attack;
                player.defence = carried.defence;
                player.weapon_name = carried.weapon_name;
                player.weapon_power = carried.weapon_power;
                player.armour_power = carried.armour_power;
                player.potions = carried.potions;
            }
            
            level.AddEntity(player);
            level.player = player;
            return player;
        }
    }
}