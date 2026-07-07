using Roguelike.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Roguelike.Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] MapRenderer map_renderer;
        [SerializeField] Camera camera;

        [Header("Generation")] 
        [SerializeField] GenerationSettings generation_settings = new GenerationSettings();

        [Tooltip("0 picks a random seed on each run.")] 
        [SerializeField] int seed = 0;
        
        public LevelData level { get; private set; }

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
        }

        void GenerateLevel(int seed)
        {
            level = MapGenerator.Generate(generation_settings, seed, 1);
            map_renderer.Render(level.map);
            FrameWholeMap();
            
            Debug.Log(
                $"Generated seed {seed}: {level.rooms.Count} rooms, " + $"{level.map.FloorTiles().Count} floor tiles."
            );
        }

        void FrameWholeMap()
        {
            if (camera == null) return;
            camera.transform.position = new Vector3(
                generation_settings.width / 2f,
                generation_settings.height / 2f,
                -10f
            );
            camera.orthographicSize = generation_settings.height / 2f + 2f;
        }

        static int NewSeed() => System.Environment.TickCount & 0x7FFFFFFF;

        void OnGUI()
        {
            if (level == null) return;
            GUI.Label(
                new Rect(10, 10, 500, 20), 
                $"Seed: {level.seed}| Rooms: {level.rooms.Count}| [F5] Regenerate"
            );
        }
    }    
}