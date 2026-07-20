using System.Collections.Generic;
using Roguelike.Core;
using UnityEngine;
using UnityEngine.InputSystem;

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
        
        [Header("Prefabs")]
        [SerializeField] EntityView player_prefab;
        [SerializeField] EntityView enemy_prefab;
        
        [Header("Data")]
        [SerializeField] EnemyDefinition[] enemy_definitions;
        
        [Header("Tuning")]
        [SerializeField] GenerationSettings generation_settings = new GenerationSettings();
        [SerializeField] DifficultySettings difficulty_settings = new DifficultySettings();

        [Tooltip("0 picks a random seed on each run.")] 
        [SerializeField] int seed = 0;
        
        public LevelData level { get; private set; }
        
        readonly List<EntityView> entity_views = new List<EntityView>();
        private bool game_over;
        private int last_seed;

        void Start()
        {
            GenerateLevel(seed != 0 ? seed : NewSeed());
        }

        void Update()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null) return;
            if (game_over)
            {
                if (keyboard.rKey.wasPressedThisFrame)
                {
                    GenerateLevel(NewSeed());
                    return;
                }
            }
            
            if (keyboard != null && Keyboard.current.f5Key.wasPressedThisFrame)
            {
                GenerateLevel(NewSeed());
                return;
            }
            HandlePlayerInput();
        }

        void HandlePlayerInput()
        {
            if (turn_manager == null || !turn_manager.AcceptsInput()) return;

            if (input_reader.WaitRequested())
            {
                turn_manager.SubmitPlayerAction(new WaitAction());
                return;
            }

            if (input_reader.TryGetDirection(out Vector2Int direction))
            {
                turn_manager.SubmitPlayerAction(new MoveOrAttackAction(level.player, direction));
            }
        }

        // ----------------------------------------------------------------------------------- level setup
        
        void GenerateLevel(int seed)
        {
            // Clear everything from the previous level
            ClearEntityViews();

            game_over = false;
            last_seed = seed;
            
            level = MapGenerator.Generate(generation_settings, seed, 1);
            level.MessageLogged += OnMessageLogged;
            
            map_renderer.Render(level.map);

            EntityState player = MapGenerator.CreatePlayer(level);
            EntityView player_view = SpawnView(player_prefab, player);

            SpawnEnemies(seed);
            
            // Set up main camera
            camera_follow.SetMapBounds(level.map.width, level.map.height);
            camera_follow.SetTarget(player_view.transform);
            // Snap the camera to the player instead of gliding from previous location
            SnapCameraToPlayer(player_view);

            turn_manager.Begin(level, new Rng(seed).Derive("ai"));
            turn_manager.player_died -= OnPlayerDied;
            turn_manager.player_died += OnPlayerDied;
            
            Debug.Log(
                $"Generated seed {seed}: {level.rooms.Count} rooms, " + $"{level.map.FloorTiles().Count} floor tiles."
            );
        }

        void SpawnEnemies(int seed)
        {
            if (enemy_definitions == null || enemy_definitions.Length == 0) return;

            // Ensures changing enemy logic doesn't shift map layout
            Rng rng = new Rng(seed).Derive("enemies");

            var allowed = new List<EnemyDefinition>();
            foreach (EnemyDefinition definition in enemy_definitions)
            {
                if (definition != null && definition.minimum_depth <= level.depth)
                {
                    allowed.Add(definition);
                }
            }

            if (allowed.Count == 0) return;

            int count = difficulty_settings.enemy_count_for_depth(level.depth);
            List<Vector2Int> tiles = SpawnPlacement.ChooseTiles(
                level, count, difficulty_settings.minimum_spawn_distance_from_player, rng);

            foreach (Vector2Int tile in tiles)
            {
                EnemyDefinition definition = allowed[rng.Range(0, allowed.Count)];
                EntityState enemy = definition.CreateEntity(tile, level.depth);
                
                level.AddEntity(enemy);

                EntityView view = SpawnView(enemy_prefab, enemy);
                view.SetSprite(definition.sprite);
            }
        }

        EntityView SpawnView(EntityView prefab, EntityState state)
        {
            EntityView view = Instantiate(prefab, entity_root);
            view.Bind(state);
            view.name = state.name;
            entity_views.Add(view);
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

        void ClearEntityViews()
        {
            if (level != null)
            {
                level.MessageLogged -= OnMessageLogged;
            }

            foreach (EntityView view in entity_views)
            {
                if (view != null) Destroy(view.gameObject);
            }
            entity_views.Clear();
        }

        void OnPlayerDied()
        {
            game_over = true;
            Debug.Log("Player died");
        }

        static void OnMessageLogged(string message) => Debug.Log(message);

        static int NewSeed() => System.Environment.TickCount & 0x7FFFFFFF;

        void OnGUI()
        {
            if (level == null) return;
            GUI.Label(
                new Rect(10, 10, 700, 20), 
            $"Seed: {level.seed}| Rooms: {level.rooms.Count} | Depth: {level.depth} | Turn {turn_manager.turn_count} | " + 
                $"HP {level.player.hp}/{level.player.max_hp} | " + 
                $"WASD = move | SPACE = wait | [F5] Regenerate"
            );
        }
    }    
}