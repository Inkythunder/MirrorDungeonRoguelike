using System.Collections.Generic;
using System.Linq;
using Roguelike.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Roguelike.Game
{
    public class MapRenderer : MonoBehaviour
    {
        [SerializeField] Tilemap floor_tilemap;
        [SerializeField] Tilemap wall_tilemap;
        [SerializeField] TileBase floor_tile;
        
        // 47 wall sprites we sliced from our tileset.
        [SerializeField] Sprite[] wall_sprites;
        
        TileBase[] wall_tiles_by_index;

        void Awake()
        {
            if (wall_sprites == null || wall_sprites.Length != 47)
            {
                Debug.LogError($"Expected 47 wall sprites, got {wall_sprites?.Length ?? 0} ");
                return;
            }

            var sorted = wall_sprites
                .OrderBy(sprite => int.Parse(sprite.name.Substring(sprite.name.LastIndexOf('_') + 1)))
                .ToArray();

            wall_tiles_by_index = new TileBase[sorted.Length];
            for (int i = 0; i < sorted.Length; i++)
            {
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sorted[i];
                wall_tiles_by_index[i] = tile;
            }
        }

        public void Render(DungeonMap map)
        {
            Clear();

            var bounds = new BoundsInt(0, 0, 0, map.width, map.height, 1);
            var floor_tiles = new TileBase[map.width * map.height];
            var wall_tiles = new TileBase[map.width * map.height];

            for (int y = 0; y < map.height; y++)
            {
                for (int x = 0; x < map.width; x++)
                {
                    int index = y * map.width + x;
                    var position = new Vector2Int(x, y);
                    
                    // Floor covers the entire map.
                    // This allows floor to cover wall-to-wall and avoid gaps on vertical walls.
                    floor_tiles[index] = floor_tile;
                    
                    if (map[position] != TileType.Floor)
                        wall_tiles[index] = PickWallTile(map, position);
                }
            }
            
            floor_tilemap.SetTilesBlock(bounds, floor_tiles);
            wall_tilemap.SetTilesBlock(bounds, wall_tiles);
        }
        
        // One bit per neighbouring cell names after compass directions.
        const int N = 1, E = 2, S = 4, W = 8, NE = 16, SE = 32, SW = 64, NW = 128;
        
        /// <summary>
        /// Chooses which sprite a wall tile should use.
        /// Wall tiles have a light trim of bricks on the top of the sprite so the correct sprite depends
        /// on which of the 8 neighbouring tiles are also walls.
        /// The neighbouring tiles are represented as bits, and we use bitwise operations to do this
        /// efficiently.
        /// </summary>
        TileBase PickWallTile(DungeonMap map, Vector2Int position)
        {
            
            // Out of bounds reads return Wall
            bool IsWall(int dx, int dy)
            {
                return map[new Vector2Int(position.x + dx, position.y + dy)] != TileType.Floor;
            }

            int wall_neighbours = 0;
            if (IsWall(0, 1)) wall_neighbours |= N;
            if (IsWall(1, 0)) wall_neighbours |= E;
            if (IsWall(0, -1)) wall_neighbours |= S;
            if (IsWall(-1, 0)) wall_neighbours |= W;

            // A diagonal neighbour only affects which sprite we use when both walls beside it are present too.
            // Otherwise, the diagonal cell isn't touching any wall we draw, and is ignored.
            // This reduces the set of possible neighbours from 256 to 47.
            if (IsWall(1, 1) && (wall_neighbours & (N | E)) == (N | E)) wall_neighbours |= NE;
            if (IsWall(1, -1) && (wall_neighbours & (S | E)) == (S | E)) wall_neighbours |= SE;
            if (IsWall(-1, -1) && (wall_neighbours & (S | W)) == (S | W)) wall_neighbours |= SW;
            if (IsWall(-1, 1) && (wall_neighbours & (N | W)) == (N | W)) wall_neighbours |= NW;

            return wall_tiles_by_index[SpriteForShape[wall_neighbours]];
        }
        
        // Maps the number we generated in PickWallTile to the sprite number for that wall tile.
        // e.g. "N | E" = the cells north and east of this wall are also walls and everything else is floor.
        static readonly Dictionary<int, int> SpriteForShape = new Dictionary<int, int>
        {
            // Pillar
            {0, 35},
            
            // Dead ends
            { N, 23 },
            { E, 36 },
            { S, 0 },
            { W, 38 },
            
            // Straight runs
            { N | S, 12 },
            { E | W, 37 },
            
            // Corners
            { N | E, 24 },
            { E | S, 1 },
            { S | W, 3 },
            { N | W, 26 },
            
            // Corners - with solid diagonal
            { N | E | NE, 43 },
            { E | S | SE, 8 },
            { S | W | SW, 11 },
            { N | W | NW, 46 },
            
            // T-Junctions
            // Variations: open and solid corners
            { N | E | S, 13 },
            { N | E | S | NE, 27 },
            { N | E | S | SE, 16 },
            { N | E | S | NE | SE, 20 },
            
            { E | S | W, 2 },
            { E | S | W | SE, 5 },
            { E | S | W | SW, 6 },
            { E | S | W | SE | SW, 10},
            
            { N | S | W, 15 },
            { N | S | W | SW, 19 },
            { N | S | W | NW, 30 },
            { N | S | W | SW | NW, 34 },
            
            { N | E | W, 25 },
            { N | E | W | NE, 40 },
            { N | E | W | NW, 41 },
            { N | E | W | NE | NW, 44},
            
            // Crosses
            // Variations: open and solid corners
            { N | E | S | W, 14 },
            { N | E | S | W | NE, 7 },
            { N | E | S | W | SE, 42 },
            { N | E | S | W | SW, 39 },
            { N | E | S | W | NW, 4 },
            { N | E | S | W | NE | SE, 31 },
            { N | E | S | W | NE | SW, 21 },
            { N | E | S | W | NE | NW, 45 },
            { N | E | S | W | SE | SW, 9 },
            { N | E | S | W | SE | NW, 33 },
            { N | E | S | W | SW | NW, 22 },
            { N | E | S | W | NE | SE | SW, 17 },
            { N | E | S | W | NE | SE | NW, 28 },
            { N | E | S | W | NE | SW | NW, 29 },
            { N | E | S | W | SE | SW | NW, 18 },
            { N | E | S | W | NE | SE | SW | NW, 32 },
        };
        
        public void Clear()
        {
            floor_tilemap.ClearAllTiles();
            wall_tilemap.ClearAllTiles();
        }
    }
}
