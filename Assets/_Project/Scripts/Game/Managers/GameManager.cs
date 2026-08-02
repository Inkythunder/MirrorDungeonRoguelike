using System.Collections.Generic;
using Roguelike.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace Roguelike.Game
{
    public class GameManager : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] MapRenderer map_renderer;
        [SerializeField] TurnManager turn_manager;
        [SerializeField] PlayerInputReader input_reader;
        [SerializeField] CameraFollow camera_follow;
        [SerializeField] Transform entity_root;
        [SerializeField] Light2D global_light;
        [SerializeField] ScreenFlash screen_flash;
        
        [Header("Prefabs")]
        [SerializeField] EntityView player_prefab;
        [SerializeField] EntityView enemy_prefab;
        [SerializeField] SpriteRenderer stairs_prefab;
        [SerializeField] SpriteRenderer item_prefab;
        
        [Header("Data")]
        [SerializeField] EnemyDefinition[] enemy_definitions;
        [SerializeField] ItemDefinition[] item_definitions;
        
        [Header("Tuning")]
        [SerializeField] GenerationSettings generation_settings = new GenerationSettings();
        [SerializeField] DifficultySettings difficulty_settings = new DifficultySettings();

        [Tooltip("0 picks a random seed on each run.")] 
        [SerializeField] int seed = 0;
        
        public LevelData level { get; private set; }
        
        public bool game_over { get; private set; }
        int run_seed;
        private readonly Dictionary<Vector2Int, GameObject> item_views = 
            new Dictionary<Vector2Int, GameObject>();

        private Color world_tint = WorldPalette.primary_world.tint;

        void Start()
        {
            StartNewRun(seed != 0 ? seed : NewSeed());
        }

        void Update()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null) return;
            if (game_over)
            {
                if (keyboard.rKey.wasPressedThisFrame)
                {
                    StartNewRun(NewSeed());
                    return;
                }
            }
            
            if (keyboard != null && Keyboard.current.f5Key.wasPressedThisFrame)
            {
                StartNewRun(NewSeed());
                return;
            }
            HandlePlayerInput();
        }

        void HandlePlayerInput()
        {
            if (turn_manager == null || !turn_manager.AcceptsInput()) return;
            
            if (input_reader.PotionRequested())
            {
                turn_manager.SubmitPlayerAction(new DrinkPotionAction(level.player));
                return;
            }

            if (input_reader.InteractRequested())
            {
                if (level.in_mirror_world)
                {
                    level.Log("You cannot descend in the mirror world...");
                }
                else if (level.player.position == level.stairs_position)
                {
                    Descend();
                }
                else
                {
                    // For when the player tries to press [E] while not standing over stairs
                    level.Log("There are no stairs here.");
                }
            }

            if (input_reader.WorldSwapRequested())
            {
                turn_manager.SubmitPlayerAction(new ToggleWorldAction(level.player));
                return;
            }

            if (input_reader.WaitRequested())
            {
                turn_manager.SubmitPlayerAction(new WaitAction());
                return;
            }

            if (input_reader.TryGetDirection(out Vector2Int direction))
            {
                turn_manager.SubmitPlayerAction(new MoveOrAttackAction(level.player, direction));
                return;
            }
        }

        // --------------------------------------------------------------------------------------------- level setup
        void StartNewRun(int new_seed)
        {
            run_seed = new_seed;
            game_over = false;
            LoadDepth(1, carried_player: null);
        }

        // Called when the player presses [E] over the exit stairs which begins the next level.
        void Descend()
        {
            int next_depth = level.depth + 1;
            level.Log($"You descend to the depth {next_depth}...");
            LoadDepth(next_depth, level.player);
        }
        
        // Called to build the next level. Player stats are carried over from previous level.
        void LoadDepth(int depth, EntityState carried_player)
        {
            // Clear everything from the previous level
            TearDownLevel();

            level = MapGenerator.Generate(generation_settings, run_seed, depth);
            level.MessageLogged += OnMessageLogged;
            level.primary.item_removed += OnItemRemoved;
            level.mirror.item_removed += OnItemRemoved;
            level.WorldToggled += OnWorldToggled;
            
            map_renderer.Render(level.map);
            // Applying this here prevents mirror world tint from showing when descending to a primary world.
            ApplyWorldTint(level.in_mirror_world);

            EntityState player = MapGenerator.CreatePlayer(level, carried_player);
            EntityView player_view = SpawnView(player_prefab, player);

            SpawnEnemies(depth);
            SpawnItems(depth);
            SpawnStairs();
            
            // Set up main camera
            camera_follow.SetMapBounds(level.map.width, level.map.height);
            camera_follow.SetTarget(player_view.transform);
            // Snap the camera to the player instead of gliding from previous location
            SnapCameraToPlayer(player_view);

            turn_manager.Begin(level, new Rng(run_seed).Derive($"ai {depth}"));
            turn_manager.player_died -= OnPlayerDied;
            turn_manager.player_died += OnPlayerDied;
            
            Debug.Log(
                $"Generated seed {run_seed}: {level.rooms.Count} rooms, " + $"{level.map.FloorTiles().Count} floor tiles."
            );
        }

        void SpawnEnemies(int depth)
        {
            if (enemy_definitions == null || enemy_definitions.Length == 0) return;

            // Ensures changing enemy logic doesn't shift map layout
            Rng rng = new Rng(run_seed).Derive($"enemies {depth}");

            var allowed = new List<EnemyDefinition>();
            foreach (EnemyDefinition definition in enemy_definitions)
            {
                if (definition != null && definition.minimum_depth <= depth)
                {
                    allowed.Add(definition);
                }
            }

            if (allowed.Count == 0) return;

            int count = difficulty_settings.enemy_count_for_depth(depth);
            List<Vector2Int> tiles = SpawnPlacement.ChooseTiles(
                level, count, difficulty_settings.minimum_spawn_distance_from_player, rng);

            foreach (Vector2Int tile in tiles)
            {
                EnemyDefinition definition = allowed[rng.Range(0, allowed.Count)];
                EntityState enemy = definition.CreateEntity(tile, depth);
                
                level.AddEntity(enemy);

                EntityView view = SpawnView(enemy_prefab, enemy);
                view.SetSprite(definition.sprite);
            }
        }
        
        void SpawnItems(int depth)
        {
            if (item_definitions == null || item_definitions.Length == 0) return;

            // Ensures changing loot doesn't shift map layout
            Rng rng = new Rng(run_seed).Derive($"loot {depth}");

            var allowed = new List<ItemDefinition>();
            foreach (ItemDefinition definition in item_definitions)
            {
                if (definition != null && definition.minimum_depth <= depth)
                {
                    allowed.Add(definition);
                }
            }

            if (allowed.Count == 0) return;

            int count = difficulty_settings.item_count_for_depth(depth);
            List<Vector2Int> tiles = SpawnPlacement.ChooseTiles(
                level, count, 0, rng);

            foreach (Vector2Int tile in tiles)
            {
                ItemDefinition definition = allowed[rng.Range(0, allowed.Count)];
                ItemState item = definition.CreateItem();
                
                level.AddItem(tile, item);

                SpriteRenderer item_view = Instantiate(item_prefab, entity_root);
                item_view.transform.position = EntityView.CellToWorld(tile);
                item_view.sprite = definition.sprite;
                item_view.color = world_tint;
                item_view.name = item.name;

                item_views[tile] = item_view.gameObject;
            }
        }

        void SpawnStairs()
        {
            SpriteRenderer stairs = Instantiate(stairs_prefab, entity_root);
            stairs.transform.position = EntityView.CellToWorld(level.stairs_position);
            stairs.color = world_tint;
            stairs.name = "Stairs";
        }

        EntityView SpawnView(EntityView prefab, EntityState state)
        {
            EntityView view = Instantiate(prefab, entity_root);
            view.SetTint(world_tint);
            view.Bind(state);
            view.name = state.name;
            return view;
        }

        void SnapCameraToPlayer(EntityView player_view)
        {
            Transform camera_transform = camera_follow.transform;
            camera_transform.position = new Vector3(
                player_view.transform.position.x,
                player_view.transform.position.y,
                camera_follow.transform.position.z
            );
        }

        void TearDownLevel()
        {
            if (level != null)
            {
                level.MessageLogged -= OnMessageLogged;
                level.primary.item_removed -= OnItemRemoved;
                level.mirror.item_removed -= OnItemRemoved;
                level.WorldToggled -= OnWorldToggled;
            }

            item_views.Clear();

            // Everything drawn in the level is parented under 'entity_root' so one loop
            // clears all of it.
            // Destroying a child shifts the indices of the ones after it so we iterate backwards.
            for (int i = entity_root.childCount - 1; i >= 0; i--)
            {
                Destroy(entity_root.GetChild(i).gameObject);
            }
        }

        void OnPlayerDied()
        {
            game_over = true;
            Debug.Log("Player died");
        }

        static void OnMessageLogged(string message) => Debug.Log(message);

        void OnItemRemoved(Vector2Int position)
        {
            if (item_views.TryGetValue(position, out GameObject view))
            {
                item_views.Remove(position);
                Destroy(view);
            }
        }

        void OnWorldToggled(bool in_mirror_world)
        {
            // Flash first, then change world.
            screen_flash.Play(WorldPalette.For(in_mirror_world).tint);
            ApplyWorldTint(in_mirror_world);
        }

        static int NewSeed() => System.Environment.TickCount & 0x7FFFFFFF;

        void ApplyWorldTint(bool in_mirror_world)
        {
            WorldLook world_look = WorldPalette.For(in_mirror_world);
            world_tint = world_look.tint;
            
            map_renderer.SetWorldTint(world_tint);

            if (global_light != null)
            {
                global_light.color = world_look.light_colour;
                global_light.intensity = world_look.light_intensity;
            }
            
            foreach (SpriteRenderer renderer in entity_root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                renderer.color = world_tint;
            }
        }
    }    
}