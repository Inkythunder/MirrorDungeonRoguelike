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

        [Header("Generation")] 
        [SerializeField] GenerationSettings generation_settings = new GenerationSettings();

        [Tooltip("0 picks a random seed on each run.")] 
        [SerializeField] int seed = 0;
        
        public LevelData level { get; private set; }
        
        readonly List<EntityView> entity_views = new List<EntityView>();

        void Start()
        {
            GenerateLevel(seed != 0 ? seed : NewSeed());
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame)
            {
                GenerateLevel(NewSeed());
            }
            HandlePlayerInput();
        }

        void HandlePlayerInput()
        {
            if (turn_manager == null || !turn_manager.AcceptsInput) return;

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
            ClearEntityViews();
            
            level = MapGenerator.Generate(generation_settings, seed, 1);
            level.MessageLogged += OnMessageLogged;
            
            map_renderer.Render(level.map);

            EntityState player = MapGenerator.CreatePlayer(level);
            EntityView player_view = SpawnView(player_prefab, player);
            
            camera_follow.SetMapBounds(level.map.width, level.map.height);
            camera_follow.SetTarget(player_view.transform);
            
            // Snap the camera to the player instead of gliding from previous location
            camera_follow.transform.position = new Vector3(
                player_view.transform.position.x,
                player_view.transform.position.y,
                camera_follow.transform.position.z
            );
            
            turn_manager.Begin(level);
            
            Debug.Log(
                $"Generated seed {seed}: {level.rooms.Count} rooms, " + $"{level.map.FloorTiles().Count} floor tiles."
            );
        }

        EntityView SpawnView(EntityView prefab, EntityState state)
        {
            EntityView view = Instantiate(prefab, entity_root);
            view.Bind(state);
            view.name = state.name;
            entity_views.Add(view);
            return view;
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

        static void OnMessageLogged(string message) => Debug.Log(message);

        static int NewSeed() => System.Environment.TickCount & 0x7FFFFFFF;

        void OnGUI()
        {
            if (level == null) return;
            GUI.Label(
                new Rect(10, 10, 500, 20), 
            $"Seed: {level.seed}| Rooms: {level.rooms.Count} | Turn {turn_manager.turn_count} | " + 
                $"HP {level.player.hp}/{level.player.max_hp} | " + 
                $"WASD = move | SPACE = wait | [F5] Regenerate"
            );
        }
    }    
}